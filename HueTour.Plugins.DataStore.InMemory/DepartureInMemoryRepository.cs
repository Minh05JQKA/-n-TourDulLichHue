using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class DepartureInMemoryRepository : IDepartureRepository
{
    private readonly List<Departure> _departures;
    private readonly ITourRepository _tourRepository;

    public DepartureInMemoryRepository(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
        var now = DateTime.Now;

        _departures = new List<Departure>
        {
            // Tour 1: Đại Nội
            new() { Id = 1, TourId = 1, DepartureDate = now.AddDays(3).Date.AddHours(8), ReturnDate = now.AddDays(3).Date.AddHours(17), MaxCapacity = 25, BookedSeats = 7, Status = "Open", MeetingPoint = "Cột cờ Phu Văn Lâu", TourGuideName = "Lê Văn Hoàng", TourGuidePhone = "0905111222" },
            new() { Id = 2, TourId = 1, DepartureDate = now.AddDays(6).Date.AddHours(8), ReturnDate = now.AddDays(6).Date.AddHours(17), MaxCapacity = 25, BookedSeats = 3, Status = "Open", MeetingPoint = "Cột cờ Phu Văn Lâu", TourGuideName = "Nguyễn Thị Thu", TourGuidePhone = "0905333444" },
            // Chuyến test K1.3 Luật 1: ĐÃ ĐẦY CHỖ (25/25)
            new() { Id = 3, TourId = 1, DepartureDate = now.AddDays(4).Date.AddHours(8), ReturnDate = now.AddDays(4).Date.AddHours(17), MaxCapacity = 25, BookedSeats = 25, Status = "Open", MeetingPoint = "Cột cờ Phu Văn Lâu", TourGuideName = "Trần Minh Đức", TourGuidePhone = "0905555666" },
            // Chuyến test K1.3 Luật 1: KHÓA TRƯỚC 24H (chỉ còn 10h)
            new() { Id = 4, TourId = 1, DepartureDate = now.AddHours(10), ReturnDate = now.AddHours(19), MaxCapacity = 25, BookedSeats = 12, Status = "Open", MeetingPoint = "Cột cờ Phu Văn Lâu", TourGuideName = "Lê Văn Hoàng", TourGuidePhone = "0905111222" },

            // Tour 2: Phá Tam Giang
            new() { Id = 5, TourId = 2, DepartureDate = now.AddDays(2).Date.AddHours(14), ReturnDate = now.AddDays(2).Date.AddHours(20), MaxCapacity = 20, BookedSeats = 8, Status = "Open", MeetingPoint = "Bến thuyền Đầm Chuồn", TourGuideName = "Phan Thanh Sang", TourGuidePhone = "0905777888" },
            new() { Id = 6, TourId = 2, DepartureDate = now.AddDays(5).Date.AddHours(14), ReturnDate = now.AddDays(5).Date.AddHours(20), MaxCapacity = 20, BookedSeats = 2, Status = "Open", MeetingPoint = "Bến thuyền Đầm Chuồn", TourGuideName = "Phan Thanh Sang", TourGuidePhone = "0905777888" },

            // Tour 3: Du thuyền Ca Huế Sông Hương
            new() { Id = 7, TourId = 3, DepartureDate = now.AddDays(2).Date.AddHours(19), ReturnDate = now.AddDays(2).Date.AddHours(22), MaxCapacity = 30, BookedSeats = 14, Status = "Open", MeetingPoint = "Bến thuyền Tòa Khâm", TourGuideName = "Hoàng Yến Nhi", TourGuidePhone = "0905999000" },
            new() { Id = 8, TourId = 3, DepartureDate = now.AddDays(4).Date.AddHours(19), ReturnDate = now.AddDays(4).Date.AddHours(22), MaxCapacity = 30, BookedSeats = 6, Status = "Open", MeetingPoint = "Bến thuyền Tòa Khâm", TourGuideName = "Hoàng Yến Nhi", TourGuidePhone = "0905999000" },

            // Tour 4: Thủy Biều Làng Cổ
            new() { Id = 9, TourId = 4, DepartureDate = now.AddDays(3).Date.AddHours(8).AddMinutes(30), ReturnDate = now.AddDays(3).Date.AddHours(13).AddMinutes(30), MaxCapacity = 15, BookedSeats = 5, Status = "Open", MeetingPoint = "Chùa Thiên Mụ", TourGuideName = "Đặng Văn Phú", TourGuidePhone = "0905222111" },

            // Tour 5: Food Tour Đêm Huế
            new() { Id = 10, TourId = 5, DepartureDate = now.AddDays(2).Date.AddHours(17).AddMinutes(30), ReturnDate = now.AddDays(2).Date.AddHours(21).AddMinutes(30), MaxCapacity = 16, BookedSeats = 4, Status = "Open", MeetingPoint = "Cổng Khách sạn Hương Giang", TourGuideName = "Võ Thị Mỹ Linh", TourGuidePhone = "0905444333" },

            // Tour 6: Bạch Mã Trekking
            new() { Id = 11, TourId = 6, DepartureDate = now.AddDays(5).Date.AddHours(7).AddMinutes(30), ReturnDate = now.AddDays(5).Date.AddHours(17).AddMinutes(30), MaxCapacity = 20, BookedSeats = 6, Status = "Open", MeetingPoint = "Trung tâm TP Huế", TourGuideName = "Bùi Quang Dũng", TourGuidePhone = "0905666777" },
            // Lịch mẫu mở bán để có thể xem và thử luồng đặt tour mới.
            new() { Id = 12, TourId = 7, DepartureDate = now.AddDays(4).Date.AddHours(8), ReturnDate = now.AddDays(4).Date.AddHours(12).AddMinutes(30), MaxCapacity = 18, BookedSeats = 0, Status = "Open", MeetingPoint = "Trung tâm TP Huế", TourGuideName = "Công ty điều phối", TourGuidePhone = null },
            new() { Id = 13, TourId = 8, DepartureDate = now.AddDays(3).Date.AddHours(8), ReturnDate = now.AddDays(3).Date.AddHours(13), MaxCapacity = 18, BookedSeats = 0, Status = "Open", MeetingPoint = "Trung tâm TP Huế", TourGuideName = "Công ty điều phối", TourGuidePhone = null },
            new() { Id = 14, TourId = 9, DepartureDate = now.AddDays(5).Date.AddHours(7), ReturnDate = now.AddDays(5).Date.AddHours(17), MaxCapacity = 20, BookedSeats = 0, Status = "Open", MeetingPoint = "Trung tâm TP Huế", TourGuideName = "Công ty điều phối", TourGuidePhone = null }
        };
    }

    public async Task<IEnumerable<Departure>> GetDeparturesByTourIdAsync(int tourId, bool futureOnly = false)
    {
        var list = _departures.Where(d => d.TourId == tourId);
        if (futureOnly)
        {
            list = list.Where(d => d.DepartureDate >= DateTime.Now);
        }

        var result = list.OrderBy(d => d.DepartureDate).ToList();
        var tour = await _tourRepository.GetTourByIdAsync(tourId);
        foreach (var dep in result)
        {
            dep.Tour = tour;
        }

        return result;
    }

    public async Task<IEnumerable<Departure>> GetAllDeparturesAsync()
    {
        var tours = (await _tourRepository.GetAllToursAsync(false)).ToDictionary(t => t.Id);
        foreach (var dep in _departures)
        {
            if (tours.TryGetValue(dep.TourId, out var tour))
            {
                dep.Tour = tour;
            }
        }
        return _departures.OrderByDescending(d => d.DepartureDate).ToList();
    }

    public async Task<Departure?> GetDepartureByIdAsync(int id)
    {
        var dep = _departures.FirstOrDefault(d => d.Id == id);
        if (dep != null)
        {
            dep.Tour = await _tourRepository.GetTourByIdAsync(dep.TourId);
        }
        return dep;
    }

    public Task<int> AddDepartureAsync(Departure departure)
    {
        int newId = _departures.Any() ? _departures.Max(d => d.Id) + 1 : 1;
        departure.Id = newId;
        _departures.Add(departure);
        return Task.FromResult(newId);
    }

    public Task UpdateDepartureAsync(Departure departure)
    {
        var existing = _departures.FirstOrDefault(d => d.Id == departure.Id);
        if (existing != null)
        {
            existing.DepartureDate = departure.DepartureDate;
            existing.ReturnDate = departure.ReturnDate;
            existing.MaxCapacity = departure.MaxCapacity;
            existing.BookedSeats = departure.BookedSeats;
            existing.Status = departure.Status;
            existing.MeetingPoint = departure.MeetingPoint;
            existing.TourGuideName = departure.TourGuideName;
            existing.TourGuidePhone = departure.TourGuidePhone;
        }
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(int departureId, string newStatus)
    {
        var dep = _departures.FirstOrDefault(d => d.Id == departureId);
        if (dep != null) dep.Status = newStatus;
        return Task.CompletedTask;
    }

    public Task<bool> IncrementBookedSeatsAsync(int departureId, int seatsCount)
    {
        var dep = _departures.FirstOrDefault(d => d.Id == departureId);
        if (dep != null)
        {
            dep.BookedSeats = Math.Max(0, dep.BookedSeats + seatsCount);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task DeleteDepartureAsync(int id)
    {
        var dep = _departures.FirstOrDefault(d => d.Id == id);
        if (dep != null) _departures.Remove(dep);
        return Task.CompletedTask;
    }
}
