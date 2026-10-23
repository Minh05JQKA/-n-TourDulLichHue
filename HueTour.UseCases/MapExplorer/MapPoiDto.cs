using HueTour.UseCases.Common;

namespace HueTour.UseCases.MapExplorer;

public enum MapPoiType
{
    Attraction,
    Food,
    Unknown
}

/// <summary>Unified map point returned by the discovery use case.</summary>
public sealed class MapPoiDto
{
    public string Id { get; set; } = string.Empty;
    public MapPoiType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? DetailUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public string? Phone { get; set; }
    public string? OpeningHours { get; set; }
    public string? SignatureDish { get; set; }
    public string? PriceLabel { get; set; }
    public string? RatingLabel { get; set; }
    public string? DataSource { get; set; }
    public IReadOnlyDictionary<string, string>? Tags { get; set; }
}

public sealed record MapPoiCategory(MapPoiType Type, string Name, int Count);

public sealed class MapPoiQuery
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public double? NearLatitude { get; set; }
    public double? NearLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public GeoViewport? Bounds { get; set; }
    public bool IncludeLiveData { get; set; } = true;
    public bool IncludeAttractions { get; set; } = true;
    public bool IncludeFood { get; set; } = true;
    public bool IncludeUnknown { get; set; }
}

public sealed class MapPoiQueryResult
{
    public IReadOnlyList<MapPoiDto> Items { get; init; } = Array.Empty<MapPoiDto>();
    public IReadOnlyList<MapPoiCategory> Categories { get; init; } = Array.Empty<MapPoiCategory>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
    public bool IsPartial => Warnings.Count > 0;
}

/// <summary>Port for nearby live map data. The WebApp adapter supplies the OpenStreetMap implementation.</summary>
public interface IDiscoveryMapDataSource
{
    Task<IReadOnlyList<MapPoiDto>> GetNearbyAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}

public interface IMapPoiCatalogUseCase
{
    Task<MapPoiQueryResult> SearchAsync(MapPoiQuery query, CancellationToken cancellationToken = default);
}
