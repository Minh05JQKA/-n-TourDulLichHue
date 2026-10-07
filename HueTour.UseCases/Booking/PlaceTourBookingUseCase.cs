using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.Booking;

public class PlaceBookingResult
{
    public bool IsSuccess { get; set; }
    public string? BookingCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RuleCode { get; set; }
    public TourBooking? Booking { get; set; }

    public static PlaceBookingResult Success(string bookingCode, TourBooking booking) =>
        new() { IsSuccess = true, BookingCode = bookingCode, Booking = booking };

    public static PlaceBookingResult Failure(string message, string ruleCode) =>
        new() { IsSuccess = false, ErrorMessage = message, RuleCode = ruleCode };
}

public interface IPlaceTourBookingUseCase
{
    Task<PlaceBookingResult> ExecuteAsync(
        int departureId,
        Customer customer,
        List<PassengerTicket> passengers,
        string? notes = null,
        string? paymentMethod = null);
}

public class PlaceTourBookingUseCase : IPlaceTourBookingUseCase
{
    private readonly IDepartureRepository _departureRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IPriceRuleRepository _priceRuleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITourBookingRepository _bookingRepository;

    public PlaceTourBookingUseCase(
        IDepartureRepository departureRepository,
        ITourRepository tourRepository,
        IPriceRuleRepository priceRuleRepository,
        ICustomerRepository customerRepository,
        ITourBookingRepository bookingRepository)
    {
        _departureRepository = departureRepository;
        _tourRepository = tourRepository;
        _priceRuleRepository = priceRuleRepository;
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<PlaceBookingResult> ExecuteAsync(
        int departureId,
        Customer customer,
        List<PassengerTicket> passengers,
        string? notes = null,
        string? paymentMethod = null)
    {
        // 1. Kiểm tra thông tin hành khách
        if (passengers == null || passengers.Count == 0)
        {
            return PlaceBookingResult.Failure("Vui lòng nhập ít nhất 1 hành khách tham gia tour.", "RULE2_EMPTY_PASSENGERS");
        }

        // 2. Tải thông tin Chuyến khởi hành và Tour
        var departure = await _departureRepository.GetDepartureByIdAsync(departureId);
        if (departure == null)
        {
            return PlaceBookingResult.Failure("Không tìm thấy thông tin chuyến khởi hành.", "RULE1_DEPARTURE_NOT_FOUND");
        }

        var tour = await _tourRepository.GetTourByIdAsync(departure.TourId);
        if (tour == null)
        {
            return PlaceBookingResult.Failure("Không tìm thấy tour du lịch tương ứng.", "RULE1_TOUR_NOT_FOUND");
        }

        // 3. THỰC THI LUẬT NGHIỆP VỤ 1 (K1.3): Sức chứa & Khóa đặt 24h
        var rule1Result = BookingBusinessRules.ValidateDepartureCapacityAndCutoff(departure, passengers.Count, DateTime.Now);
        if (!rule1Result.IsSuccess)
        {
            return PlaceBookingResult.Failure(rule1Result.ErrorMessage!, rule1Result.RuleCode!);
        }

        // 4. THỰC THI LUẬT NGHIỆP VỤ 2 (K1.3): Tính giá vé theo loại khách & Tổng tiền
        var priceRules = await _priceRuleRepository.GetAllPriceRulesAsync();
        decimal totalAmount = 0;
        string datePrefix = DateTime.Now.ToString("yyyyMMdd");
        string randomSuffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        string bookingCode = $"HUE-{datePrefix}-{randomSuffix}";

        for (int i = 0; i < passengers.Count; i++)
        {
            var p = passengers[i];
            if (string.IsNullOrWhiteSpace(p.FullName))
            {
                return PlaceBookingResult.Failure($"Hành khách #{i + 1} chưa điền họ tên đầy đủ.", "RULE2_MISSING_NAME");
            }

            p.Price = BookingBusinessRules.CalculatePassengerFare(tour.BasePrice, p.PassengerType, priceRules);
            p.TicketCode = $"{bookingCode}-{i + 1:D2}";
            if (string.IsNullOrEmpty(p.SeatNumber))
            {
                p.SeatNumber = $"G{departure.BookedSeats + i + 1:D2}";
            }
            totalAmount += p.Price;
        }

        // 5. Lưu hoặc cập nhật khách hàng
        Customer? existingCustomer = null;
        if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
        {
            existingCustomer = await _customerRepository.GetCustomerByPhoneAsync(customer.PhoneNumber);
        }
        else if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            existingCustomer = await _customerRepository.GetCustomerByEmailAsync(customer.Email);
        }

        int customerId;
        if (existingCustomer != null)
        {
            existingCustomer.FullName = customer.FullName;
            existingCustomer.Address = customer.Address;
            if (!string.IsNullOrWhiteSpace(customer.Email)) existingCustomer.Email = customer.Email;
            await _customerRepository.UpdateCustomerAsync(existingCustomer);
            customerId = existingCustomer.Id;
        }
        else
        {
            customerId = await _customerRepository.AddCustomerAsync(customer);
        }

        // 6. Khởi tạo đối tượng TourBooking (Header)
        var booking = new TourBooking
        {
            BookingCode = bookingCode,
            DepartureId = departureId,
            CustomerId = customerId,
            BookingDate = DateTime.Now,
            TotalAmount = totalAmount,
            Status = "Pending",
            Notes = notes,
            PaymentMethod = paymentMethod ?? "Thanh toán khi khởi hành / Chuyển khoản QR",
            PassengerTickets = passengers
        };

        // 7. Ghi nhận Header-Line trong 1 Database Transaction (K2.2)
        await _bookingRepository.CreateBookingTransactionalAsync(booking, passengers);

        // 8. Cập nhật số ghế đã đặt của Departure
        await _departureRepository.IncrementBookedSeatsAsync(departureId, passengers.Count);

        booking.Departure = departure;
        booking.Customer = customer;
        departure.Tour = tour;

        return PlaceBookingResult.Success(bookingCode, booking);
    }
}
