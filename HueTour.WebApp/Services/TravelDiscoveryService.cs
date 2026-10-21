using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using HueTour.UseCases.Common;

namespace HueTour.WebApp.Services;

public sealed record TravelCoordinate(double Latitude, double Longitude);

public sealed record TravelDestination(string Name, TravelCoordinate Coordinate, bool IsCurrentPosition = false);

public sealed class DiscoveryPlace
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Cuisine { get; init; } = string.Empty;
    public string OpeningHours { get; init; } = string.Empty;
    public string PriceInformation { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double DistanceKm { get; init; }
    public string MapDetailsUrl { get; init; } = string.Empty;
    public double? Rating { get; init; }
}

public sealed record CurrentWeather(
    double TemperatureC,
    double FeelsLikeC,
    int HumidityPercent,
    double WindKmh,
    int WeatherCode);

public sealed record DailyWeather(
    DateOnly Date,
    double MinimumC,
    double MaximumC,
    int WeatherCode,
    int? PrecipitationChance);

public sealed record TravelWeather(CurrentWeather Current, IReadOnlyList<DailyWeather> Forecast);

public interface ITravelDiscoveryService
{
    Task<TravelDestination?> ResolveDestinationAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscoveryPlace>> SearchPlacesAsync(TravelCoordinate center, string category, CancellationToken cancellationToken = default);
    Task<TravelWeather> GetWeatherAsync(TravelCoordinate coordinate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Live discovery data: OSM/Nominatim for geocoding and places, and Open-Meteo for forecasts.
/// Requests are user-triggered and cached to keep traffic modest on public community services.
/// </summary>
public sealed class TravelDiscoveryService : ITravelDiscoveryService
{
    private const string DefaultNominatimEndpoint = "https://nominatim.openstreetmap.org/search";
    private const string DefaultFallbackGeocodingEndpoint = "https://geocoding-api.open-meteo.com/v1/search";
    private const string DefaultWeatherEndpoint = "https://api.open-meteo.com/v1/forecast";
    private static readonly string[] DefaultOverpassEndpoints =
    ["https://overpass-api.de/api/interpreter", "https://overpass.kumi.systems/api/interpreter"];
    private static readonly SemaphoreSlim GeocodingGate = new(1, 1);
    private static DateTime LastGeocodingRequestUtc = DateTime.MinValue;

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TravelDiscoveryService> _logger;
    private readonly string _nominatimEndpoint;
    private readonly string _fallbackGeocodingEndpoint;
    private readonly string _weatherEndpoint;
    private readonly string[] _overpassEndpoints;

    public TravelDiscoveryService(HttpClient httpClient, IMemoryCache cache, ILogger<TravelDiscoveryService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _nominatimEndpoint = configuration["TravelApis:NominatimEndpoint"] ?? DefaultNominatimEndpoint;
        _fallbackGeocodingEndpoint = configuration["TravelApis:FallbackGeocodingEndpoint"] ?? DefaultFallbackGeocodingEndpoint;
        _weatherEndpoint = configuration["TravelApis:WeatherEndpoint"] ?? DefaultWeatherEndpoint;
        _overpassEndpoints = configuration.GetSection("TravelApis:OverpassEndpoints").Get<string[]>() ?? DefaultOverpassEndpoints;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("HueTour.WebApp/1.0 (+https://github.com/Minh05JQKA/-n-TourDulLichHue)");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<TravelDestination?> ResolveDestinationAsync(string query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = (query ?? string.Empty).Trim();
        if (normalizedQuery.Length < 2)
            return null;

        var cacheKey = "travel-geocode:" + normalizedQuery.ToLowerInvariant();
        if (_cache.TryGetValue(cacheKey, out TravelDestination? cached))
            return cached!;

        // Huế is the site's home city, so resolve its well-known center locally. This avoids
        // showing a geocoder outage for the default search and avoids an unnecessary public API call.
        if (IsHueQuery(normalizedQuery))
        {
            var hue = new TravelDestination("Thành phố Huế", new TravelCoordinate(16.4637, 107.5909));
            _cache.Set(cacheKey, hue, TimeSpan.FromHours(24));
            return hue;
        }

        var destination = await ResolveWithNominatimAsync(normalizedQuery, cancellationToken);
        destination ??= await ResolveWithOpenMeteoAsync(RemoveAdministrativePrefix(normalizedQuery), cancellationToken);
        if (destination is not null)
        {
            _cache.Set(cacheKey, destination, TimeSpan.FromHours(24));
            return destination;
        }

        return null;
    }

    private async Task<TravelDestination?> ResolveWithNominatimAsync(string query, CancellationToken cancellationToken)
    {
        await GeocodingGate.WaitAsync(cancellationToken);
        try
        {
            var elapsed = DateTime.UtcNow - LastGeocodingRequestUtc;
            if (elapsed < TimeSpan.FromSeconds(1))
                await Task.Delay(TimeSpan.FromSeconds(1) - elapsed, cancellationToken);

            var uri = _nominatimEndpoint + "?format=jsonv2&limit=1&countrycodes=vn&accept-language=vi&q=" + Uri.EscapeDataString(query);
            LastGeocodingRequestUtc = DateTime.UtcNow;
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var result = document.RootElement.EnumerateArray().FirstOrDefault();
            if (result.ValueKind == JsonValueKind.Object
                && result.TryGetProperty("lat", out var latValue)
                && result.TryGetProperty("lon", out var lonValue)
                && double.TryParse(latValue.GetString(), CultureInfo.InvariantCulture, out var latitude)
                && double.TryParse(lonValue.GetString(), CultureInfo.InvariantCulture, out var longitude))
            {
                var displayName = result.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;
                if (string.IsNullOrWhiteSpace(displayName) && result.TryGetProperty("display_name", out var displayValue))
                    displayName = displayValue.GetString()?.Split(',').FirstOrDefault();

                return new TravelDestination(string.IsNullOrWhiteSpace(displayName) ? query : displayName, new TravelCoordinate(latitude, longitude));
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Nominatim could not resolve the requested destination.");
        }
        finally
        {
            GeocodingGate.Release();
        }

        return null;
    }

    private async Task<TravelDestination?> ResolveWithOpenMeteoAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        try
        {
            var uri = _fallbackGeocodingEndpoint
                + "?name=" + Uri.EscapeDataString(query)
                + "&count=10&language=vi&countryCode=VN&format=json";
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var result in results.EnumerateArray())
            {
                if (result.TryGetProperty("country_code", out var countryCode)
                    && !string.Equals(countryCode.GetString(), "vn", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!result.TryGetProperty("latitude", out var latitudeValue)
                    || !latitudeValue.TryGetDouble(out var latitude)
                    || !result.TryGetProperty("longitude", out var longitudeValue)
                    || !longitudeValue.TryGetDouble(out var longitude))
                    continue;

                var displayName = result.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;
                if (!string.IsNullOrWhiteSpace(displayName))
                    return new TravelDestination(displayName, new TravelCoordinate(latitude, longitude));
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Open-Meteo geocoding could not resolve the requested destination.");
        }

        return null;
    }

    private static string RemoveAdministrativePrefix(string query)
    {
        string[] prefixes = ["thành phố ", "tp. ", "tp ", "tỉnh ", "city of "];
        foreach (var prefix in prefixes)
        {
            if (query.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return query[prefix.Length..].Trim();
        }

        return query;
    }

    public async Task<IReadOnlyList<DiscoveryPlace>> SearchPlacesAsync(TravelCoordinate center, string category, CancellationToken cancellationToken = default)
    {
        var normalizedCategory = NormalizeCategory(category);
        var cacheKey = string.Create(CultureInfo.InvariantCulture, $"travel-places:{center.Latitude:F3}:{center.Longitude:F3}:{normalizedCategory}");
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<DiscoveryPlace>? cached))
            return cached!;

        var query = BuildOverpassQuery(center, normalizedCategory);
        foreach (var endpoint in _overpassEndpoints)
        {
            try
            {
                using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", query)]);
                using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                var places = ParsePlaces(document.RootElement, center)
                    .OrderBy(place => place.DistanceKm)
                    .Take(80)
                    .ToList();
                _cache.Set(cacheKey, places, TimeSpan.FromMinutes(15));
                return places;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "Overpass endpoint {Endpoint} failed; trying the next public endpoint.", endpoint);
            }
        }

        throw new InvalidOperationException("Nguồn bản đồ đang bận hoặc không phản hồi. Bạn có thể thử lại sau; thời tiết và cẩm nang vẫn hoạt động.");
    }

    public async Task<TravelWeather> GetWeatherAsync(TravelCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Create(CultureInfo.InvariantCulture, $"travel-weather:{coordinate.Latitude:F2}:{coordinate.Longitude:F2}");
        if (_cache.TryGetValue(cacheKey, out TravelWeather? cached))
            return cached!;

        var uri = string.Create(CultureInfo.InvariantCulture,
            $"{_weatherEndpoint}?latitude={coordinate.Latitude:F4}&longitude={coordinate.Longitude:F4}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&forecast_days=7&timezone=Asia%2FBangkok");
        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;
        var current = root.GetProperty("current");
        var currentWeather = new CurrentWeather(
            ReadDouble(current, "temperature_2m"),
            ReadDouble(current, "apparent_temperature"),
            (int)ReadDouble(current, "relative_humidity_2m"),
            ReadDouble(current, "wind_speed_10m"),
            (int)ReadDouble(current, "weather_code"));

        var daily = root.GetProperty("daily");
        var dates = daily.GetProperty("time").EnumerateArray().Select(value => DateOnly.Parse(value.GetString()!, CultureInfo.InvariantCulture)).ToArray();
        var lows = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        var highs = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        var codes = daily.GetProperty("weather_code").EnumerateArray().Select(value => value.GetInt32()).ToArray();
        var rain = daily.TryGetProperty("precipitation_probability_max", out var rainValues)
            ? rainValues.EnumerateArray().Select(value => value.ValueKind == JsonValueKind.Number ? (int?)value.GetInt32() : null).ToArray()
            : new int?[dates.Length];

        var forecast = dates.Select((date, index) => new DailyWeather(date, lows[index], highs[index], codes[index], rain[index])).ToList();
        var result = new TravelWeather(currentWeather, forecast);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
        return result;
    }

    private static string BuildOverpassQuery(TravelCoordinate center, string category)
    {
        var lat = center.Latitude.ToString("F6", CultureInfo.InvariantCulture);
        var lon = center.Longitude.ToString("F6", CultureInfo.InvariantCulture);
        var radius = category == "current" ? 4000 : 8500;
        var around = string.Create(CultureInfo.InvariantCulture, $"(around:{radius},{lat},{lon})");
        var selectors = category switch
        {
            "food" => new[]
            {
                "nwr[\"amenity\"~\"^(restaurant|fast_food|food_court|ice_cream)$\"][\"name\"]" + around + ";"
            },
            "cafe" => new[]
            {
                "nwr[\"amenity\"~\"^(cafe|bar|pub)$\"][\"name\"]" + around + ";"
            },
            "stay" => new[]
            {
                "nwr[\"tourism\"~\"^(hotel|guest_house|hostel|motel)$\"][\"name\"]" + around + ";"
            },
            "attraction" => new[]
            {
                "nwr[\"tourism\"~\"^(attraction|museum|gallery|viewpoint)$\"][\"name\"]" + around + ";",
                "nwr[\"amenity\"~\"^(theatre|cinema|library|arts_centre)$\"][\"name\"]" + around + ";",
                "nwr[\"shop\"=\"mall\"][\"name\"]" + around + ";",
                "nwr[\"historic\"][\"name\"]" + around + ";",
                "nwr[\"amenity\"=\"place_of_worship\"][\"name\"]" + around + ";"
            },
            _ => new[]
            {
                "nwr[\"amenity\"~\"^(restaurant|fast_food|food_court|ice_cream|cafe|bar|pub)$\"][\"name\"]" + around + ";",
                "nwr[\"tourism\"~\"^(attraction|museum|gallery|viewpoint|hotel|guest_house|hostel|motel)$\"][\"name\"]" + around + ";",
                "nwr[\"amenity\"~\"^(theatre|cinema|library|arts_centre)$\"][\"name\"]" + around + ";",
                "nwr[\"shop\"=\"mall\"][\"name\"]" + around + ";",
                "nwr[\"historic\"][\"name\"]" + around + ";",
                "nwr[\"amenity\"=\"place_of_worship\"][\"name\"]" + around + ";"
            }
        };

        return "[out:json][timeout:20];(\n" + string.Join("\n", selectors) + "\n);out center tags;";
    }

    private static IEnumerable<DiscoveryPlace> ParsePlaces(JsonElement root, TravelCoordinate center)
    {
        if (!root.TryGetProperty("elements", out var elements) || elements.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var element in elements.EnumerateArray())
        {
            if (!element.TryGetProperty("tags", out var tags) || !tags.TryGetProperty("name", out var nameValue))
                continue;

            var name = nameValue.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var latitude = ReadCoordinate(element, "lat", "center", "lat");
            var longitude = ReadCoordinate(element, "lon", "center", "lon");
            if (!latitude.HasValue || !longitude.HasValue)
                continue;

            var amenity = GetTag(tags, "amenity");
            var tourism = GetTag(tags, "tourism");
            var historic = GetTag(tags, "historic");
            var shop = GetTag(tags, "shop");
            var category = DescribeCategory(amenity, tourism, historic, shop);
            var address = GetTag(tags, "addr:full");
            if (string.IsNullOrWhiteSpace(address))
            {
                var addressParts = new[]
                {
                    GetTag(tags, "addr:housenumber"), GetTag(tags, "addr:street"),
                    GetTag(tags, "addr:suburb"), GetTag(tags, "addr:city")
                }.Where(value => !string.IsNullOrWhiteSpace(value));
                address = string.Join(", ", addressParts);
            }

            var type = element.TryGetProperty("type", out var typeValue) ? typeValue.GetString() : "node";
            var id = element.TryGetProperty("id", out var idValue) ? idValue.ToString() : string.Empty;
            var image = ResolveImage(tags);
            var rating = double.TryParse(GetTag(tags, "rating"), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedRating)
                ? parsedRating
                : (double?)null;
            var distance = GeoDistanceCalculator.CalculateKm(center.Latitude, center.Longitude, latitude.Value, longitude.Value);

            yield return new DiscoveryPlace
            {
                Id = type + "/" + id,
                Name = name,
                Category = category,
                Address = address,
                Cuisine = GetTag(tags, "cuisine"),
                OpeningHours = GetTag(tags, "opening_hours"),
                PriceInformation = FirstNonEmpty(GetTag(tags, "price_range"), GetTag(tags, "charge"), GetTag(tags, "fee")),
                ImageUrl = image,
                Latitude = latitude.Value,
                Longitude = longitude.Value,
                DistanceKm = distance,
                MapDetailsUrl = "https://www.openstreetmap.org/" + type + "/" + id,
                Rating = rating
            };
        }
    }

    private static double? ReadCoordinate(JsonElement element, string directName, string parentName, string nestedName)
    {
        if (element.TryGetProperty(directName, out var direct) && direct.ValueKind == JsonValueKind.Number)
            return direct.GetDouble();
        if (element.TryGetProperty(parentName, out var parent)
            && parent.TryGetProperty(nestedName, out var nested)
            && nested.ValueKind == JsonValueKind.Number)
            return nested.GetDouble();
        return null;
    }

    private static string ResolveImage(JsonElement tags)
    {
        var image = GetTag(tags, "image");
        if (Uri.TryCreate(image, UriKind.Absolute, out var imageUri)
            && (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp))
            return imageUri.ToString();

        var commonsFile = GetTag(tags, "wikimedia_commons");
        if (commonsFile.StartsWith("File:", StringComparison.OrdinalIgnoreCase))
            return "https://commons.wikimedia.org/wiki/Special:FilePath/" + Uri.EscapeDataString(commonsFile[5..]);
        return string.Empty;
    }

    private static string DescribeCategory(string amenity, string tourism, string historic, string shop)
    {
        if (amenity is "restaurant" or "fast_food" or "food_court" or "ice_cream") return "Ăn uống";
        if (amenity is "cafe" or "bar" or "pub") return "Cà phê & thư giãn";
        if (tourism is "hotel" or "guest_house" or "hostel" or "motel") return "Lưu trú";
        if (shop == "mall") return "Trong nhà · Trung tâm thương mại";
        if (tourism is "museum" or "gallery" || amenity is "theatre" or "cinema" or "library" or "arts_centre") return "Trong nhà · Văn hóa";
        if (!string.IsNullOrWhiteSpace(historic)) return "Di tích · Lịch sử";
        if (amenity == "place_of_worship") return "Tâm linh · Kiến trúc";
        if (tourism is "attraction" or "viewpoint") return "Tham quan · Cảnh quan";
        return FirstNonEmpty(tourism, amenity, historic, "Địa điểm");
    }

    private static string NormalizeCategory(string category) => category switch
    {
        "food" or "cafe" or "stay" or "attraction" or "current" => category,
        _ => "all"
    };

    private static string GetTag(JsonElement tags, string key) =>
        tags.TryGetProperty(key, out var value) ? value.GetString()?.Trim() ?? string.Empty : string.Empty;

    private static double ReadDouble(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0;

    private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static bool IsHueQuery(string value)
    {
        var simplified = value.Normalize(System.Text.NormalizationForm.FormD);
        var withoutMarks = new string(simplified.Where(character => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark).ToArray());
        return withoutMarks.Contains("hue", StringComparison.OrdinalIgnoreCase);
    }

}
