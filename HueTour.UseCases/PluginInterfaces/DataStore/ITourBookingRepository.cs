using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public sealed record CreateBookingResult(bool IsSuccess, string? BookingCode = null, string? ErrorMessage = null, string? RuleCode = null)
{
    public static CreateBookingResult Success(string bookingCode) => new(true, bookingCode);
    public static CreateBookingResult Failure(string message, string ruleCode) => new(false, ErrorMessage: message, RuleCode: ruleCode);
}

public interface ITourBookingRepository
{
    /// <summary>
    /// K2.2: Cặp header-line (TourBooking - PassengerTicket) được ghi trong 1 transaction CSDL
    /// </summary>
    Task<CreateBookingResult> CreateBookingTransactionalAsync(TourBooking booking, List<PassengerTicket> tickets, int seatsToReserve);
    Task<BusinessRuleResult> CancelBookingTransactionalAsync(int bookingId);
    Task<TourBooking?> GetBookingByIdAsync(int id);
    Task<TourBooking?> GetBookingByCodeAsync(string bookingCode);
    Task<IEnumerable<TourBooking>> GetAllBookingsAsync();
    Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId);
    Task UpdateBookingStatusAsync(int bookingId, string status);
    Task<IEnumerable<PassengerTicket>> GetTicketsByBookingIdAsync(int bookingId);
}
