using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Departures;

public interface IGetDepartureDetailsUseCase
{
    Task<Departure?> ExecuteAsync(int departureId);
}

public class GetDepartureDetailsUseCase : IGetDepartureDetailsUseCase
{
    private readonly IDepartureRepository _departureRepository;
    private readonly ITourRepository _tourRepository;

    public GetDepartureDetailsUseCase(IDepartureRepository departureRepository, ITourRepository tourRepository)
    {
        _departureRepository = departureRepository;
        _tourRepository = tourRepository;
    }

    public async Task<Departure?> ExecuteAsync(int departureId)
    {
        var departure = await _departureRepository.GetDepartureByIdAsync(departureId);
        if (departure != null && departure.Tour == null)
        {
            departure.Tour = await _tourRepository.GetTourByIdAsync(departure.TourId);
        }
        return departure;
    }
}
