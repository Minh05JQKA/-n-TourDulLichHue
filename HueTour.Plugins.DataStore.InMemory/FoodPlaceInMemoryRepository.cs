using System.Reflection;
using System.Text.Json;
using HueTour.CoreBusiness;
using HueTour.UseCases.FoodPlaces;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

/// <summary>
/// Kho địa điểm ẩm thực chạy trong bộ nhớ. Dữ liệu được nhúng sẵn trong
/// assembly dưới dạng embedded resource <c>dia-diem-am-thuc.json</c>, sinh ra bởi
/// <c>scripts/import_food_places.py</c> từ dự án mẫu HUEGO.
/// </summary>
public class FoodPlaceInMemoryRepository : IFoodPlaceRepository
{
    private const string ResourceName = "HueTour.Plugins.DataStore.InMemory.Data.dia-diem-am-thuc.json";

    private static readonly string[] CategoryOrder =
    {
        "restaurant", "cafe", "bakery", "noodle", "bar", "dessert", "hotpot", "grill", "seafood", "vegetarian"
    };

    private readonly Lazy<IReadOnlyList<FoodPlace>> _places = new(LoadPlaces, LazyThreadSafetyMode.ExecutionAndPublication);

    private IReadOnlyList<FoodPlace> Places => _places.Value;

    public Task<IEnumerable<FoodPlace>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<FoodPlace>>(Places);
    }

    public Task<FoodPlace?> GetByIdAsync(int id)
    {
        return Task.FromResult(Places.FirstOrDefault(p => p.Id == id));
    }

    public Task<FoodPlace?> GetBySlugAsync(string slug)
    {
        return Task.FromResult(Places.FirstOrDefault(p =>
            string.Equals(p.Slug, slug?.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    public Task<IEnumerable<FoodPlace>> SearchAsync(string? keyword, string? categoryCode, GeoViewport? bounds = null)
    {
        IEnumerable<FoodPlace> query = Places;

        if (bounds is not null)
        {
            query = query.Where(p => p.Longitude >= bounds.West && p.Longitude <= bounds.East
                && p.Latitude >= bounds.South && p.Latitude <= bounds.North);
        }

        if (!string.IsNullOrWhiteSpace(categoryCode))
        {
            query = query.Where(p => string.Equals(p.CategoryCode, categoryCode.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var terms = keyword.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToArray();

            query = query.Where(p => terms.All(term =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Address.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Ward.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.SignatureDish.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        return Task.FromResult(query);
    }

    public Task<IReadOnlyList<CategorySummary>> GetCategoriesAsync()
    {
        var summaries = Places
            .GroupBy(p => new { p.CategoryCode, p.Category })
            .Select(g => new CategorySummary
            {
                CategoryCode = g.Key.CategoryCode,
                DisplayName = g.Key.Category,
                Count = g.Count()
            })
            .OrderBy(s => Array.IndexOf(CategoryOrder, s.CategoryCode) is var index && index >= 0 ? index : int.MaxValue)
            .ThenByDescending(s => s.Count)
            .ToList();

        return Task.FromResult<IReadOnlyList<CategorySummary>>(summaries);
    }

    public Task<IEnumerable<FoodPlace>> GetFeaturedAsync(int take = 6, string? categoryCode = null)
    {
        IEnumerable<FoodPlace> query = Places;

        if (!string.IsNullOrWhiteSpace(categoryCode))
        {
            query = query.Where(p => string.Equals(p.CategoryCode, categoryCode.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var featured = query
            .OrderByDescending(p => p.IsCurated)
            .ThenByDescending(p => p.Rating ?? 0)
            .ThenByDescending(p => p.ReviewCount ?? 0)
            .Take(Math.Max(1, take))
            .ToList();

        return Task.FromResult<IEnumerable<FoodPlace>>(featured);
    }

    private static IReadOnlyList<FoodPlace> LoadPlaces()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            return Array.Empty<FoodPlace>();
        }

        using var reader = new StreamReader(stream);
        using var document = JsonDocument.Parse(reader.ReadToEnd());

        if (!document.RootElement.TryGetProperty("DiaDiem", out var items))
        {
            return Array.Empty<FoodPlace>();
        }

        var results = new List<FoodPlace>(items.GetArrayLength());
        foreach (var item in items.EnumerateArray())
        {
            results.Add(new FoodPlace
            {
                Id = GetInt(item, "MaDiaDiem"),
                Slug = GetString(item, "Slug"),
                Name = GetString(item, "TenDiaDiem"),
                Category = GetString(item, "DanhMuc"),
                CategoryCode = GetString(item, "MaDanhMuc"),
                SignatureDish = GetString(item, "MonDacTrung"),
                Description = GetString(item, "MoTa"),
                Address = GetString(item, "DiaChi"),
                Ward = GetString(item, "PhuongXa"),
                Latitude = GetDouble(item, "ToaDoLat"),
                Longitude = GetDouble(item, "ToaDoLng"),
                Phone = GetString(item, "SoDienThoai"),
                AveragePrice = GetNullableDouble(item, "GiaThamKhao") is { } price
                    ? (decimal)price
                    : null,
                Rating = GetNullableDouble(item, "DiemDanhGia"),
                ReviewCount = GetNullableInt(item, "SoDanhGia"),
                ImageUrl = GetString(item, "AnhMinhHoa"),
                SourceImageUrl = GetString(item, "AnhGoc"),
                Source = GetString(item, "Nguon"),
                IsCurated = GetBool(item, "DuocDoiChieu")
            });
        }

        return results;
    }

    private static string GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static int GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : 0;

    private static double GetDouble(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)
            ? number
            : 0d;

    private static double? GetNullableDouble(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    private static int? GetNullableInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;

    private static bool GetBool(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value)
        && value.ValueKind is JsonValueKind.True or JsonValueKind.String
        && (value.ValueKind == JsonValueKind.True
            || string.Equals(value.GetString(), "true", StringComparison.OrdinalIgnoreCase));
}
