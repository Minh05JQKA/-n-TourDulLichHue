using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public interface IViewBookingConfirmationUseCase
{
    Task<TourBooking?> ExecuteAsync(string bookingCode);
    Task<TourBooking?> ExecuteByIdAsync(int id);
}

public class ViewBookingConfirmationUseCase : IViewBookingConfirmationUseCase
{
    private readonly ITourBookingRepository _bookingRepository;
    private readonly IDepartureRepository _departureRepository;
    private readonly ITourRepository _tourRepository;
    private readonly ICustomerRepository _customerRepository;

    public ViewBookingConfirmationUseCase(
        ITourBookingRepository bookingRepository,
        IDepartureRepository departureRepository,
        ITourRepository tourRepository,
        ICustomerRepository customerRepository)
    {
        _bookingRepository = bookingRepository;
        _departureRepository = departureRepository;
        _tourRepository = tourRepository;
        _customerRepository = customerRepository;
    }

    public async Task<TourBooking?> ExecuteAsync(string bookingCode)
    {
        var booking = await _bookingRepository.GetBookingByCodeAsync(bookingCode);
        if (booking == null) return null;
        await LoadBookingDetails(booking);
        return booking;
    }

    public async Task<TourBooking?> ExecuteByIdAsync(int id)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(id);
        if (booking == null) return null;
        await LoadBookingDetails(booking);
        return booking;
    }

    private async Task LoadBookingDetails(TourBooking booking)
    {
        if (booking.Customer == null && booking.CustomerId > 0)
        {
            booking.Customer = await _customerRepository.GetCustomerByIdAsync(booking.CustomerId);
        }

        if (booking.Departure == null && booking.DepartureId > 0)
        {
            booking.Departure = await _departureRepository.GetDepartureByIdAsync(booking.DepartureId);
            if (booking.Departure != null && booking.Departure.Tour == null)
            {
                booking.Departure.Tour = await _tourRepository.GetTourByIdAsync(booking.Departure.TourId);
            }
        }

        if (booking.PassengerTickets == null || booking.PassengerTickets.Count == 0)
        {
            var tickets = await _bookingRepository.GetTicketsByBookingIdAsync(booking.Id);
            booking.PassengerTickets = tickets.ToList();
        }
    }
}
