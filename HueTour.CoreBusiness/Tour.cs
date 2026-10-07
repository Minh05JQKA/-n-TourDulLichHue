namespace HueTour.CoreBusiness;

public class Tour
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Di tích Cố Đô";
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Itinerary { get; set; } = string.Empty;
    public string Highlights { get; set; } = string.Empty;
    public string Duration { get; set; } = "1 Ngày";
    public string StartingLocation { get; set; } = "Trung tâm TP Huế";
    public decimal BasePrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Departure> Departures { get; set; } = new();
}
