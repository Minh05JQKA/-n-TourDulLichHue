using HueTour.CoreBusiness;
using HueTour.UseCases.Common;
using HueTour.UseCases.FoodPlaces;

namespace HueTour.UseCases.MapExplorer;

/// <summary>Combines the curated food catalogue with live OSM places for the shared map.</summary>
public sealed class MapPoiCatalogUseCase : IMapPoiCatalogUseCase
{
    private const double DefaultLatitude = 16.4637;
    private const double DefaultLongitude = 107.5909;
    private readonly IFoodPlaceCatalogUseCase _foodCatalog;
    private readonly IDiscoveryMapDataSource _liveSource;

    public MapPoiCatalogUseCase(IFoodPlaceCatalogUseCase foodCatalog, IDiscoveryMapDataSource liveSource)
    {
        _foodCatalog = foodCatalog;
        _liveSource = liveSource;
    }

    public async Task<MapPoiQueryResult> SearchAsync(MapPoiQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var hasOrigin = TryGetOrigin(query, out var latitude, out var longitude);
        if (!hasOrigin)
        {
            (latitude, longitude) = query.Bounds is { } bounds
                ? ((bounds.South + bounds.North) / 2, (bounds.West + bounds.East) / 2)
                : (DefaultLatitude, DefaultLongitude);
        }

        var warnings = new List<string>();
        IReadOnlyList<FoodPlace> foodPlaces = Array.Empty<FoodPlace>();
        IReadOnlyList<MapPoiDto> livePlaces = Array.Empty<MapPoiDto>();

        var foodTask = LoadFoodAsync(query, cancellationToken);
        var liveTask = query.IncludeLiveData
            ? LoadLiveAsync(latitude, longitude, cancellationToken)
            : Task.FromResult<IReadOnlyList<MapPoiDto>>(Array.Empty<MapPoiDto>());
        try
        {
            foodPlaces = await foodTask;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            warnings.Add("Danh mục ẩm thực tạm thời chưa tải được.");
        }

        try
        {
            livePlaces = await liveTask;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            warnings.Add(ex is InvalidOperationException
                ? ex.Message
                : "Dữ liệu OpenStreetMap tạm thời chưa phản hồi.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var combined = foodPlaces.Select(ToFoodPoi).ToList();
        foreach (var livePoi in livePlaces)
        {
            if (livePoi.Type == MapPoiType.Food && combined.Any(food => IsSamePlace(food, livePoi)))
            {
                continue;
            }

            combined.Add(livePoi);
        }

        foreach (var poi in combined)
        {
            poi.DistanceKm = hasOrigin
                ? GeoDistanceCalculator.CalculateKm(latitude, longitude, poi.Latitude, poi.Longitude)
                : null;
        }

        var geographicallyMatched = combined
            .Where(poi => IsInsideBounds(poi, query.Bounds))
            .Where(poi => !hasOrigin || !IsValidRadius(query.RadiusKm)
                || poi.DistanceKm <= query.RadiusKm)
            .ToList();

        var categories = geographicallyMatched
            .Where(poi => !string.IsNullOrWhiteSpace(poi.Category))
            .GroupBy(poi => (poi.Type, Name: poi.Category!.Trim()))
            .Select(group => new MapPoiCategory(group.Key.Type, group.Key.Name, group.Count()))
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var filtered = geographicallyMatched
            .Where(poi => IsTypeIncluded(poi.Type, query))
            .Where(poi => string.IsNullOrWhiteSpace(query.Category)
                || string.Equals(poi.Category, query.Category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(poi => MatchesKeyword(poi, query.Keyword))
            .OrderBy(poi => hasOrigin ? poi.DistanceKm : null)
            .ThenBy(poi => poi.Type)
            .ThenBy(poi => poi.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(1000)
            .ToList();

        return new MapPoiQueryResult
        {
            Items = filtered,
            Categories = categories,
            Warnings = warnings
        };
    }

    private async Task<IReadOnlyList<FoodPlace>> LoadFoodAsync(MapPoiQuery query, CancellationToken cancellationToken)
    {
        var result = await _foodCatalog.SearchAsync(new FoodPlaceQuery
        {
            BoundsWest = query.Bounds?.West,
            BoundsSouth = query.Bounds?.South,
            BoundsEast = query.Bounds?.East,
            BoundsNorth = query.Bounds?.North,
            Page = 1,
            PageSize = 120
        }, cancellationToken);

        return result.MapItems;
    }

    private Task<IReadOnlyList<MapPoiDto>> LoadLiveAsync(double latitude, double longitude, CancellationToken cancellationToken) =>
        _liveSource.GetNearbyAsync(latitude, longitude, cancellationToken);

    private static MapPoiDto ToFoodPoi(FoodPlace place)
    {
        var tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddTag(tags, "categoryCode", place.CategoryCode);
        AddTag(tags, "signatureDish", place.SignatureDish);
        AddTag(tags, "price", place.AveragePrice.HasValue ? place.PriceLabel : string.Empty);
        AddTag(tags, "rating", place.Rating.HasValue ? place.RatingLabel : string.Empty);
        AddTag(tags, "curated", place.IsCurated ? "Đã đối chiếu" : string.Empty);

        return new MapPoiDto
        {
            Id = "food/" + place.Id,
            Type = MapPoiType.Food,
            Name = place.Name,
            Category = place.Category,
            Address = place.Address,
            Description = place.Description,
            ImageUrl = place.ImageUrl,
            DetailUrl = place.MapDetailsUrl,
            Latitude = place.Latitude,
            Longitude = place.Longitude,
            DistanceKm = place.DistanceKm,
            Phone = place.Phone,
            SignatureDish = place.SignatureDish,
            PriceLabel = place.AveragePrice.HasValue ? place.PriceLabel : null,
            RatingLabel = place.Rating.HasValue ? place.RatingLabel : null,
            DataSource = place.IsCurated ? "Danh mục HueTour · đã đối chiếu" : "Danh mục HueTour",
            Tags = tags
        };
    }

    private static bool IsSamePlace(MapPoiDto curated, MapPoiDto live)
    {
        if (!string.Equals(NormalizeName(curated.Name), NormalizeName(live.Name), StringComparison.Ordinal))
        {
            return false;
        }

        return GeoDistanceCalculator.CalculateKm(curated.Latitude, curated.Longitude, live.Latitude, live.Longitude) <= 0.15;
    }

    private static string NormalizeName(string value) =>
        new(value.Normalize(System.Text.NormalizationForm.FormD)
            .Where(character => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());

    private static bool MatchesKeyword(MapPoiDto poi, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        var term = keyword.Trim();
        return Contains(poi.Name, term)
            || Contains(poi.Category, term)
            || Contains(poi.Address, term)
            || Contains(poi.Description, term)
            || Contains(poi.SignatureDish, term)
            || (poi.Tags?.Values.Any(value => Contains(value, term)) ?? false);
    }

    private static bool Contains(string? value, string term) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains(term, StringComparison.CurrentCultureIgnoreCase);

    private static bool IsInsideBounds(MapPoiDto poi, GeoViewport? bounds) => bounds is null
        || (poi.Longitude >= bounds.West && poi.Longitude <= bounds.East
            && poi.Latitude >= bounds.South && poi.Latitude <= bounds.North);

    private static bool IsTypeIncluded(MapPoiType type, MapPoiQuery query) => type switch
    {
        MapPoiType.Attraction => query.IncludeAttractions,
        MapPoiType.Food => query.IncludeFood,
        _ => query.IncludeUnknown
    };

    private static bool TryGetOrigin(MapPoiQuery query, out double latitude, out double longitude)
    {
        latitude = query.NearLatitude.GetValueOrDefault();
        longitude = query.NearLongitude.GetValueOrDefault();
        return query.NearLatitude.HasValue && query.NearLongitude.HasValue
            && double.IsFinite(latitude) && double.IsFinite(longitude)
            && latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }

    private static bool IsValidRadius(double? radiusKm) => radiusKm.HasValue
        && double.IsFinite(radiusKm.Value) && radiusKm.Value > 0;

    private static void AddTag(IDictionary<string, string> tags, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            tags[key] = value;
        }
    }
}
