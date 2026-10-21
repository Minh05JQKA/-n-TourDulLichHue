namespace HueTour.CoreBusiness;

/// <summary>
/// Địa điểm ẩm thực trong Huế (nhà hàng, quán cà phê, tiệm bánh, quán bar).
/// Dữ liệu được nạp từ tệp seed sinh bởi scripts/import_food_places.py.
/// </summary>
public class FoodPlace
{
    public int Id { get; set; }

    /// <summary>Định danh dùng để làm URL thân thiện, ví dụ "bun-bo-mu-roi-12".</summary>
    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Nhóm hiển thị, ví dụ "Nhà hàng", "Quán cà phê".</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Mã nhóm chuẩn hoá, ví dụ "restaurant", "cafe", "bakery".</summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>Món đặc trưng được suy ra từ tên quán, ví dụ "Bún bò Huế".</summary>
    public string SignatureDish { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    /// <summary>Khoảng cách từ tâm tìm kiếm hiện tại, tính bởi use case; không lưu trong cơ sở dữ liệu.</summary>
    public double? DistanceKm { get; set; }

    public string Phone { get; set; } = string.Empty;

    /// <summary>Giá tham khảo bằng VNĐ; null nghĩa là nguồn không có dữ liệu.</summary>
    public decimal? AveragePrice { get; set; }

    /// <summary>Điểm đánh giá 0-5; null nghĩa là nguồn không có dữ liệu.</summary>
    public double? Rating { get; set; }

    public int? ReviewCount { get; set; }

    /// <summary>Đường dẫn ảnh nội bộ đã tải về, ví dụ "/images/food/bun-bo-mu-roi-12.webp".</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Đường dẫn ảnh gốc trên nguồn của dự án mẫu, giữ để đối chiếu nguồn gốc.</summary>
    public string SourceImageUrl { get; set; } = string.Empty;

    /// <summary>Tệp dữ liệu gốc đã dùng để nạp bản ghi này.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>True nếu bản ghi có mô tả biên tập tay từ clean_46_places.json.</summary>
    public bool IsCurated { get; set; }

    /// <summary>Bản đồ OpenStreetMap để người dùng tra cứu chi tiết.</summary>
    public string MapDetailsUrl => "https://www.openstreetmap.org/search?query="
        + Uri.EscapeDataString($"{Name} {Address}".Trim());

    public string DirectionsUrl =>
        $"https://www.google.com/maps/dir/?api=1&destination={Latitude:0.0000},{Longitude:0.0000}";

    /// <summary>Nhãn giá hiển thị, ví dụ "50.000 VNĐ" hoặc "Chưa có dữ liệu giá".</summary>
    public string PriceLabel => AveragePrice.HasValue
        ? $"{AveragePrice.Value:N0} VNĐ"
        : "Chưa có dữ liệu giá";

    /// <summary>Nhãn đánh giá hiển thị, nêu rõ nguồn để không gây hiểu nhầm.</summary>
    public string RatingLabel => Rating.HasValue
        ? $"{Rating.Value:0.0}/5 · {ReviewCount ?? 0} đánh giá (nguồn mẫu)"
        : "Chưa có đánh giá";

    public bool HasCoordinates => Latitude is > 0 and < 90 && Longitude is > 0 and < 180;
}
