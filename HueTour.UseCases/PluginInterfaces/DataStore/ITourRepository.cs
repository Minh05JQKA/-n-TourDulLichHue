using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllToursAsync(bool activeOnly = true);
    Task<Tour?> GetTourByIdAsync(int id);
    Task<Tour?> GetTourByCodeAsync(string code);
    Task<IEnumerable<Tour>> SearchToursAsync(string? searchTerm, string? category);
    Task<int> AddTourAsync(Tour tour);
    Task UpdateTourAsync(Tour tour);
    Task DeleteTourAsync(int id);
}
