using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class PriceRuleInMemoryRepository : IPriceRuleRepository
{
    private readonly List<PriceRule> _priceRules;

    public PriceRuleInMemoryRepository()
    {
        _priceRules = new List<PriceRule>
        {
            new() { Id = 1, PassengerType = "Adult", DisplayName = "Người lớn", RateMultiplier = 1.0m, MinAge = 12, MaxAge = null, Description = "Hành khách từ 12 tuổi trở lên tính 100% giá gốc", IsActive = true },
            new() { Id = 2, PassengerType = "Child", DisplayName = "Trẻ em (5 - 11 tuổi)", RateMultiplier = 0.5m, MinAge = 5, MaxAge = 11, Description = "Trẻ em từ 5 đến 11 tuổi tính 50% giá gốc", IsActive = true },
            new() { Id = 3, PassengerType = "Infant", DisplayName = "Em bé (Dưới 5 tuổi)", RateMultiplier = 0.0m, MinAge = 0, MaxAge = 4, Description = "Em bé dưới 5 tuổi miễn phí vé tour (0 VNĐ)", IsActive = true }
        };
    }

    public Task<IEnumerable<PriceRule>> GetAllPriceRulesAsync()
    {
        return Task.FromResult<IEnumerable<PriceRule>>(_priceRules.Where(r => r.IsActive).OrderByDescending(r => r.MinAge).ToList());
    }

    public Task<PriceRule?> GetPriceRuleByTypeAsync(string passengerType)
    {
        var rule = _priceRules.FirstOrDefault(r => string.Equals(r.PassengerType, passengerType, StringComparison.OrdinalIgnoreCase) && r.IsActive);
        return Task.FromResult(rule);
    }

    public Task UpdatePriceRuleAsync(PriceRule priceRule)
    {
        var existing = _priceRules.FirstOrDefault(r => r.Id == priceRule.Id);
        if (existing != null)
        {
            existing.DisplayName = priceRule.DisplayName;
            existing.RateMultiplier = priceRule.RateMultiplier;
            existing.MinAge = priceRule.MinAge;
            existing.MaxAge = priceRule.MaxAge;
            existing.Description = priceRule.Description;
            existing.IsActive = priceRule.IsActive;
        }
        return Task.CompletedTask;
    }
}
