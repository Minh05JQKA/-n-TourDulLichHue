using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Departures;

public interface IViewDeparturesByTourUseCase
{
    Task<IEnumerable<Departure>> ExecuteAsync(int tourId, bool futureOnly = false);
}

public class ViewDeparturesByTourUseCase : IViewDeparturesByTourUseCase
{
    private readonly IDepartureRepository _departureRepository;

    public ViewDeparturesByTourUseCase(IDepartureRepository departureRepository)
    {
        _departureRepository = departureRepository;
    }

    public async Task<IEnumerable<Departure>> ExecuteAsync(int tourId, bool futureOnly = false)
    {
        return await _departureRepository.GetDeparturesByTourIdAsync(tourId, futureOnly);
    }
}
