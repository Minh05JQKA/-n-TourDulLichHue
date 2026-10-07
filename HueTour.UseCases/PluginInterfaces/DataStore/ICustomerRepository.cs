using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> GetCustomerByPhoneAsync(string phone);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<int> AddCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
}
