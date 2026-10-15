using System.Text;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Reports;

public interface IExportBookingReportUseCase
{
    Task<byte[]> ExportBookingsToCsvAsync();
    Task<byte[]> ExportDeparturePassengersToCsvAsync(int departureId);
}

/// <summary>
/// TIÊU CHÍ K4.3: Xuất báo cáo dữ liệu thật từ hệ thống (Excel / CSV với UTF-8 BOM).
/// Phục vụ điều hành tour xuất danh sách hành khách đoàn và báo cáo doanh thu theo kỳ.
/// </summary>
public class ExportBookingReportUseCase : IExportBookingReportUseCase
{
    private readonly ITourBookingRepository _bookingRepository;
    private readonly IDepartureRepository _departureRepository;

    public ExportBookingReportUseCase(
        ITourBookingRepository bookingRepository,
        IDepartureRepository departureRepository)
    {
        _bookingRepository = bookingRepository;
        _departureRepository = departureRepository;
    }

    public async Task<byte[]> ExportBookingsToCsvAsync()
    {
        var bookings = (await _bookingRepository.GetAllBookingsAsync()).ToList();

        var sb = new StringBuilder();
        // UTF-8 BOM giúp Microsoft Excel mở file CSV hiển thị đúng 100% tiếng Việt có dấu
        sb.Append('\uFEFF');

        // Header dòng tiêu đề
        sb.AppendLine("Mã Đặt Tour,Ngày Đặt,Tên Tour,Ngày Khởi Hành,Họ Tên Khách,Số Điện Thoại,Email,Tổng Tiền (VNĐ),Trạng Thái,Phương Thức Thanh Toán,Ghi Chú");

        foreach (var b in bookings)
        {
            string tourTitle = EscapeCsv(b.Departure?.Tour?.Title ?? "N/A");
            string depDate = b.Departure?.DepartureDate.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
            string custName = EscapeCsv(b.Customer?.FullName ?? "N/A");
            string phone = EscapeCsv(b.Customer?.PhoneNumber ?? "N/A");
            string email = EscapeCsv(b.Customer?.Email ?? "N/A");
            string status = EscapeCsv(b.Status);
            string payment = EscapeCsv(b.PaymentMethod);
            string notes = EscapeCsv(b.Notes ?? string.Empty);

            sb.AppendLine($"{b.BookingCode},{b.BookingDate:dd/MM/yyyy HH:mm},{tourTitle},{depDate},{custName},{phone},{email},{b.TotalAmount},{status},{payment},{notes}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportDeparturePassengersToCsvAsync(int departureId)
    {
        var departure = await _departureRepository.GetDepartureByIdAsync(departureId);
        var bookings = (await _bookingRepository.GetBookingsByDepartureIdAsync(departureId)).ToList();

        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        sb.AppendLine($"DANH SÁCH HÀNH KHÁCH ĐOÀN - CHUYẾN KHỞI HÀNH {departure?.DepartureDate:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Tour: {EscapeCsv(departure?.Tour?.Title ?? "N/A")}");
        sb.AppendLine($"Điểm đón: {EscapeCsv(departure?.MeetingPoint ?? "N/A")}, Hướng dẫn viên: {EscapeCsv(departure?.TourGuideName ?? "N/A")}");
        sb.AppendLine();

        sb.AppendLine("STT,Mã Vé,Họ Và Tên,Giới Tính,Loại Vé,Năm Sinh,Giá Vé (VNĐ),Số Ghế,Mã Booking,Tên Người Đặt,SĐT Liên Hệ");

        int stt = 1;
        foreach (var b in bookings)
        {
            var tickets = await _bookingRepository.GetTicketsByBookingIdAsync(b.Id);
            foreach (var t in tickets)
            {
                string pType = t.PassengerType switch
                {
                    "Adult" => "Người lớn",
                    "Child" => "Trẻ em (5-11t)",
                    "Infant" => "Em bé (<5t)",
                    _ => t.PassengerType
                };

                sb.AppendLine($"{stt++},{t.TicketCode},{EscapeCsv(t.FullName)},{t.Gender},{pType},{t.BirthYear},{t.Price},{t.SeatNumber},{b.BookingCode},{EscapeCsv(b.Customer?.FullName ?? "")},{b.Customer?.PhoneNumber ?? ""}");
            }
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
