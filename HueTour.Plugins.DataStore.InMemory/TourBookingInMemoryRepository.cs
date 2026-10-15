using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class TourBookingInMemoryRepository : ITourBookingRepository
{
    private readonly List<TourBooking> _bookings;
    private readonly List<PassengerTicket> _tickets;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDepartureRepository _departureRepository;

    public TourBookingInMemoryRepository(
        ICustomerRepository customerRepository,
        IDepartureRepository departureRepository)
    {
        _customerRepository = customerRepository;
        _departureRepository = departureRepository;

        _bookings = new List<TourBooking>
        {
            new()
            {
                Id = 1,
                BookingCode = "HUE-202610-001",
                DepartureId = 1,
                CustomerId = 1,
                BookingDate = DateTime.Now.AddDays(-1),
                TotalAmount = 825000,
                Status = TourBookingStatus.Confirmed,
                Notes = "Khách gia đình có 1 bé nhỏ",
                SpecialRequests = "Sắp xếp xe ngồi gần cửa sổ",
                PaymentMethod = "Chuyển khoản QR"
            },
            new()
            {
                Id = 2,
                BookingCode = "HUE-202610-002",
                DepartureId = 5,
                CustomerId = 3,
                BookingDate = DateTime.Now.AddDays(-2),
                TotalAmount = 1200000,
                Status = TourBookingStatus.Confirmed,
                Notes = "Đoàn tham gia chèo Sup hoàng hôn",
                SpecialRequests = "Áo phao cho trẻ em",
                PaymentMethod = "Chuyển khoản VietQR"
            },
            new()
            {
                Id = 3,
                BookingCode = "HUE-202610-003",
                DepartureId = 7,
                CustomerId = 4,
                BookingDate = DateTime.Now.AddDays(-1),
                TotalAmount = 550000,
                Status = TourBookingStatus.Pending,
                Notes = "Muốn ngồi tầng 2 du thuyền rồng",
                SpecialRequests = "Thả hoa đăng giấy màu",
                PaymentMethod = "Thanh toán trực tiếp"
            },
            new()
            {
                Id = 4,
                BookingCode = "HUE-202610-004",
                DepartureId = 11,
                CustomerId = 5,
                BookingDate = DateTime.Now.AddDays(-3),
                TotalAmount = 1360000,
                Status = TourBookingStatus.Confirmed,
                Notes = "Đoàn khám phá Bạch Mã",
                SpecialRequests = "Ăn trưa picnic bên suối",
                PaymentMethod = "Chuyển khoản QR"
            },
            new()
            {
                Id = 5,
                BookingCode = "HUE-202610-005",
                DepartureId = 1,
                CustomerId = 6,
                BookingDate = DateTime.Now.AddDays(-5),
                TotalAmount = 550000,
                Status = TourBookingStatus.Completed,
                Notes = "Khách đi 1 mình công tác",
                PaymentMethod = "Thẻ tín dụng"
            },
            new()
            {
                Id = 6,
                BookingCode = "HUE-202610-006",
                DepartureId = 2,
                CustomerId = 2,
                BookingDate = DateTime.Now.AddDays(-1),
                TotalAmount = 480000,
                Status = TourBookingStatus.Cancelled,
                Notes = "Bận việc đột xuất xin hủy",
                PaymentMethod = "Chuyển khoản"
            }
        };

        _tickets = new List<PassengerTicket>
        {
            // Booking 1 (1 Người lớn 550k + 1 Trẻ em 275k = 825k)
            new() { Id = 1, BookingId = 1, TicketCode = "HUE-202610-001-01", FullName = "Trần Minh Tuấn", Gender = "Nam", PassengerType = "Adult", BirthYear = 1990, Price = 550000, SeatNumber = "G01" },
            new() { Id = 2, BookingId = 1, TicketCode = "HUE-202610-001-02", FullName = "Trần Tuấn Khang", Gender = "Nam", PassengerType = "Child", BirthYear = 2018, Price = 275000, SeatNumber = "G02" },

            // Booking 2 (2 Người lớn 480k x 2 + 1 Trẻ em 240k + 1 Em bé 0k = 1.200k)
            new() { Id = 3, BookingId = 2, TicketCode = "HUE-202610-002-01", FullName = "Nguyễn Hoàng Long", Gender = "Nam", PassengerType = "Adult", BirthYear = 1988, Price = 480000, SeatNumber = "G01" },
            new() { Id = 4, BookingId = 2, TicketCode = "HUE-202610-002-02", FullName = "Lê Ngọc Bích", Gender = "Nữ", PassengerType = "Adult", BirthYear = 1991, Price = 480000, SeatNumber = "G02" },
            new() { Id = 5, BookingId = 2, TicketCode = "HUE-202610-002-03", FullName = "Nguyễn Hoàng Nam", Gender = "Nam", PassengerType = "Child", BirthYear = 2016, Price = 240000, SeatNumber = "G03" },
            new() { Id = 6, BookingId = 2, TicketCode = "HUE-202610-002-04", FullName = "Nguyễn Ngọc An", Gender = "Nữ", PassengerType = "Infant", BirthYear = 2023, Price = 0, SeatNumber = "G04" },

            // Booking 3 (2 Người lớn 220k x 2 + 1 Trẻ em 110k = 550k)
            new() { Id = 7, BookingId = 3, TicketCode = "HUE-202610-003-01", FullName = "Vũ Minh Châu", Gender = "Nữ", PassengerType = "Adult", BirthYear = 1995, Price = 220000, SeatNumber = "G05" },
            new() { Id = 8, BookingId = 3, TicketCode = "HUE-202610-003-02", FullName = "Phạm Nhật Minh", Gender = "Nam", PassengerType = "Adult", BirthYear = 1993, Price = 220000, SeatNumber = "G06" },
            new() { Id = 9, BookingId = 3, TicketCode = "HUE-202610-003-03", FullName = "Phạm Minh Nhi", Gender = "Nữ", PassengerType = "Child", BirthYear = 2019, Price = 110000, SeatNumber = "G07" },

            // Booking 4 (2 Người lớn 680k x 2 = 1.360k)
            new() { Id = 10, BookingId = 4, TicketCode = "HUE-202610-004-01", FullName = "Đặng Thu Thảo", Gender = "Nữ", PassengerType = "Adult", BirthYear = 1997, Price = 680000, SeatNumber = "G01" },
            new() { Id = 11, BookingId = 4, TicketCode = "HUE-202610-004-02", FullName = "Nguyễn Quang Vinh", Gender = "Nam", PassengerType = "Adult", BirthYear = 1996, Price = 680000, SeatNumber = "G02" },

            // Booking 5 (1 Người lớn = 550k)
            new() { Id = 12, BookingId = 5, TicketCode = "HUE-202610-005-01", FullName = "Hoàng Đức Anh", Gender = "Nam", PassengerType = "Adult", BirthYear = 1992, Price = 550000, SeatNumber = "G08" },

            // Booking 6 (1 Người lớn = 480k, đã hủy)
            new() { Id = 13, BookingId = 6, TicketCode = "HUE-202610-006-01", FullName = "Lê Thị Mai Anh", Gender = "Nữ", PassengerType = "Adult", BirthYear = 1994, Price = 480000, SeatNumber = "G09" }
        };
    }

    public async Task<CreateBookingResult> CreateBookingTransactionalAsync(TourBooking booking, List<PassengerTicket> tickets, int seatsToReserve)
    {
        var departure = await _departureRepository.GetDepartureByIdAsync(booking.DepartureId);
        if (departure == null)
            return CreateBookingResult.Failure("Không tìm thấy thông tin chuyến khởi hành.", "RULE1_DEPARTURE_NOT_FOUND");

        lock (_bookings)
        {
            var ruleResult = BookingBusinessRules.ValidateDepartureCapacityAndCutoff(departure, seatsToReserve, DateTime.Now);
            if (!ruleResult.IsSuccess)
                return CreateBookingResult.Failure(ruleResult.ErrorMessage!, ruleResult.RuleCode!);

            departure.BookedSeats += seatsToReserve;
            try
            {
            int newBookingId = _bookings.Any() ? _bookings.Max(b => b.Id) + 1 : 1;
            booking.Id = newBookingId;
            _bookings.Add(booking);

            int currentTicketId = _tickets.Any() ? _tickets.Max(t => t.Id) : 0;
            foreach (var ticket in tickets)
            {
                ticket.Id = ++currentTicketId;
                ticket.BookingId = newBookingId;
                _tickets.Add(ticket);
            }

            booking.PassengerTickets = new List<PassengerTicket>(tickets);
            return CreateBookingResult.Success(booking.BookingCode);
            }
            catch
            {
                departure.BookedSeats -= seatsToReserve;
                throw;
            }
        }
    }

    public async Task<BusinessRuleResult> CancelBookingTransactionalAsync(int bookingId)
    {
        int departureId;
        lock (_bookings)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
                return BusinessRuleResult.Failure(BookingBusinessRules.BookingNotFoundMessage, "RULE_CANCEL_BOOKING_NOT_FOUND");
            if (booking.Status == TourBookingStatus.Cancelled)
                return BusinessRuleResult.Success();
            departureId = booking.DepartureId;
        }

        var departure = await _departureRepository.GetDepartureByIdAsync(departureId);

        lock (_bookings)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
                return BusinessRuleResult.Failure(BookingBusinessRules.BookingNotFoundMessage, "RULE_CANCEL_BOOKING_NOT_FOUND");
            if (booking.Status == TourBookingStatus.Cancelled)
                return BusinessRuleResult.Success();

            var ticketCount = _tickets.Count(ticket => ticket.BookingId == bookingId);
            if (departure == null || departure.BookedSeats < ticketCount)
                return BusinessRuleResult.Failure(BookingBusinessRules.SeatReleaseFailedMessage, "RULE_CANCEL_SEAT_RELEASE_FAILED");

            booking.Status = TourBookingStatus.Cancelled;
            departure.BookedSeats -= ticketCount;
            return BusinessRuleResult.Success();
        }
    }

    public async Task<TourBooking?> GetBookingByIdAsync(int id)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == id);
        if (booking != null) await EnrichBookingAsync(booking);
        return booking;
    }

    public async Task<TourBooking?> GetBookingByCodeAsync(string bookingCode)
    {
        var booking = _bookings.FirstOrDefault(b => string.Equals(b.BookingCode, bookingCode, StringComparison.OrdinalIgnoreCase));
        if (booking != null) await EnrichBookingAsync(booking);
        return booking;
    }

    public async Task<IEnumerable<TourBooking>> GetAllBookingsAsync()
    {
        foreach (var b in _bookings)
        {
            await EnrichBookingAsync(b);
        }
        return _bookings.OrderByDescending(b => b.BookingDate).ToList();
    }

    public async Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId)
    {
        var list = _bookings.Where(b => b.DepartureId == departureId).ToList();
        foreach (var b in list)
        {
            await EnrichBookingAsync(b);
        }
        return list;
    }

    public Task UpdateBookingStatusAsync(int bookingId, string status)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null) booking.Status = status;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<PassengerTicket>> GetTicketsByBookingIdAsync(int bookingId)
    {
        var tickets = _tickets.Where(t => t.BookingId == bookingId).OrderBy(t => t.Id).ToList();
        return Task.FromResult<IEnumerable<PassengerTicket>>(tickets);
    }

    private async Task EnrichBookingAsync(TourBooking booking)
    {
        booking.Customer = await _customerRepository.GetCustomerByIdAsync(booking.CustomerId);
        booking.Departure = await _departureRepository.GetDepartureByIdAsync(booking.DepartureId);
        booking.PassengerTickets = _tickets.Where(t => t.BookingId == booking.Id).ToList();
    }
}
