using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class TourInMemoryRepository : ITourRepository
{
    private readonly List<Tour> _tours;

    public TourInMemoryRepository()
    {
        _tours = new List<Tour>
        {
            new()
            {
                Id = 1,
                Code = "HT-DAINOI01",
                Title = "Tour Khám Phá Di Sản Hoàng Cung Đại Nội & Các Lăng Tẩm Triều Nguyễn",
                Category = "Di tích Cố Đô",
                ShortDescription = "Hành trình xuyên qua 143 năm vàng son triều Nguyễn: Ngọ Môn, Điện Thái Hòa, Lăng Khải Định, Lăng Tự Đức.",
                Description = "Trải nghiệm văn hóa di sản hoàng gia trọn vẹn trong ngày. Bạn sẽ được hướng dẫn viên thuyết minh chi tiết về điển tích, nghệ thuật kiến trúc cung đình đỉnh cao, thưởng ngoạn cảnh quan sơn thủy hữu tình.",
                Itinerary = "08:00 Đón khách tại trung tâm TP Huế -> 08:30 Tham quan Đại Nội -> 11:30 Cơm niêu Cố Đô -> 13:30 Viếng Lăng Khải Định -> 15:30 Chiêm bái Lăng Tự Đức -> 17:00 Trả khách.",
                Highlights = "Vé vào cổng trọn gói các điểm di tích; Xe du lịch máy lạnh; Thuyết minh viên chuyên nghiệp; Ăn trưa đặc sản Huế.",
                Duration = "1 Ngày",
                StartingLocation = "Cột cờ Phu Văn Lâu, TP Huế",
                BasePrice = 550000,
                ImageUrl = "/images/imperial-city-2025.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 2,
                Code = "HT-TAMGIANG02",
                Title = "Tour Hoàng Hôn Đầm Phá Tam Giang & Trải Nghiệm Chèo Sup Đầm Chuồn",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Ngắm nhìn phá Tam Giang lung linh trong ánh hoàng hôn tím, chèo Sup thư giãn và thưởng thức hải sản đầm Chuồn tươi sống.",
                Description = "Rời xa khói bụi thành phố, cùng ngư dân lênh đênh trên mặt nước mênh mông, đổ nò bắt tôm cá, chèo Sup thư giãn và thưởng thức tiệc hải sản tươi rói ngay tại nhà chồ giữa đầm.",
                Itinerary = "14:00 Xe đón khách -> 14:45 Lên thuyền tham quan đầm Chuồn -> 15:30 Trải nghiệm đổ nò, quăng chài cùng ngư dân -> 16:30 Chèo Sup ngắm hoàng hôn -> 18:00 Tiệc hải sản -> 19:30 Về Huế.",
                Highlights = "Thuyền du lịch an toàn; Chèo Sup và áo phao miễn phí; Bữa tối hải sản đầm phá cực ngon; Chụp ảnh hoàng hôn tuyệt mỹ.",
                Duration = "Nửa Ngày (Chiều tối)",
                StartingLocation = "Bến thuyền Đầm Chuồn, Phú Vang",
                BasePrice = 480000,
                ImageUrl = "/images/tam-giang-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 3,
                Code = "HT-SONGHUONG03",
                Title = "Du Thuyền Sông Hương Ngắm Cố Đô & Nghe Ca Huế Thính Phòng Về Đêm",
                Category = "Sông Hương & Làng nghề",
                ShortDescription = "Thả hồn theo dòng Hương giang thơ mộng trên thuyền rồng đôi, lắng nghe điệu Nam ai, Nam bình và thả hoa đăng cầu an.",
                Description = "Ca Huế là Di sản văn hóa phi vật thể quốc gia. Trong không gian tĩnh lặng lung linh ánh đèn của cầu Tràng Tiền, những giai điệu nhã nhạc thính phòng đưa bạn về với hoài niệm Cố Đô sâu lắng.",
                Itinerary = "19:00 Đón khách tại bến Tòa Khâm -> 19:30 Thuyền rồng rời bến xuôi dòng Hương qua cầu Tràng Tiền -> 20:00 Đêm biểu diễn Ca Huế mộc -> 20:45 Trải nghiệm thả hoa đăng giấy màu -> 21:15 Cập bến.",
                Highlights = "Vé thuyền rồng đôi ngắm sông Hương; Đoàn nghệ nhân biểu diễn Ca Huế; Mỗi khách tặng hoa đăng thả sông; Thưởng trà cung đình.",
                Duration = "3 Giờ",
                StartingLocation = "Bến thuyền Tòa Khâm, 49 Lê Lợi, TP Huế",
                BasePrice = 220000,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e1/Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg/1280px-Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 4,
                Code = "HT-THUYBIEU04",
                Title = "Tour Sinh Thái Làng Cổ Thủy Biều & Ngâm Chân Thảo Dược Thanh Trà",
                Category = "Sông Hương & Làng nghề",
                ShortDescription = "Đạp xe dưới bóng râm mát rượi của những vườn thanh trà trĩu quả, tham quan nhà rường cổ và ngâm chân thảo mộc lá bưởi.",
                Description = "Hành trình chữa lành và thư giãn tuyệt đối bên dòng sông Hương. Khám phá nét đẹp mộc mạc của làng cổ Thủy Biều, học làm mè xửng, làm bánh gói truyền thống Huế.",
                Itinerary = "08:30 Đón khách tại chùa Thiên Mụ -> 09:00 Thuyền lên Thủy Biều -> 09:30 Đạp xe quanh đường làng -> 10:30 Trải nghiệm làm bánh lọc -> 11:30 Thư giãn ngâm chân thảo dược lá bưởi -> 12:30 Bữa trưa nhà vườn.",
                Highlights = "Thuyền đưa đón dọc sông Hương; Xe đạp miễn phí suốt hành trình; Ngâm chân nước ấm thảo dược vườn nhà; Thưởng thức thanh trà và bữa trưa quê.",
                Duration = "Nửa Ngày",
                StartingLocation = "Chùa Thiên Mụ / Làng Thủy Biều",
                BasePrice = 390000,
                ImageUrl = "/images/thien-mu-2025.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 5,
                Code = "HT-AMTHUCDEM05",
                Title = "Food Tour Ẩm Thực Cố Đô Đêm Bằng Xe Xích Lô Truyền Thống",
                Category = "Văn hóa & Ẩm thực",
                ShortDescription = "Ngồi xích lô dạo qua 36 phố phường đêm Huế, nếm thử 10 món ăn đặc sản trứ danh: bánh bèo, nậm, lọc, bún bò và chè hẻm.",
                Description = "Huế là cái nôi ẩm thực tinh hoa của Việt Nam. Chuyến đi đưa bạn đến đúng những quán ăn gia truyền có tuổi đời hàng chục năm, nếm vị đậm đà cay nồng đúng chất Huế.",
                Itinerary = "17:30 Xích lô đón tại khách sạn -> 18:00 Thưởng thức Bánh bèo - nậm - lọc bà Đỏ -> 19:00 Bún bò mụ Rơi nổi tiếng -> 20:00 Bánh ép Thuận An -> 20:45 Chè hẻm Cung Đình 20 món -> 21:30 Về lại điểm hẹn.",
                Highlights = "Xe xích lô và tài xế suốt buổi tối; Trọn gói chi phí thưởng thức 10 món ăn; Hướng dẫn viên bản địa sành ăn dẫn đường.",
                Duration = "4 Giờ",
                StartingLocation = "Đón tại các khách sạn trung tâm TP Huế",
                BasePrice = 420000,
                ImageUrl = "/images/bun-bo-hue-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 6,
                Code = "HT-BACHMA06",
                Title = "Tour Trekking Vườn Quốc Gia Bạch Mã & Chinh Phục Ngũ Hồ - Hải Vọng Đài",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Khám phá thiên nhiên kỳ vĩ tại Bạch Mã, tắm suối mát lạnh ở cụm Ngũ Hồ và chiêm ngưỡng toàn cảnh phá Cầu Hai từ Hải Vọng Đài.",
                Description = "Dành cho những tâm hồn yêu thiên nhiên hoang sơ và vận động. Khí hậu mát lạnh quanh năm tựa Đà Lạt giữa lòng Cố Đô, ngắm thác Đỗ Quyên hùng vĩ cao hơn 300 mét.",
                Itinerary = "07:30 Xe đón tại trung tâm Huế -> 09:00 Chân núi Bạch Mã -> 10:00 Chinh phục Hải Vọng Đài 1.450m -> 11:30 Trekking Ngũ Hồ, ăn trưa picnic -> 13:30 Ngắm thác Đỗ Quyên -> 15:30 Xuống núi -> 17:30 Về TP Huế.",
                Highlights = "Xe trung chuyển hai chiều và xe lên đỉnh; Vé tham quan vườn quốc gia; Bữa trưa picnic thơm ngon; Hướng dẫn viên trekking có kinh nghiệm.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 680000,
                ImageUrl = "/images/bach-ma-waterfall.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 7,
                Code = "HT-TUDUC07",
                Title = "Tour Lăng Tự Đức – Làng Hương Thủy Xuân – Đồi Vọng Cảnh",
                Category = "Di tích Cố Đô",
                ShortDescription = "Khám phá Lăng Tự Đức, sắc màu làng hương Thủy Xuân và ngắm sông Hương từ đồi Vọng Cảnh.",
                Description = "Hành trình kết hợp di sản và làng nghề truyền thống. Du khách tham quan lăng Tự Đức, tìm hiểu nghề làm hương tại Thủy Xuân và dừng chân ngắm cảnh sông Hương từ đồi Vọng Cảnh.",
                Itinerary = "08:00 Đón khách tại trung tâm Huế -> 08:30 Tham quan Lăng Tự Đức -> 10:30 Ghé làng hương Thủy Xuân, trò chuyện cùng nghệ nhân -> 11:30 Ngắm cảnh tại đồi Vọng Cảnh -> 12:30 Trả khách tại trung tâm.",
                Highlights = "Di tích Lăng Tự Đức; Trải nghiệm làng nghề làm hương; Ngắm sông Hương từ đồi Vọng Cảnh; Hướng dẫn viên địa phương.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 520000,
                ImageUrl = "/images/tu-duc-tomb.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 8,
                Code = "HT-THANHTOAN08",
                Title = "Tour Cầu Ngói Thanh Toàn – Chợ Quê – Làng Hoa Giấy Thanh Tiên",
                Category = "Sông Hương & Làng nghề",
                ShortDescription = "Thăm cây cầu ngói cổ, khám phá nhịp sống chợ quê và tìm hiểu nghề làm hoa giấy truyền thống xứ Huế.",
                Description = "Một buổi khám phá vùng quê Huế với kiến trúc cầu ngói Thanh Toàn, không gian chợ quê và hoạt động tìm hiểu nghề làm hoa giấy tại làng Thanh Tiên.",
                Itinerary = "08:00 Đón khách tại trung tâm Huế -> 08:45 Tham quan Cầu Ngói Thanh Toàn và nhà trưng bày nông cụ -> 10:00 Dạo chợ quê -> 10:45 Đến làng Thanh Tiên, tìm hiểu cách làm hoa giấy -> 12:00 Thưởng thức bữa trưa địa phương -> 13:00 Trở về trung tâm.",
                Highlights = "Cầu ngói Thanh Toàn; Không gian chợ quê; Gặp gỡ nghệ nhân làng hoa giấy Thanh Tiên; Thưởng thức món ăn địa phương.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 450000,
                ImageUrl = "/images/thanh-toan-2018.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 9,
                Code = "HT-LANGCO09",
                Title = "Tour Biển Lăng Cô – Đầm Lập An – Hải Vân Quan",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Kết hợp biển xanh Lăng Cô, cảnh đầm Lập An dưới chân núi và điểm dừng ngắm cảnh Hải Vân Quan.",
                Description = "Khám phá vùng ven biển phía nam Huế với các điểm dừng nổi bật: ngắm cảnh đầm Lập An, thư giãn tại vịnh Lăng Cô và tham quan Hải Vân Quan. Lịch trình có thời gian di chuyển đường dài, phù hợp cho chuyến đi trong ngày.",
                Itinerary = "07:00 Đón khách tại trung tâm Huế -> 08:30 Dừng ngắm cảnh tại Hải Vân Quan -> 10:00 Tham quan đầm Lập An -> 11:30 Ăn trưa hải sản tại Lăng Cô -> 13:00 Nghỉ ngơi và dạo biển -> 15:00 Khởi hành về Huế -> 17:00 Trả khách.",
                Highlights = "Vịnh biển Lăng Cô; Cảnh quan đầm Lập An; Hải Vân Quan; Xe đưa đón trong ngày và hướng dẫn viên.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 850000,
                ImageUrl = "/images/lang-co-2023.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 10,
                Code = "HT-MINHMANG10",
                Title = "Tour Di Tích Lăng Minh Mạng – Lăng Gia Long – Đồi Vọng Cảnh",
                Category = "Di tích Cố Đô",
                ShortDescription = "Khám phá lăng tẩm uy nghiêm nhất triều Nguyễn, chiêm ngưỡng kiến trúc phong thủy đăng đối đỉnh cao bên dòng Hương giang.",
                Description = "Hành trình đưa quý khách đến với Lăng Gia Long giữa rừng thông bạt ngàn và Lăng Minh Mạng thanh tịnh, tôn nghiêm.",
                Itinerary = "08:00 Đón khách tại trung tâm TP Huế -> 08:45 Khám phá Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng uy nghiêm -> 11:45 Ngắm sông Hương từ Đồi Vọng Cảnh -> 12:30 Thưởng thức bữa trưa đặc sản Huế.",
                Highlights = "Xe du lịch đời mới; Vé tham quan các lăng; Hướng dẫn viên am tường lịch sử; Nước suối và khăn lạnh.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 520000,
                ImageUrl = "/images/minh-mang-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 11,
                Code = "HT-NONLA11",
                Title = "Tour Trải Nghiệm Làng Nghề Nón Lá Tây Hồ & Hoa Giấy Thanh Tiên",
                Category = "Sông Hương & Làng nghề",
                ShortDescription = "Tự tay chằm chiếc nón bài thơ xứ Huế và học gấp hoa sen giấy Thanh Tiên cùng các nghệ nhân lão thành.",
                Description = "Tour văn hóa làng nghề truyền thống đặc sắc. Du khách được tự tay làm nón bài thơ và hoa giấy truyền thống mang về làm kỷ niệm.",
                Itinerary = "08:30 Đón khách -> 09:00 Đến làng nón Tây Hồ, trải nghiệm chằm nón bài thơ -> 10:30 Thăm làng hoa giấy Thanh Tiên -> 11:30 Thưởng trà sen và bánh đậu xanh trái cây -> 12:30 Trả khách.",
                Highlights = "Tự tay làm nón bài thơ mang về; Trò chuyện với nghệ nhân; Xe đưa đón; Trà sen cung đình.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 420000,
                ImageUrl = "/images/hue-conical-hat.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 12,
                Code = "HT-SUOIVOI12",
                Title = "Tour Sinh Thái Suối Voi – Đầm Lập An – Vịnh Lăng Cô",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Tuyến Suối Voi – Lập An – Lăng Cô đang tạm ẩn trong khi xác minh tình trạng khai thác tại điểm đến.",
                Description = "Tuyến gợi ý phía Nam Huế được giữ trong dữ liệu capstone để quản trị tham khảo. Suối Voi đang có dự án du lịch dang dở và thông tin về hoạt động tại điểm cần được xác nhận trước khi mở bán lại.",
                Itinerary = "Tạm ngưng nhận đặt chỗ. Chỉ kích hoạt lại sau khi xác minh được hoạt động, dịch vụ và điều kiện tham quan tại Suối Voi.",
                Highlights = "Đầm Lập An; Vịnh Lăng Cô; Tình trạng Suối Voi cần xác minh trước khi tổ chức.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 720000,
                ImageUrl = "/images/lang-co-2023.jpg",
                IsActive = false,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 13,
                Code = "HT-DAINOI-THIENMU13",
                Title = "Đại Nội – Chùa Thiên Mụ – Nhà vườn An Hiên",
                Category = "Di tích Cố Đô",
                ShortDescription = "Khám phá Hoàng thành, ghé ngôi chùa bên sông Hương và tìm hiểu nếp nhà vườn Huế.",
                Description = "Lịch trình gợi ý kết hợp di sản trung tâm, cảnh quan sông Hương và kiến trúc nhà vườn. Thứ tự điểm đến có thể điều chỉnh theo giờ mở cửa và điều kiện thực tế.",
                Itinerary = "08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Đại Nội -> 11:30 Nghỉ trưa -> 13:30 Tham quan chùa Thiên Mụ -> 15:00 Ghé nhà vườn An Hiên -> 16:30 Kết thúc tại trung tâm.",
                Highlights = "Đại Nội Huế; chùa Thiên Mụ; nhà vườn An Hiên; tuyến di sản trong thành phố.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 590000,
                ImageUrl = "/images/thien-mu-2025.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 14,
                Code = "HT-RUCHATAMGIANG14",
                Title = "Bình Minh Rú Chá – Phá Tam Giang – Làng chài",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Ngắm bình minh giữa rừng ngập mặn Rú Chá và tìm hiểu nhịp sống ven phá Tam Giang.",
                Description = "Chuyến đi gợi ý dành cho người thích thiên nhiên và đời sống địa phương. Thời điểm ngắm bình minh, hoạt động trên thuyền và điểm dừng cần xác nhận trước với đơn vị vận hành.",
                Itinerary = "05:00 Đón tại trung tâm -> 05:45 Dạo rừng ngập mặn Rú Chá -> 07:00 Ăn sáng địa phương -> 08:00 Tham quan vùng phá Tam Giang và làng chài -> 10:30 Trở về Huế.",
                Highlights = "Rừng ngập mặn Rú Chá; cảnh quan đầm phá; tìm hiểu sinh kế địa phương; khung giờ bình minh.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 460000,
                ImageUrl = "https://hue.gov.vn/Portals/0/Uploads/00.00.H57/Nam2025/Thang6/z5862841503198458c3a50b5d9fa36ebde75e83e601fed_17271732773451054397863_28_4_2025_17_12_01_832_CH.jpg?v=1773705600033",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 15,
                Code = "HT-SONGHUONGAMTHUC15",
                Title = "Dạo phố Huế – Chợ Đông Ba – Ẩm thực địa phương",
                Category = "Văn hóa & Ẩm thực",
                ShortDescription = "Đi bộ qua những điểm phố trung tâm, ghé chợ Đông Ba và thưởng thức các món Huế theo mùa.",
                Description = "Tuyến tham quan gợi ý kết hợp cảnh quan đô thị và ẩm thực. Món ăn, quán dừng và thời lượng có thể thay đổi theo ngày; giá chưa đại diện cho báo giá của nhà cung cấp.",
                Itinerary = "16:00 Hẹn gần cầu Trường Tiền -> 16:30 Dạo bờ sông Hương -> 17:00 Tham quan khu vực chợ Đông Ba -> 18:00 Thưởng thức các món địa phương -> 19:30 Kết thúc tại trung tâm.",
                Highlights = "Cầu Trường Tiền; chợ Đông Ba; món ăn địa phương; trải nghiệm đi bộ trung tâm.",
                Duration = "4 Giờ",
                StartingLocation = "Cầu Trường Tiền, TP Huế",
                BasePrice = 350000,
                ImageUrl = "/images/dong-ba-market-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 16,
                Code = "HT-LANGMINHMANG16",
                Title = "Lăng Gia Long – Lăng Minh Mạng – Lăng Khải Định",
                Category = "Di tích Cố Đô",
                ShortDescription = "So sánh dấu ấn kiến trúc và cảnh quan của ba lăng vua Nguyễn trong chuyến đi một ngày.",
                Description = "Tuyến di sản gợi ý nối các lăng tẩm ở phía nam thành phố. Thời gian di chuyển giữa các điểm khá dài, cần kiểm tra giờ mở cửa, vé và phương tiện trước ngày đi.",
                Itinerary = "07:30 Hẹn tại trung tâm -> 08:30 Tham quan Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng -> 12:00 Nghỉ trưa -> 14:00 Tham quan Lăng Khải Định -> 16:30 Trở về trung tâm.",
                Highlights = "Lăng Gia Long; lăng Minh Mạng; lăng Khải Định; kiến trúc và cảnh quan lăng tẩm triều Nguyễn.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 690000,
                ImageUrl = "/images/minh-mang-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 17,
                Code = "HT-THUANAN-TAMGIANG17",
                Title = "Biển Thuận An – Phá Tam Giang – Đầm Chuồn",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Kết hợp dạo biển Thuận An với cảnh quan đầm phá và trải nghiệm ẩm thực vùng ven Huế.",
                Description = "Lịch trình gợi ý theo mùa, kết hợp vùng biển và đầm phá phía đông Huế. Điều kiện thời tiết, tắm biển và hoạt động thuyền cần được xác nhận trước chuyến đi.",
                Itinerary = "08:00 Hẹn tại trung tâm -> 08:45 Dạo biển Thuận An -> 11:00 Di chuyển đến vùng phá Tam Giang -> 12:00 Ăn trưa địa phương -> 14:00 Tham quan Đầm Chuồn -> 16:00 Trở về Huế.",
                Highlights = "Biển Thuận An; cảnh quan phá Tam Giang; Đầm Chuồn; ẩm thực địa phương.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 560000,
                ImageUrl = "/images/thuan-an-beach-2023.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 18,
                Code = "HT-HOQUYEN18",
                Title = "Hổ Quyền – Điện Voi Ré – Lăng Tự Đức",
                Category = "Di tích Cố Đô",
                ShortDescription = "Tìm hiểu dấu tích trường đấu voi độc đáo, Điện Voi Ré và không gian lăng Tự Đức.",
                Description = "Tuyến di sản gợi ý ở phía tây nam Huế, nối Hổ Quyền với Điện Voi Ré và lăng Tự Đức. Cần kiểm tra giờ tham quan, vé và phương tiện trước khi khởi hành.",
                Itinerary = "08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Hổ Quyền -> 09:45 Tìm hiểu Điện Voi Ré -> 11:00 Tham quan lăng Tự Đức -> 12:30 Kết thúc tại trung tâm.",
                Highlights = "Di tích Hổ Quyền; Điện Voi Ré; lăng Tự Đức; kiến trúc và câu chuyện triều Nguyễn.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 420000,
                ImageUrl = "/images/ho-quyen-2008.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 19,
                Code = "HT-HONCHEN19",
                Title = "Điện Hòn Chén – Đồi Vọng Cảnh – Sông Hương",
                Category = "Sông Hương & Làng nghề",
                ShortDescription = "Ghé không gian tín ngưỡng Điện Hòn Chén và ngắm khúc sông Hương từ đồi Vọng Cảnh.",
                Description = "Hành trình gợi ý kết hợp điểm di tích bên sông và cảnh quan đồi Vọng Cảnh. Nếu đi thuyền, cần xác nhận bến, giờ chạy, điều kiện thời tiết và chương trình lễ hội với đơn vị tổ chức.",
                Itinerary = "07:30 Hẹn tại trung tâm Huế -> 08:15 Tham quan Điện Hòn Chén -> 10:30 Dừng ngắm cảnh tại đồi Vọng Cảnh -> 11:30 Trở về trung tâm. Phương án đường thủy chỉ thực hiện khi được xác nhận.",
                Highlights = "Điện Hòn Chén; đồi Vọng Cảnh; cảnh quan sông Hương; trải nghiệm văn hóa tín ngưỡng địa phương.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 390000,
                ImageUrl = "/images/hon-chen-2008.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 20,
                Code = "HT-THANHTAN20",
                Title = "Suối khoáng nóng Thanh Tân – Nghỉ dưỡng giữa thiên nhiên",
                Category = "Thiên nhiên & Nghỉ dưỡng",
                ShortDescription = "Dành một ngày thư giãn với trải nghiệm suối khoáng nóng ở phía tây bắc Huế.",
                Description = "Tuyến nghỉ dưỡng tham khảo đến khu suối khoáng nóng Thanh Tân, cách trung tâm Huế khoảng 30 km. Hạng mục dịch vụ, giờ hoạt động, vé và phương tiện cần xác nhận trực tiếp với khu du lịch trước chuyến đi.",
                Itinerary = "08:00 Khởi hành từ trung tâm Huế -> 09:00 Đến khu vực Thanh Tân -> 09:30–14:00 Tham quan và sử dụng dịch vụ đang mở -> 15:00 Trở về Huế. Hoạt động cụ thể phụ thuộc dịch vụ được xác nhận.",
                Highlights = "Khu suối khoáng nóng Thanh Tân; cảnh quan thiên nhiên; lịch trình nghỉ dưỡng trong ngày; dịch vụ phụ thuộc tình trạng vận hành.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 750000,
                ImageUrl = "https://huetourism.gov.vn/Medias/Nam2023/Thang8/suoi-khoang-nong-thanh-tan.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 21,
                Code = "HT-VANCU21",
                Title = "Làng bún Vân Cù – Phố cổ Bao Vinh – Làng Kim Long",
                Category = "Văn hóa & Ẩm thực",
                ShortDescription = "Tìm hiểu nghề làm bún truyền thống, dạo phố Bao Vinh và khám phá nếp sống ven sông.",
                Description = "Tuyến trải nghiệm cộng đồng gợi ý kết nối làng nghề Vân Cù, khu phố cổ Bao Vinh và làng Kim Long. Hoạt động gặp nghệ nhân, làm thử và nếm món cần đặt trước, tùy lịch của cộng đồng địa phương.",
                Itinerary = "08:00 Hẹn tại trung tâm Huế -> 08:45 Tham quan làng nghề bún Vân Cù -> 10:30 Dạo khu phố Bao Vinh -> 11:30 Ghé làng Kim Long, tìm hiểu nghề và ẩm thực địa phương -> 13:00 Trở về trung tâm.",
                Highlights = "Nghề làm bún Vân Cù; không gian phố Bao Vinh; làng Kim Long; trải nghiệm ẩm thực và đời sống địa phương.",
                Duration = "Nửa Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 430000,
                ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bd/Bao_Vinh_street.jpg/1280px-Bao_Vinh_street.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 22,
                Code = "HT-HONCHAO22",
                Title = "Đảo Hòn Chảo – Vịnh Lăng Cô",
                Category = "Sinh thái & Đầm phá",
                ShortDescription = "Ngắm cảnh biển Lăng Cô và trải nghiệm vùng đảo Hòn Chảo theo điều kiện thời tiết.",
                Description = "Tuyến biển tham khảo kết hợp vịnh Lăng Cô với hoạt động tham quan Hòn Chảo. Tàu thuyền, lặn ngắm san hô và các dịch vụ phải được xác nhận với đơn vị có tổ chức; lịch trình phụ thuộc thời tiết và an toàn hàng hải.",
                Itinerary = "07:00 Khởi hành từ trung tâm Huế -> 08:30 Đến vùng Lăng Cô -> 09:00 Tham quan Hòn Chảo nếu tàu được phép hoạt động và thời tiết phù hợp -> 12:00 Nghỉ trưa -> 14:00 Trở về Huế. Nếu điều kiện không đảm bảo, đổi lịch hoặc dừng hoạt động.",
                Highlights = "Vịnh Lăng Cô; cảnh quan đảo Hòn Chảo; hoạt động biển theo điều kiện thực tế; ưu tiên an toàn thời tiết.",
                Duration = "1 Ngày",
                StartingLocation = "Trung tâm TP Huế",
                BasePrice = 980000,
                ImageUrl = "/images/lang-co-2023.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new()
            {
                Id = 23,
                Code = "HT-DI_SAN_DEM23",
                Title = "Đại Nội Huế về đêm – Trải nghiệm di sản theo chương trình",
                Category = "Di tích Cố Đô",
                ShortDescription = "Gợi ý khám phá không gian Hoàng cung vào buổi tối khi có chương trình được công bố.",
                Description = "Một số chương trình đêm và dạ yến cung đình được tổ chức theo sự kiện hoặc lịch riêng. Tuyến này chỉ là gợi ý trong capstone; không mặc định hoạt động hằng ngày và cần xác nhận chương trình, vé, thời gian với đơn vị tổ chức.",
                Itinerary = "Lịch trình thay đổi theo chương trình được công bố. Chỉ đặt chỗ sau khi xác nhận ngày tổ chức, vé vào cửa và nội dung dịch vụ.",
                Highlights = "Không gian Hoàng cung; hoạt động văn hóa buổi tối theo lịch; cần xác nhận chương trình trước khi đặt.",
                Duration = "Theo chương trình",
                StartingLocation = "Đại Nội Huế",
                BasePrice = 650000,
                ImageUrl = "/images/hue-imperial-night-2024.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };
    }

    public Task<IEnumerable<Tour>> GetAllToursAsync(bool activeOnly = true)
    {
        var result = activeOnly ? _tours.Where(t => t.IsActive) : _tours;
        return Task.FromResult<IEnumerable<Tour>>(result.OrderByDescending(t => t.Id).ToList());
    }

    public Task<Tour?> GetTourByIdAsync(int id)
    {
        var tour = _tours.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(tour);
    }

    public Task<Tour?> GetTourByCodeAsync(string code)
    {
        var tour = _tours.FirstOrDefault(t => string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(tour);
    }

    public Task<IEnumerable<Tour>> SearchToursAsync(string? searchTerm, string? category)
    {
        var query = _tours.Where(t => t.IsActive);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(t => t.Title.ToLowerInvariant().Contains(term) ||
                                     t.ShortDescription.ToLowerInvariant().Contains(term) ||
                                     t.Highlights.ToLowerInvariant().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "Tất cả")
        {
            query = query.Where(t => string.Equals(t.Category, category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<Tour>>(query.OrderByDescending(t => t.Id).ToList());
    }

    public Task<int> AddTourAsync(Tour tour)
    {
        int newId = _tours.Any() ? _tours.Max(t => t.Id) + 1 : 1;
        tour.Id = newId;
        tour.CreatedAt = DateTime.Now;
        _tours.Add(tour);
        return Task.FromResult(newId);
    }

    public Task UpdateTourAsync(Tour tour)
    {
        var existing = _tours.FirstOrDefault(t => t.Id == tour.Id);
        if (existing != null)
        {
            existing.Code = tour.Code;
            existing.Title = tour.Title;
            existing.Category = tour.Category;
            existing.ShortDescription = tour.ShortDescription;
            existing.Description = tour.Description;
            existing.Itinerary = tour.Itinerary;
            existing.Highlights = tour.Highlights;
            existing.Duration = tour.Duration;
            existing.StartingLocation = tour.StartingLocation;
            existing.BasePrice = tour.BasePrice;
            existing.ImageUrl = tour.ImageUrl;
            existing.IsActive = tour.IsActive;
        }
        return Task.CompletedTask;
    }

    public Task DeleteTourAsync(int id)
    {
        var existing = _tours.FirstOrDefault(t => t.Id == id);
        if (existing != null) _tours.Remove(existing);
        return Task.CompletedTask;
    }
}
