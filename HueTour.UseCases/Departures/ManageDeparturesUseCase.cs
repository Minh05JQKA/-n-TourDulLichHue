using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Departures;

public interface IManageDeparturesUseCase
{
    Task<IEnumerable<Departure>> GetAllDeparturesAsync();
    Task<int> AddDepartureAsync(Departure departure);
    Task UpdateDepartureAsync(Departure departure);
    Task CloseDepartureAsync(int departureId); // Chốt đoàn
    Task CancelDepartureAsync(int departureId); // Hủy chuyến không đủ khách
}

public class ManageDeparturesUseCase : IManageDeparturesUseCase
{
    private readonly IDepartureRepository _departureRepository;

    public ManageDeparturesUseCase(IDepartureRepository departureRepository)
    {
        _departureRepository = departureRepository;
    }

    public async Task<IEnumerable<Departure>> GetAllDeparturesAsync()
    {
        return await _departureRepository.GetAllDeparturesAsync();
    }

    public async Task<int> AddDepartureAsync(Departure departure)
    {
        return await _departureRepository.AddDepartureAsync(departure);
    }

    public async Task UpdateDepartureAsync(Departure departure)
    {
        await _departureRepository.UpdateDepartureAsync(departure);
    }

    public async Task CloseDepartureAsync(int departureId)
    {
        await _departureRepository.UpdateStatusAsync(departureId, "Closed");
    }

    public async Task CancelDepartureAsync(int departureId)
    {
        await _departureRepository.UpdateStatusAsync(departureId, "Cancelled");
    }
}
