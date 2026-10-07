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
}
