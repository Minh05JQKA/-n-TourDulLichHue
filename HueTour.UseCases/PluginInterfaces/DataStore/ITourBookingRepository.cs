using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface ITourBookingRepository
{
    /// <summary>
    /// K2.2: Cặp header-line (TourBooking - PassengerTicket) được ghi trong 1 transaction CSDL
    /// </summary>
    Task<string> CreateBookingTransactionalAsync(TourBooking booking, List<PassengerTicket> tickets);
    Task<TourBooking?> GetBookingByIdAsync(int id);
    Task<TourBooking?> GetBookingByCodeAsync(string bookingCode);
    Task<IEnumerable<TourBooking>> GetAllBookingsAsync();
    Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId);
    Task UpdateBookingStatusAsync(int bookingId, string status);
    Task<IEnumerable<PassengerTicket>> GetTicketsByBookingIdAsync(int bookingId);
}
