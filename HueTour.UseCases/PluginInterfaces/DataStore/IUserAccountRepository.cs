using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.DataStore;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username);
    Task<UserAccount?> ValidateCredentialsAsync(string username, string password);
}
