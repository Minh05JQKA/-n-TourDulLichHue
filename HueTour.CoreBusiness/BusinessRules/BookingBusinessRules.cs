namespace HueTour.CoreBusiness.BusinessRules;

public class BusinessRuleResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RuleCode { get; set; }

    public static BusinessRuleResult Success() => new() { IsSuccess = true };
    public static BusinessRuleResult Failure(string message, string ruleCode) =>
        new() { IsSuccess = false, ErrorMessage = message, RuleCode = ruleCode };
}

/// <summary>
/// Chứa 2 luật nghiệp vụ bắt buộc của Đề tài 08: Tour Du Lịch Huế (ECO2415)
/// Cài đặt trực tiếp trong tầng CoreBusiness theo nguyên lý Clean Architecture.
/// </summary>
public static class BookingBusinessRules
{
    public const int CutoffHoursBeforeDeparture = 24;

    /// <summary>
    /// LUẬT NGHIỆP VỤ 1 (K1.3):
    /// 1. Tổng hành khách của chuyến <= số chỗ (MaxCapacity).
    /// 2. Khóa đặt trước giờ khởi hành 24 giờ.
    /// 3. Chuyến phải ở trạng thái mở (Open).
    /// </summary>
    public static BusinessRuleResult ValidateDepartureCapacityAndCutoff(
        Departure departure,
        int requestedPassengerCount,
        DateTime currentTime)
    {
        if (departure == null)
        {
            return BusinessRuleResult.Failure("Thông tin chuyến khởi hành không tồn tại.", "RULE1_DEPARTURE_NOT_FOUND");
        }

        if (requestedPassengerCount <= 0)
        {
            return BusinessRuleResult.Failure("Số lượng hành khách đặt tour phải lớn hơn 0.", "RULE1_PASSENGER_COUNT_ZERO");
        }

        // Kiểm tra trạng thái chuyến
        if (departure.Status != "Open")
        {
            return BusinessRuleResult.Failure(
                $"Chuyến khởi hành ngày {departure.DepartureDate:dd/MM/yyyy HH:mm} hiện đang ở trạng thái '{departure.Status}', đã tạm dừng nhận đăng ký.",
                "RULE1_DEPARTURE_CLOSED");
        }

        // Kiểm tra thời gian khóa đặt 24 giờ
        TimeSpan timeUntilDeparture = departure.DepartureDate - currentTime;
        if (timeUntilDeparture.TotalHours < CutoffHoursBeforeDeparture)
        {
            if (timeUntilDeparture.TotalHours <= 0)
            {
                return BusinessRuleResult.Failure(
                    $"Chuyến khởi hành lúc {departure.DepartureDate:HH:mm dd/MM/yyyy} đã qua giờ xuất phát. Quý khách vui lòng chọn chuyến tiếp theo.",
                    "RULE1_DEPARTURE_PAST");
            }

            return BusinessRuleResult.Failure(
                $"Chuyến khởi hành lúc {departure.DepartureDate:HH:mm dd/MM/yyyy} còn {timeUntilDeparture.TotalHours:0.#} giờ nữa sẽ xuất phát. " +
                $"Theo quy định, hệ thống khóa đặt vé trước {CutoffHoursBeforeDeparture} giờ để điều hành sắp xếp xe và hướng dẫn viên. Quý khách vui lòng chọn chuyến khởi hành khác.",
                "RULE1_CUTOFF_VIOLATION");
        }

        // Kiểm tra sức chứa (MaxCapacity)
        int availableSeats = departure.AvailableSeats;
        if (departure.BookedSeats + requestedPassengerCount > departure.MaxCapacity)
        {
            return BusinessRuleResult.Failure(
                $"Chuyến khởi hành ngày {departure.DepartureDate:dd/MM/yyyy} chỉ còn trống {availableSeats} chỗ. " +
                $"Số lượng quý khách muốn đặt ({requestedPassengerCount} khách) vượt quá số ghế còn lại của chuyến.",
                "RULE1_CAPACITY_EXCEEDED");
        }

        return BusinessRuleResult.Success();
    }

    /// <summary>
    /// LUẬT NGHIỆP VỤ 2 (K1.3):
    /// 1. Giá vé theo loại khách:
    ///    - Trẻ em dưới 5 tuổi: miễn phí (0% giá gốc, 0 VNĐ)
    ///    - Trẻ em từ 5 đến 11 tuổi: tính 50% giá gốc
    ///    - Người lớn từ 12 tuổi trở lên: tính 100% giá gốc
    ///    (Tỷ lệ được nạp từ cấu hình PriceRule)
    /// 2. Tổng booking = Tổng giá vé của tất cả hành khách (Sigma vé).
    /// </summary>
    public static decimal CalculatePassengerFare(decimal tourBasePrice, string passengerType, IEnumerable<PriceRule>? priceRules)
    {
        if (tourBasePrice < 0) tourBasePrice = 0;

        decimal multiplier = 1.0m;

        if (priceRules != null)
        {
            var rule = priceRules.FirstOrDefault(r => r.IsActive &&
                string.Equals(r.PassengerType, passengerType, StringComparison.OrdinalIgnoreCase));
            if (rule != null)
            {
                multiplier = rule.RateMultiplier;
            }
            else
            {
                multiplier = passengerType.ToLowerInvariant() switch
                {
                    "infant" => 0.0m,
                    "child" => 0.5m,
                    _ => 1.0m
                };
            }
        }
        else
        {
            multiplier = passengerType.ToLowerInvariant() switch
            {
                "infant" => 0.0m,
                "child" => 0.5m,
                _ => 1.0m
            };
        }

        return Math.Round(tourBasePrice * multiplier, 0);
    }

    /// <summary>
    /// Tính tổng tiền booking từ danh sách vé và xác thực tính nhất quán
    /// </summary>
    public static decimal CalculateTotalBookingAmount(IEnumerable<PassengerTicket> tickets)
    {
        if (tickets == null) return 0;
        return tickets.Sum(t => t.Price);
    }

    /// <summary>
    /// Xác thực toàn bộ danh sách vé và tổng tiền
    /// </summary>
    public static BusinessRuleResult ValidateBookingFares(
        TourBooking booking,
        decimal tourBasePrice,
        IEnumerable<PriceRule> priceRules)
    {
        if (booking == null)
            return BusinessRuleResult.Failure("Dữ liệu đơn đặt tour không hợp lệ.", "RULE2_NULL_BOOKING");

        if (booking.PassengerTickets == null || booking.PassengerTickets.Count == 0)
            return BusinessRuleResult.Failure("Đơn đặt tour phải có ít nhất 1 hành khách.", "RULE2_EMPTY_PASSENGERS");

        decimal computedTotal = 0;
        for (int i = 0; i < booking.PassengerTickets.Count; i++)
        {
            var ticket = booking.PassengerTickets[i];
            if (string.IsNullOrWhiteSpace(ticket.FullName))
            {
                return BusinessRuleResult.Failure($"Hành khách thứ {i + 1} chưa nhập họ và tên.", "RULE2_INVALID_NAME");
            }

            decimal expectedPrice = CalculatePassengerFare(tourBasePrice, ticket.PassengerType, priceRules);
            ticket.Price = expectedPrice;
            computedTotal += expectedPrice;
        }

        booking.TotalAmount = computedTotal;
        return BusinessRuleResult.Success();
    }
}
