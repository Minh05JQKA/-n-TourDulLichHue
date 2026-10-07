using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Tours;

public interface IManageToursUseCase
{
    Task<int> AddTourAsync(Tour tour);
    Task UpdateTourAsync(Tour tour);
    Task DeleteTourAsync(int tourId);
}

public class ManageToursUseCase : IManageToursUseCase
{
    private readonly ITourRepository _tourRepository;

    public ManageToursUseCase(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<int> AddTourAsync(Tour tour)
    {
        return await _tourRepository.AddTourAsync(tour);
    }

    public async Task UpdateTourAsync(Tour tour)
    {
        await _tourRepository.UpdateTourAsync(tour);
    }

    public async Task DeleteTourAsync(int tourId)
    {
        await _tourRepository.DeleteTourAsync(tourId);
    }
}
