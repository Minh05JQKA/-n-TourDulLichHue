using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class UserAccountInMemoryRepository : IUserAccountRepository
{
    private readonly List<UserAccount> _users = new()
    {
        new() { Id = 1, Username = "admin", FullName = "Quản Trị Viên Tour Huế", Role = "Admin", Email = "admin@huetour.vn", PhoneNumber = "0905123456" },
        new() { Id = 2, Username = "khachhang", FullName = "Nguyễn Văn An", Role = "Customer", Email = "an.nguyen@gmail.com", PhoneNumber = "0912345678" }
    };

    public UserAccountInMemoryRepository()
    {
        _users[0].PasswordHash = UserAccountPasswordHasher.Hash("Admin@123");
        _users[1].PasswordHash = UserAccountPasswordHasher.Hash("User@123");
    }

    public Task<UserAccount?> GetByUsernameAsync(string username)
    {
        return Task.FromResult(_users.FirstOrDefault(u => string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    public Task<UserAccount?> ValidateCredentialsAsync(string username, string password)
    {
        var user = _users.FirstOrDefault(u => string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        var valid = user is not null && UserAccountPasswordHasher.Verify(user.PasswordHash, password);
        return Task.FromResult(valid ? user : null);
    }

    public Task<bool> RegisterUserAccountAsync(UserAccount user)
    {
        lock (_users)
        {
            if (_users.Any(u => string.Equals(u.Username, user.Username.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(false);
            }

            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
            if (string.IsNullOrWhiteSpace(user.Role)) user.Role = "Customer";
            _users.Add(user);
            return Task.FromResult(true);
        }
    }
}
