using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public interface IManageBookingsUseCase
{
    Task<IEnumerable<TourBooking>> GetAllBookingsAsync();
    Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId);
    Task ConfirmBookingAsync(int bookingId);
    Task<BusinessRuleResult> CancelBookingAsync(int bookingId);
    Task CompleteBookingAsync(int bookingId);
}

public class ManageBookingsUseCase : IManageBookingsUseCase
{
    private readonly ITourBookingRepository _bookingRepository;
    public ManageBookingsUseCase(ITourBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
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
        await _bookingRepository.UpdateBookingStatusAsync(bookingId, TourBookingStatus.Confirmed);
    }

    public Task<BusinessRuleResult> CancelBookingAsync(int bookingId) =>
        _bookingRepository.CancelBookingTransactionalAsync(bookingId);

    public async Task CompleteBookingAsync(int bookingId)
    {
        await _bookingRepository.UpdateBookingStatusAsync(bookingId, TourBookingStatus.Completed);
    }
}
