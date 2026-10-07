using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public interface IValidateTourBookingUseCase
{
    Task<BusinessRuleResult> ValidateAsync(int departureId, int passengerCount, DateTime? currentTime = null);
}

public class ValidateTourBookingUseCase : IValidateTourBookingUseCase
{
    private readonly IDepartureRepository _departureRepository;

    public ValidateTourBookingUseCase(IDepartureRepository departureRepository)
    {
        _departureRepository = departureRepository;
    }

    public async Task<BusinessRuleResult> ValidateAsync(int departureId, int passengerCount, DateTime? currentTime = null)
    {
        var departure = await _departureRepository.GetDepartureByIdAsync(departureId);
        if (departure == null)
        {
            return BusinessRuleResult.Failure("Không tìm thấy chuyến khởi hành tương ứng.", "RULE1_DEPARTURE_NOT_FOUND");
        }

        DateTime now = currentTime ?? DateTime.Now;
        return BookingBusinessRules.ValidateDepartureCapacityAndCutoff(departure, passengerCount, now);
    }
}
