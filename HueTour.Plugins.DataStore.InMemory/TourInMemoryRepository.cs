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
                ImageUrl = "/images/ngo-mon-gate.jpg",
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
                ImageUrl = "/images/tam-giang-lagoon.jpg",
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
                ImageUrl = "/images/truong-tien-bridge.jpg",
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
                ImageUrl = "/images/thien-mu-pagoda.jpg",
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
                ImageUrl = "/images/bun-bo-hue.jpg",
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
                ImageUrl = "/images/bach-ma-national-park.jpg",
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
