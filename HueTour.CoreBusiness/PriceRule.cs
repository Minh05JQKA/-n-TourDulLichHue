namespace HueTour.CoreBusiness;

public class PriceRule
{
    public int Id { get; set; }
    public string PassengerType { get; set; } = "Adult"; // Adult, Child, Infant
    public string DisplayName { get; set; } = "Người lớn";
    public decimal RateMultiplier { get; set; } = 1.0m; // 1.0 (100%), 0.5 (50%), 0.0 (Miễn phí)
    public int MinAge { get; set; } = 12;
    public int? MaxAge { get; set; }
    public string Description { get; set; } = "Áp dụng cho hành khách từ 12 tuổi trở lên";
    public bool IsActive { get; set; } = true;
}
