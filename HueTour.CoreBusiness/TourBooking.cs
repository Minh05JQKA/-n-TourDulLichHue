namespace HueTour.CoreBusiness;

public static class TourBookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public class TourBooking
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int DepartureId { get; set; }
    public Departure? Departure { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = TourBookingStatus.Pending;
    public string? Notes { get; set; }
    public string? SpecialRequests { get; set; }
    public string PaymentMethod { get; set; } = "Thanh toán khi khởi hành / Chuyển khoản QR";

    // Header-Line Relationship
    public List<PassengerTicket> PassengerTickets { get; set; } = new();
}
