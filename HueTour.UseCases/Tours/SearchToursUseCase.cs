using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Tours;

public interface ISearchToursUseCase
{
    Task<IEnumerable<Tour>> ExecuteAsync(string? searchTerm, string? category);
}

public class SearchToursUseCase : ISearchToursUseCase
{
    private readonly ITourRepository _tourRepository;

    public SearchToursUseCase(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<IEnumerable<Tour>> ExecuteAsync(string? searchTerm, string? category)
    {
        return await _tourRepository.SearchToursAsync(searchTerm, category);
    }
}
