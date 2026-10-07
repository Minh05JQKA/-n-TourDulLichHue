using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface IDepartureRepository
{
    Task<IEnumerable<Departure>> GetDeparturesByTourIdAsync(int tourId, bool futureOnly = false);
    Task<IEnumerable<Departure>> GetAllDeparturesAsync();
    Task<Departure?> GetDepartureByIdAsync(int id);
    Task<int> AddDepartureAsync(Departure departure);
    Task UpdateDepartureAsync(Departure departure);
    Task UpdateStatusAsync(int departureId, string newStatus);
    Task<bool> IncrementBookedSeatsAsync(int departureId, int seatsCount);
    Task DeleteDepartureAsync(int id);
}
