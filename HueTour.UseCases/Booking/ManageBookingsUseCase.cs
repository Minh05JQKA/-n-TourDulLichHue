using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public interface IManageBookingsUseCase
{
    Task<IEnumerable<TourBooking>> GetAllBookingsAsync();
    Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId);
    Task ConfirmBookingAsync(int bookingId);
    Task CancelBookingAsync(int bookingId);
    Task CompleteBookingAsync(int bookingId);
}

public class ManageBookingsUseCase : IManageBookingsUseCase
{
    private readonly ITourBookingRepository _bookingRepository;
    private readonly IDepartureRepository _departureRepository;

    public ManageBookingsUseCase(
        ITourBookingRepository bookingRepository,
        IDepartureRepository departureRepository)
    {
        _bookingRepository = bookingRepository;
        _departureRepository = departureRepository;
    }

    public async Task<IEnumerable<TourBooking>> GetAllBookingsAsync()
    {
        return await _bookingRepository.GetAllBookingsAsync();
    }

    public async Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId)
    {
        return await _bookingRepository.GetBookingsByDepartureIdAsync(departureId);
    }

    public async Task ConfirmBookingAsync(int bookingId)
    {
        await _bookingRepository.UpdateBookingStatusAsync(bookingId, "Confirmed");
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking != null && booking.Status != "Cancelled")
        {
            await _bookingRepository.UpdateBookingStatusAsync(bookingId, "Cancelled");
            // Hoàn lại số ghế nếu cần
            var tickets = await _bookingRepository.GetTicketsByBookingIdAsync(bookingId);
            int count = tickets.Count();
            if (count > 0)
            {
                await _departureRepository.IncrementBookedSeatsAsync(booking.DepartureId, -count);
            }
        }
    }

    public async Task CompleteBookingAsync(int bookingId)
    {
        await _bookingRepository.UpdateBookingStatusAsync(bookingId, "Completed");
    }
}
