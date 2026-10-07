using HueTour.CoreBusiness;
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
                Status = "Confirmed",
                Notes = "Khách gia đình có 1 bé nhỏ",
                SpecialRequests = "Sắp xếp xe ngồi gần cửa sổ",
                PaymentMethod = "Chuyển khoản QR"
            }
        };

        _tickets = new List<PassengerTicket>
        {
            new()
            {
                Id = 1,
                BookingId = 1,
                TicketCode = "HUE-202610-001-01",
                FullName = "Trần Minh Tuấn",
                Gender = "Nam",
                PassengerType = "Adult",
                BirthYear = 1990,
                Price = 550000,
                SeatNumber = "G01"
            },
            new()
            {
                Id = 2,
                BookingId = 1,
                TicketCode = "HUE-202610-001-02",
                FullName = "Trần Tuấn Khang",
                Gender = "Nam",
                PassengerType = "Child",
                BirthYear = 2018,
                Price = 275000,
                SeatNumber = "G02"
            }
        };
    }

    public Task<string> CreateBookingTransactionalAsync(TourBooking booking, List<PassengerTicket> tickets)
    {
        lock (_bookings)
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
            return Task.FromResult(booking.BookingCode);
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
