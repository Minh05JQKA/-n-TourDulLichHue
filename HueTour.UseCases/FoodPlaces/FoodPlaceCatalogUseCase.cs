using HueTour.CoreBusiness;
using HueTour.UseCases.Common;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.FoodPlaces;

/// <summary>
/// Yêu cầu tra cứu danh mục địa điểm ẩm thực.
/// </summary>
public class FoodPlaceQuery
{
    public string? Keyword { get; set; }

    public string? CategoryCode { get; set; }

    public double? NearLatitude { get; set; }

    public double? NearLongitude { get; set; }

    public double? RadiusKm { get; set; }

    public double? BoundsWest { get; set; }

    public double? BoundsSouth { get; set; }

    public double? BoundsEast { get; set; }

    public double? BoundsNorth { get; set; }

    /// <summary>Số bản ghi trả về tối đa cho trang kết quả.</summary>
    public int PageSize { get; set; } = 24;

    public int Page { get; set; } = 1;
}

/// <summary>
/// Kết quả tra cứu địa điểm ẩm thực kèm thông tin phân trang và danh mục.
/// </summary>
public class FoodPlaceQueryResult
{
    public IReadOnlyList<FoodPlace> Items { get; set; } = Array.Empty<FoodPlace>();

    /// <summary>Toàn bộ kết quả đã lọc để bản đồ không bị giới hạn theo trang danh sách.</summary>
    public IReadOnlyList<FoodPlace> MapItems { get; set; } = Array.Empty<FoodPlace>();

    public IReadOnlyList<CategorySummary> Categories { get; set; } = Array.Empty<CategorySummary>();

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 24;

    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public string? ActiveCategoryCode { get; set; }

    public string? Keyword { get; set; }
}

/// <summary>Phạm vi địa lý của khung nhìn bản đồ.</summary>
public sealed record GeoViewport(double West, double South, double East, double North);

public interface IFoodPlaceCatalogUseCase
{
    Task<FoodPlaceQueryResult> SearchAsync(FoodPlaceQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodPlace>> GetFeaturedAsync(int take = 8, string? categoryCode = null, CancellationToken cancellationToken = default);

    Task<FoodPlace?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

/// <summary>
/// Đọc danh mục địa điểm ẩm thực từ kho dữ liệu và áp dụng quy tắc lọc/phân trang.
/// </summary>
public class FoodPlaceCatalogUseCase : IFoodPlaceCatalogUseCase
{
    private readonly IFoodPlaceRepository _repository;

    public FoodPlaceCatalogUseCase(IFoodPlaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<FoodPlaceQueryResult> SearchAsync(FoodPlaceQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        var keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword.Trim();
        var categoryCode = string.IsNullOrWhiteSpace(query.CategoryCode) ? null : query.CategoryCode.Trim();

        var bounds = TryCreateViewport(query);
        var candidates = (await _repository.SearchAsync(keyword, categoryCode, bounds)).ToList();
        var hasOrigin = TryGetSearchOrigin(query, out var latitude, out var longitude);
        var radiusKm = query.RadiusKm is { } radius && double.IsFinite(radius) && radius > 0
            ? radius
            : (double?)null;
        List<FoodPlace> matched;

        if (hasOrigin)
        {
            matched = candidates
                .Select(place => CopyWithDistance(place, GeoDistanceCalculator.CalculateKm(latitude, longitude, place.Latitude, place.Longitude)))
                .Where(place => !radiusKm.HasValue || place.DistanceKm <= radiusKm.Value)
                .OrderBy(place => place.DistanceKm)
                .ThenByDescending(place => place.IsCurated)
                .ThenByDescending(place => place.Rating ?? 0)
                .ThenBy(place => place.Id)
                .ToList();
        }
        else
        {
            matched = candidates.ToList();
        }

        var categories = await _repository.GetCategoriesAsync();

        int pageSize = Math.Clamp(query.PageSize, 6, 120);
        int totalPages = Math.Max(1, (int)Math.Ceiling(matched.Count / (double)pageSize));
        int page = Math.Clamp(query.Page, 1, totalPages);

        var items = matched
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new FoodPlaceQueryResult
        {
            Items = items,
            MapItems = matched,
            Categories = categories,
            TotalCount = matched.Count,
            Page = page,
            PageSize = pageSize,
            ActiveCategoryCode = categoryCode,
            Keyword = keyword
        };
    }

    /// <summary>
    /// Returns true when the next viewport moved or resized by at least the requested
    /// fraction of the previous viewport. This keeps small map drags from re-querying.
    /// </summary>
    public static bool IsViewportShiftSignificant(GeoViewport previous, GeoViewport next, double threshold = 0.1)
    {
        var previousWidth = previous.East - previous.West;
        var previousHeight = previous.North - previous.South;
        var nextWidth = next.East - next.West;
        var nextHeight = next.North - next.South;
        if (previousWidth <= 0 || previousHeight <= 0 || nextWidth <= 0 || nextHeight <= 0)
        {
            return true;
        }

        var previousCenterLongitude = (previous.West + previous.East) / 2;
        var previousCenterLatitude = (previous.South + previous.North) / 2;
        var nextCenterLongitude = (next.West + next.East) / 2;
        var nextCenterLatitude = (next.South + next.North) / 2;

        var change = Math.Max(
            Math.Max(Math.Abs(nextCenterLongitude - previousCenterLongitude) / previousWidth,
                Math.Abs(nextCenterLatitude - previousCenterLatitude) / previousHeight),
            Math.Max(Math.Abs(nextWidth - previousWidth) / previousWidth,
                Math.Abs(nextHeight - previousHeight) / previousHeight));
        return change >= threshold;
    }

    private static GeoViewport? TryCreateViewport(FoodPlaceQuery query)
    {
        if (query.BoundsWest is not { } west || query.BoundsSouth is not { } south
            || query.BoundsEast is not { } east || query.BoundsNorth is not { } north
            || !double.IsFinite(west) || !double.IsFinite(south)
            || !double.IsFinite(east) || !double.IsFinite(north)
            || west < -180 || east > 180 || south < -90 || north > 90
            || west >= east || south >= north)
        {
            return null;
        }

        return new GeoViewport(west, south, east, north);
    }

    private static bool TryGetSearchOrigin(FoodPlaceQuery query, out double latitude, out double longitude)
    {
        latitude = query.NearLatitude.GetValueOrDefault();
        longitude = query.NearLongitude.GetValueOrDefault();
        return query.NearLatitude.HasValue && query.NearLongitude.HasValue
            && double.IsFinite(latitude) && double.IsFinite(longitude)
            && latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }

    private static FoodPlace CopyWithDistance(FoodPlace place, double distanceKm) => new()
    {
        Id = place.Id,
        Slug = place.Slug,
        Name = place.Name,
        Category = place.Category,
        CategoryCode = place.CategoryCode,
        SignatureDish = place.SignatureDish,
        Description = place.Description,
        Address = place.Address,
        Ward = place.Ward,
        Latitude = place.Latitude,
        Longitude = place.Longitude,
        Phone = place.Phone,
        AveragePrice = place.AveragePrice,
        Rating = place.Rating,
        ReviewCount = place.ReviewCount,
        ImageUrl = place.ImageUrl,
        SourceImageUrl = place.SourceImageUrl,
        Source = place.Source,
        IsCurated = place.IsCurated,
        DistanceKm = distanceKm
    };

    public async Task<IReadOnlyList<FoodPlace>> GetFeaturedAsync(
        int take = 8,
        string? categoryCode = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetFeaturedAsync(take, categoryCode);
        return items.ToList();
    }

    public async Task<FoodPlace?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        return await _repository.GetBySlugAsync(slug);
    }
}
