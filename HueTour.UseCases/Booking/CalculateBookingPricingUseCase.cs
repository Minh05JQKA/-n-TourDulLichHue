using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public interface ICalculateBookingPricingUseCase
{
    Task<decimal> CalculateTicketPriceAsync(decimal tourBasePrice, string passengerType);
    Task<decimal> CalculateTotalAmountAsync(decimal tourBasePrice, IEnumerable<PassengerTicket> tickets);
    Task<IEnumerable<PriceRule>> GetPriceRulesAsync();
}

public class CalculateBookingPricingUseCase : ICalculateBookingPricingUseCase
{
    private readonly IPriceRuleRepository _priceRuleRepository;

    public CalculateBookingPricingUseCase(IPriceRuleRepository priceRuleRepository)
    {
        _priceRuleRepository = priceRuleRepository;
    }

    public async Task<IEnumerable<PriceRule>> GetPriceRulesAsync()
    {
        return await _priceRuleRepository.GetAllPriceRulesAsync();
    }

    public async Task<decimal> CalculateTicketPriceAsync(decimal tourBasePrice, string passengerType)
    {
        var rules = await _priceRuleRepository.GetAllPriceRulesAsync();
        return BookingBusinessRules.CalculatePassengerFare(tourBasePrice, passengerType, rules);
    }

    public async Task<decimal> CalculateTotalAmountAsync(decimal tourBasePrice, IEnumerable<PassengerTicket> tickets)
    {
        var rules = await _priceRuleRepository.GetAllPriceRulesAsync();
        foreach (var ticket in tickets)
        {
            ticket.Price = BookingBusinessRules.CalculatePassengerFare(tourBasePrice, ticket.PassengerType, rules);
        }
        return BookingBusinessRules.CalculateTotalBookingAmount(tickets);
    }
}
