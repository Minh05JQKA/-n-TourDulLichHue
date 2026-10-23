using HueTour.CoreBusiness;

namespace HueTour.Plugins.DataStore.InMemory;

internal static class HueTourMarketCatalog
{
    private const string HueTripOUrl = "https://huetripo.hue.gov.vn/tour-du-lich/danh-sach-tour";
    private static readonly DateTime PriceVerifiedAt = new(2026, 10, 22);

    public static IEnumerable<Tour> Create()
    {
        yield return CreateTour(1002, "HT-MARKET-THANHTAN24",
            "[Huế] Huế – Suối khoáng nóng Thanh Tân – Huế",
            "Thiên nhiên & Nghỉ dưỡng", "1 Ngày", 950000,
            "Công ty CP Du lịch Đại Bàng", HueTripOUrl, "/images/thien-mu-2025.jpg",
            "Tour ghép khởi hành từ Huế, đưa bạn đến khu suối khoáng nóng Thanh Tân. Giá công bố là giá từ; lịch trình chi tiết và dịch vụ đi kèm cần hỏi đơn vị tổ chức.",
            "Suối khoáng nóng Thanh Tân; khởi hành từ Huế; tour ghép; giá tham khảo từ 950.000đ/khách.",
            "Huế -> Khu suối khoáng nóng Thanh Tân -> Huế. Hoạt động và dịch vụ cụ thể theo xác nhận của đơn vị tổ chức.", "Huế");

        yield return CreateTour(1003, "HT-MARKET-BACHMA25",
            "Huế – Vườn Quốc gia Bạch Mã – Hải Vọng Đài – Ngũ Hồ – Thác Đỗ Quyên",
            "Sinh thái & Đầm phá", "1 Ngày", 1050000,
            "Công ty CP Du lịch Đại Bàng", HueTripOUrl, "/images/bach-ma-waterfall.jpg",
            "Tour ghép một ngày từ Huế đến Vườn Quốc gia Bạch Mã, với các điểm được đơn vị niêm yết gồm Hải Vọng Đài, Ngũ Hồ và thác Đỗ Quyên.",
            "Vườn Quốc gia Bạch Mã; Hải Vọng Đài; Ngũ Hồ; thác Đỗ Quyên; giá tham khảo từ 1.050.000đ/khách.",
            "Huế -> Vườn Quốc gia Bạch Mã -> Hải Vọng Đài -> Ngũ Hồ -> Thác Đỗ Quyên -> Huế. Thứ tự và hoạt động theo chương trình nhà cung cấp.", "Huế");

        yield return CreateTour(1004, "HT-MARKET-TAMGIANG3D26",
            "Hoàng hôn trên phá Tam Giang – Vườn Quốc gia Bạch Mã",
            "Sinh thái & Đầm phá", "3 Ngày", 3990000,
            "Công ty TNHH Lữ hành An Tường Travel", HueTripOUrl, "/images/tam-giang-2024.jpg",
            "Hành trình ghép đoàn 3 ngày kết hợp phá Tam Giang và Vườn Quốc gia Bạch Mã; điểm đón được niêm yết gồm sân bay Phú Bài, ga Huế hoặc bến xe.",
            "Phá Tam Giang; Vườn Quốc gia Bạch Mã; tour ghép 3 ngày; giá tham khảo từ 3.990.000đ/khách.",
            "Sân bay Phú Bài / Ga Huế / Bến xe -> Phá Tam Giang -> Vườn Quốc gia Bạch Mã -> Kết thúc theo chương trình An Tường Travel.", "Sân bay Phú Bài / Ga Huế / Bến xe");

        yield return CreateTour(1005, "HT-MARKET-TAMGIANG2D27",
            "Cố đô Huế – Hoàng hôn trên phá Tam Giang – Hoàng cung – Lăng Khải Định – Chùa Thiên Mụ – Chợ Đông Ba",
            "Di tích Cố Đô", "2 Ngày 1 Đêm", 2890000,
            "Công ty TNHH Lữ hành An Tường Travel", HueTripOUrl, "/images/imperial-city-2025.jpg",
            "Tour ghép 2 ngày 1 đêm kết hợp di sản trung tâm Huế với trải nghiệm hoàng hôn trên phá Tam Giang. Giá công bố là giá từ.",
            "Hoàng cung; lăng Khải Định; chùa Thiên Mụ; chợ Đông Ba; hoàng hôn phá Tam Giang; giá tham khảo từ 2.890.000đ/khách.",
            "Sân bay Phú Bài / Ga Huế / Bến xe -> Hoàng cung -> Lăng Khải Định -> Chùa Thiên Mụ -> Chợ Đông Ba -> Phá Tam Giang ngắm hoàng hôn. Lịch từng ngày theo đơn vị tổ chức.", "Sân bay Phú Bài / Ga Huế / Bến xe");

        yield return CreateTour(1006, "HT-MARKET-TAMGIANG-SUNSET28",
            "Trải nghiệm trên phá Tam Giang – Ngắm hoàng hôn",
            "Sinh thái & Đầm phá", "Nửa Ngày", 850000,
            "Công ty TNHH MTV Restour", HueTripOUrl, "/images/tam-giang-2024.jpg",
            "Tour ghép nửa ngày khởi hành từ thành phố Huế. Lịch trình chi tiết, hoạt động trên phá và giờ khởi hành cần xác nhận với Restour.",
            "Phá Tam Giang; trải nghiệm ngắm hoàng hôn; tour ghép nửa ngày; giá tham khảo từ 850.000đ/khách.",
            "Thành phố Huế -> Phá Tam Giang -> Ngắm hoàng hôn -> Trở về Huế. Chi tiết theo chương trình Restour.", "Thành phố Huế");

        yield return CreateTour(1007, "HT-MARKET-SONGHUONG-TEA29",
            "Trà chiều trên sông Hương – Du thuyền ngắm hoàng hôn",
            "Sông Hương & Làng nghề", "Nửa Ngày", 450000,
            "Công ty TNHH MTV Restour", HueTripOUrl, "/images/thien-mu-2025.jpg",
            "Trải nghiệm ghép nửa ngày trên sông Hương, kết hợp trà chiều và ngắm hoàng hôn. Giá và giờ chạy được niêm yết ở mức khởi điểm; cần xác nhận với Restour.",
            "Du thuyền sông Hương; trà chiều; ngắm hoàng hôn; giá tham khảo từ 450.000đ/khách.",
            "Thành phố Huế -> Du thuyền trên sông Hương -> Trải nghiệm trà chiều và ngắm hoàng hôn -> Kết thúc theo chương trình Restour.", "Thành phố Huế");

        yield return CreateTour(1008, "HT-MARKET-THUYBIEU30",
            "Khám phá Thủy Biều",
            "Sông Hương & Làng nghề", "Nửa Ngày", 850000,
            "Công ty TNHH MTV Restour", HueTripOUrl, "/images/thien-mu-2025.jpg",
            "Tour ghép nửa ngày khám phá khu vực Thủy Biều, khởi hành từ thành phố Huế. Nội dung trải nghiệm cụ thể cần xác nhận với Restour trước ngày đi.",
            "Thủy Biều; tour ghép nửa ngày; khởi hành từ thành phố Huế; giá tham khảo từ 850.000đ/khách.",
            "Thành phố Huế -> Khám phá Thủy Biều -> Trở về Huế. Các điểm dừng theo chương trình Restour.", "Thành phố Huế");

        yield return CreateTour(1009, "HT-MARKET-FOODTOUR31",
            "Khám phá ẩm thực xứ Huế – Hue Food Tour",
            "Văn hóa & Ẩm thực", "Nửa Ngày", 850000,
            "Công ty TNHH MTV Restour", HueTripOUrl, "/images/bun-bo-hue-2024.jpg",
            "Tour tự chọn nửa ngày khám phá ẩm thực Huế, khởi hành từ thành phố Huế. Món ăn, điểm dừng và nội dung bao gồm cần xác nhận với Restour.",
            "Ẩm thực Huế; tour tự chọn; thời lượng nửa ngày; giá tham khảo từ 850.000đ/khách.",
            "Thành phố Huế -> Các trải nghiệm ẩm thực theo chương trình Hue Food Tour -> Kết thúc tại Huế.", "Thành phố Huế");

        yield return CreateTour(1010, "HT-MARKET-VIETRAVEL32",
            "Huế – Đại Nội – Lăng Khải Định",
            "Di tích Cố Đô", "1 Ngày", 890000,
            "Công ty TNHH MTV Du lịch Vietravel Huế", HueTripOUrl, "/images/imperial-city-2025.jpg",
            "Tour ghép một ngày khởi hành tại địa chỉ 01 Hùng Vương, thành phố Huế. Giá công bố là giá từ; lịch và các dịch vụ bao gồm cần xác nhận với Vietravel Huế.",
            "Đại Nội Huế; lăng Khải Định; tour ghép một ngày; điểm khởi hành 01 Hùng Vương; giá tham khảo từ 890.000đ/khách.",
            "01 Hùng Vương, TP Huế -> Đại Nội Huế -> Lăng Khải Định -> Kết thúc theo lịch trình Vietravel Huế.", "01 Hùng Vương, TP Huế");

        yield return CreateTour(1011, "HT-MARKET-BAOVINH33",
            "Huế – Phố cổ Bao Vinh – Tranh làng Sình – Làng hoa giấy Thanh Tiên – Bữa trưa dân dã",
            "Văn hóa & Ẩm thực", "Nửa Ngày", 750000,
            "Công ty CP Du lịch Đại Bàng", HueTripOUrl, "/images/dong-ba-market-2024.jpg",
            "Tour ghép nửa ngày từ thành phố Huế, kết nối phố cổ Bao Vinh với tranh làng Sình và làng hoa giấy Thanh Tiên. Giá công bố là giá từ.",
            "Phố cổ Bao Vinh; tranh làng Sình; làng hoa giấy Thanh Tiên; bữa trưa dân dã; giá tham khảo từ 750.000đ/khách.",
            "Thành phố Huế -> Phố cổ Bao Vinh -> Tranh làng Sình -> Làng hoa giấy Thanh Tiên -> Bữa trưa dân dã. Thứ tự theo chương trình đơn vị tổ chức.", "Thành phố Huế");

        yield return CreateTour(1012, "HT-MARKET-HITOUR1D34",
            "Tour du lịch Huế trong ngày – Thiên Mụ, Đại Nội, hai lăng và làng nghề",
            "Di tích Cố Đô", "1 Ngày", 850000,
            "Hitour", "https://hitour.vn/tour-du-lich-hue/tour-du-lich-hue-1-ngay-tu-thua-thien-hue", "/images/imperial-city-2025.jpg",
            "Tour khởi hành hằng ngày từ Huế, niêm yết giá áp dụng từ 01/01 đến 31/12/2026. Lịch trình gồm chùa Thiên Mụ, Đại Nội, lăng Minh Mạng, lăng Khải Định, làng nón và làng hương; giá người lớn từ 850.000đ.",
            "Khởi hành hằng ngày; chùa Thiên Mụ; Đại Nội; lăng Minh Mạng; lăng Khải Định; làng nón; làng hương; giá niêm yết 2026.",
            "08:00 Đón tại khách sạn trung tâm Huế -> Chùa Thiên Mụ -> Đại Nội -> Chợ Đông Ba -> Lăng Minh Mạng -> Lăng Khải Định -> Làng nón, làng hương -> 16:30 Trả khách.", "Trung tâm thành phố Huế");
    }

    private static Tour CreateTour(int id, string code, string title, string category, string duration,
        decimal price, string provider, string sourceUrl, string imageUrl, string description,
        string highlights, string itinerary, string startingLocation) => new()
    {
        Id = id,
        Code = code,
        Title = title,
        Category = category,
        ShortDescription = description,
        Description = $"Thông tin tour được tổng hợp từ nguồn công khai ngày 22/10/2026. Mức giá là giá khởi điểm do đơn vị niêm yết, không phải xác nhận chỗ đặt. Vui lòng liên hệ đơn vị tổ chức để kiểm tra giá cuối cùng, lịch khởi hành, chỗ trống và dịch vụ bao gồm.\n\nĐơn vị: {provider}.",
        Itinerary = itinerary,
        Highlights = highlights,
        Duration = duration,
        StartingLocation = startingLocation,
        BasePrice = price,
        ImageUrl = imageUrl,
        ProviderName = provider,
        SourceUrl = sourceUrl,
        PriceVerifiedAt = PriceVerifiedAt,
        IsActive = true,
        CreatedAt = PriceVerifiedAt
    };
}
