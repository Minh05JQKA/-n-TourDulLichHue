using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.InMemory;

public class CustomerInMemoryRepository : ICustomerRepository
{
    private readonly List<Customer> _customers;

    public CustomerInMemoryRepository()
    {
        _customers = new List<Customer>
        {
            new() { Id = 1, FullName = "Trần Minh Tuấn", Email = "tuan.tran@gmail.com", PhoneNumber = "0988776655", Address = "12 Lê Lợi, TP Huế", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = 2, FullName = "Lê Thị Mai Anh", Email = "maianh.le@gmail.com", PhoneNumber = "0977889900", Address = "84 Nguyễn Huệ, TP Huế", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = 3, FullName = "Nguyễn Hoàng Long", Email = "long.nguyen@outlook.com", PhoneNumber = "0903112233", Address = "158 Phố Huế, Q. Hai Bà Trưng, Hà Nội", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = 4, FullName = "Vũ Minh Châu", Email = "minhchau.vu@gmail.com", PhoneNumber = "0918445566", Address = "45 Nguyễn Đình Chiểu, Q.1, TP. Hồ Chí Minh", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = 5, FullName = "Đặng Thu Thảo", Email = "thuthao.dang@gmail.com", PhoneNumber = "0935667788", Address = "22 Bạch Đằng, Q. Hải Châu, Đà Nẵng", CreatedAt = DateTime.Now.AddDays(-3) },
            new() { Id = 6, FullName = "Hoàng Đức Anh", Email = "ducanh.hoang@gmail.com", PhoneNumber = "0983990011", Address = "08 Hùng Vương, TP Huế", CreatedAt = DateTime.Now.AddDays(-5) }
        };
    }

    public Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.PhoneNumber == phone.Trim()));
    }

    public Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => string.Equals(c.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)));
    }

    public Task<int> AddCustomerAsync(Customer customer)
    {
        int newId = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
        customer.Id = newId;
        customer.CreatedAt = DateTime.Now;
        _customers.Add(customer);
        return Task.FromResult(newId);
    }

    public Task UpdateCustomerAsync(Customer customer)
    {
        var existing = _customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existing != null)
        {
            existing.FullName = customer.FullName;
            existing.Email = customer.Email;
            existing.PhoneNumber = customer.PhoneNumber;
            existing.Address = customer.Address;
            existing.IdentityCard = customer.IdentityCard;
        }
        return Task.CompletedTask;
    }
}
