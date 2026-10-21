using HueTour.CoreBusiness;
using HueTour.UseCases.FoodPlaces;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

/// <summary>
/// Kho dữ liệu địa điểm ẩm thực Huế (nhà hàng, quán cà phê, tiệm bánh...).
/// Dữ liệu được nạp từ tệp seed sinh bởi <c>scripts/import_food_places.py</c>.
/// </summary>
public interface IFoodPlaceRepository
{
    Task<IEnumerable<FoodPlace>> GetAllAsync();

    Task<FoodPlace?> GetByIdAsync(int id);

    Task<FoodPlace?> GetBySlugAsync(string slug);

    /// <summary>Tìm theo từ khoá trên tên, món đặc trưng, địa chỉ và phường.</summary>
    Task<IEnumerable<FoodPlace>> SearchAsync(string? keyword, string? categoryCode, GeoViewport? bounds = null);

    /// <summary>Các nhóm danh mục đang có dữ liệu, kèm số lượng tương ứng.</summary>
    Task<IReadOnlyList<CategorySummary>> GetCategoriesAsync();

    /// <summary>Những quán được xếp hạng cao nhất trong một nhóm danh mục.</summary>
    Task<IEnumerable<FoodPlace>> GetFeaturedAsync(int take = 6, string? categoryCode = null);
}

/// <summary>Tóm tắt số lượng địa điểm theo nhóm danh mục.</summary>
public class CategorySummary
{
    public string CategoryCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int Count { get; set; }
}
