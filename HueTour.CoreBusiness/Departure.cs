namespace HueTour.CoreBusiness;

public static class DepartureStatus
{
    public const string Open = "Open";
    public const string Closed = "Closed";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public class Departure
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public Tour? Tour { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public int MaxCapacity { get; set; } = 25;
    public int BookedSeats { get; set; } = 0;
    public string Status { get; set; } = DepartureStatus.Open;
    public string? MeetingPoint { get; set; } = "Bến thuyền Tòa Khâm / Cột cờ Phu Văn Lâu";
    public string? TourGuideName { get; set; }
    public string? TourGuidePhone { get; set; }

    // Calculated Helper Properties
    public int AvailableSeats => Math.Max(0, MaxCapacity - BookedSeats);
    public bool IsFullyBooked => BookedSeats >= MaxCapacity;
    public bool IsClosed => Status == DepartureStatus.Closed || Status == DepartureStatus.Completed || Status == DepartureStatus.Cancelled;
}
