using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Tours;

public interface IViewToursUseCase
{
    Task<IEnumerable<Tour>> ExecuteAsync(bool activeOnly = true);
}

public class ViewToursUseCase : IViewToursUseCase
{
    private readonly ITourRepository _tourRepository;

    public ViewToursUseCase(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<IEnumerable<Tour>> ExecuteAsync(bool activeOnly = true)
    {
        return await _tourRepository.GetAllToursAsync(activeOnly);
    }
}
