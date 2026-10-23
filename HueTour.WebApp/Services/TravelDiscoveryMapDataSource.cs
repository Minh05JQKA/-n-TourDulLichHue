using System.Globalization;
using HueTour.UseCases.Common;
using HueTour.UseCases.MapExplorer;
using Microsoft.Extensions.Caching.Memory;

namespace HueTour.WebApp.Services;

/// <summary>Adapts the existing live travel discovery service to the unified map input port.</summary>
public sealed class TravelDiscoveryMapDataSource : IDiscoveryMapDataSource
{
    private const string CachePrefix = "unified-map-live:";
    private const double ReuseRadiusKm = 3;
    private static readonly TimeSpan LiveDataTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly ITravelDiscoveryService _discoveryService;
    private readonly IMemoryCache _cache;
    private (double Lat, double Lng)? _lastCenter;
    private IReadOnlyList<MapPoiDto>? _lastResult;
    private DateTime _lastFetchedUtc;

    public TravelDiscoveryMapDataSource(ITravelDiscoveryService discoveryService, IMemoryCache cache)
    {
        _discoveryService = discoveryService;
        _cache = cache;
    }

    public async Task<IReadOnlyList<MapPoiDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        // A pan inside the same neighbourhood should not start another public API
        // request. Overpass already returns places in an 8.5 km radius, so reuse
        // the last answer while the new centre stays close to it.
        if (_lastResult is not null
            && _lastCenter is { } lastCenter
            && DateTime.UtcNow - _lastFetchedUtc < CacheDuration
            && GeoDistanceCalculator.CalculateKm(lastCenter.Lat, lastCenter.Lng, latitude, longitude) <= ReuseRadiusKm)
        {
            return _lastResult;
        }

        // Quantise the cache key to roughly one kilometre so small map pans reuse
        // the same public Overpass response instead of starting a new request.
        var cacheKey = string.Create(
            CultureInfo.InvariantCulture,
            $"{CachePrefix}{latitude:F2}:{longitude:F2}");
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<MapPoiDto>? cached))
        {
            _lastCenter = (latitude, longitude);
            _lastResult = cached;
            _lastFetchedUtc = DateTime.UtcNow;
            return cached!;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LiveDataTimeout);
        IReadOnlyList<DiscoveryPlace> places;
        try
        {
            places = await _discoveryService.SearchPlacesAsync(
                new TravelCoordinate(latitude, longitude), "all", timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "Nguồn OpenStreetMap phản hồi chậm. Bản đồ đang hiển thị dữ liệu HueTour; bạn có thể thử lại sau.");
        }

        var result = places.Select(ToMapPoi).ToList();
        _cache.Set(cacheKey, result, CacheDuration);
        _lastCenter = (latitude, longitude);
        _lastResult = result;
        _lastFetchedUtc = DateTime.UtcNow;
        return result;
    }

    private static MapPoiDto ToMapPoi(DiscoveryPlace place) => new()
    {
        Id = "osm/" + place.Id,
        Type = Classify(place.Category),
        Name = place.Name,
        Category = place.Category,
        Address = place.Address,
        Description = string.IsNullOrWhiteSpace(place.Cuisine) ? null : "Ẩm thực: " + place.Cuisine,
        ImageUrl = place.ImageUrl,
        DetailUrl = place.MapDetailsUrl,
        Latitude = place.Latitude,
        Longitude = place.Longitude,
        DistanceKm = place.DistanceKm,
        OpeningHours = place.OpeningHours,
        PriceLabel = string.IsNullOrWhiteSpace(place.PriceInformation) ? null : place.PriceInformation,
        RatingLabel = place.Rating.HasValue
            ? place.Rating.Value.ToString("0.0", CultureInfo.InvariantCulture) + "/5 · OpenStreetMap"
            : null,
        DataSource = "OpenStreetMap",
        Tags = string.IsNullOrWhiteSpace(place.Cuisine)
            ? null
            : new Dictionary<string, string> { ["cuisine"] = place.Cuisine }
    };

    private static MapPoiType Classify(string category)
    {
        if (category.Contains("Ăn uống", StringComparison.OrdinalIgnoreCase)
            || category.Contains("Cà phê", StringComparison.OrdinalIgnoreCase)
            || category.Contains("thư giãn", StringComparison.OrdinalIgnoreCase))
        {
            return MapPoiType.Food;
        }

        if (category.Contains("Lưu trú", StringComparison.OrdinalIgnoreCase))
        {
            return MapPoiType.Unknown;
        }

        return MapPoiType.Attraction;
    }
}
