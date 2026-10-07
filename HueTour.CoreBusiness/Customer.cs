namespace HueTour.CoreBusiness;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? IdentityCard { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
