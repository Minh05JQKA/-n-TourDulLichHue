using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface IPriceRuleRepository
{
    Task<IEnumerable<PriceRule>> GetAllPriceRulesAsync();
    Task<PriceRule?> GetPriceRuleByTypeAsync(string passengerType);
    Task UpdatePriceRuleAsync(PriceRule priceRule);
}
