using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Auth;

public interface IAuthenticateUserUseCase
{
    Task<UserAccount?> ExecuteAsync(string username, string password);
}

public class AuthenticateUserUseCase : IAuthenticateUserUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;

    public AuthenticateUserUseCase(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    public async Task<UserAccount?> ExecuteAsync(string username, string password)
    {
        return await _userAccountRepository.ValidateCredentialsAsync(username, password);
    }
}
