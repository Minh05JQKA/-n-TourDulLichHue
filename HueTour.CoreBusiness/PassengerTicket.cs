namespace HueTour.CoreBusiness;

public class PassengerTicket
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public TourBooking? TourBooking { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Nam"; // Nam, Nữ, Khác
    public string PassengerType { get; set; } = "Adult"; // Adult (Người lớn), Child (Trẻ em 5-11t), Infant (Em bé <5t)
    public int BirthYear { get; set; } = DateTime.Now.Year - 25;
    public decimal Price { get; set; }
    public string? SeatNumber { get; set; }
    public string? Note { get; set; }
}
