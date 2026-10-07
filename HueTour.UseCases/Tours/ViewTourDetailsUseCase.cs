using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Tours;

public interface IViewTourDetailsUseCase
{
    Task<Tour?> ExecuteAsync(int tourId);
}

public class ViewTourDetailsUseCase : IViewTourDetailsUseCase
{
    private readonly ITourRepository _tourRepository;
    private readonly IDepartureRepository _departureRepository;

    public ViewTourDetailsUseCase(ITourRepository tourRepository, IDepartureRepository departureRepository)
    {
        _tourRepository = tourRepository;
        _departureRepository = departureRepository;
    }

    public async Task<Tour?> ExecuteAsync(int tourId)
    {
        var tour = await _tourRepository.GetTourByIdAsync(tourId);
        if (tour != null)
        {
            var departures = await _departureRepository.GetDeparturesByTourIdAsync(tourId, futureOnly: false);
            tour.Departures = departures.ToList();
        }
        return tour;
    }
}
