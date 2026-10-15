using System.Text;
using Microsoft.Extensions.AI;
using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.AiAssistant;

public class AiAdviceRequest
{
    public string UserPrompt { get; set; } = string.Empty;
    public decimal? MaxBudget { get; set; }
    public string? PreferredDuration { get; set; } // "Nửa Ngày", "1 Ngày", "2 Ngày 1 Đêm", "3 Giờ"
    public int AdultCount { get; set; } = 1;
    public int ChildCount { get; set; } = 0;
}

public class AiAdviceResponse
{
    public string AdviceMessage { get; set; } = string.Empty;
    public List<TourRecommendationItem> RecommendedTours { get; set; } = new();
    public bool UsedLlm { get; set; }
    public string? AiProvider { get; set; }
    public string? Notice { get; set; }
}

public class TourRecommendationItem
{
    public Tour Tour { get; set; } = null!;
    public Departure? NextDeparture { get; set; }
    public decimal EstimatedTotalCost { get; set; }
    public string WhyRecommended { get; set; } = string.Empty;
}

public interface IAiTourAdvisorUseCase
{
    Task<AiAdviceResponse> GetTourAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// TIÊU CHÍ K4.3 & MỤC 10 (capstone-topics-2026.pdf):
/// - IChatClient được inject vào UseCase riêng (không gọi trực tiếp từ file .razor).
/// - API key đặt trong User Secrets / biến môi trường, không commit lên git.
/// - AI chạy trên dữ liệu tour thật từ CSDL, không bịa đặt tour.
/// - Có xử lý lỗi/quota: nếu API LLM gặp sự cố, ứng dụng vẫn chạy bình thường, hiển thị thông báo.
/// - AI chỉ đóng vai trò gợi ý; dữ liệu booking được người dùng xác nhận trước khi lưu vào CSDL.
/// </summary>
public class AiTourAdvisorUseCase : IAiTourAdvisorUseCase
{
    private readonly ITourRepository _tourRepository;
    private readonly IDepartureRepository _departureRepository;
    private readonly IPriceRuleRepository _priceRuleRepository;
    private readonly IChatClient? _chatClient;

    public AiTourAdvisorUseCase(
        ITourRepository tourRepository,
        IDepartureRepository departureRepository,
        IPriceRuleRepository priceRuleRepository,
        IChatClient? chatClient = null)
    {
        _tourRepository = tourRepository;
        _departureRepository = departureRepository;
        _priceRuleRepository = priceRuleRepository;
        _chatClient = chatClient;
    }

    public async Task<AiAdviceResponse> GetTourAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Lấy dữ liệu thật từ cơ sở dữ liệu
        var allTours = (await _tourRepository.GetAllToursAsync(activeOnly: true)).ToList();
        var priceRules = (await _priceRuleRepository.GetAllPriceRulesAsync()).ToList();

        string query = (request.UserPrompt ?? string.Empty).ToLowerInvariant();

        // 2. Chấm điểm mức độ phù hợp của từng tour theo từ khóa & ngân sách
        bool likesHistory = query.Contains("đại nội") || query.Contains("lăng") || query.Contains("lịch sử") || query.Contains("cung đình") || query.Contains("hoàng");
        bool likesNature = query.Contains("sông") || query.Contains("tam giang") || query.Contains("đầm phá") || query.Contains("hoàng hôn") || query.Contains("thiên nhiên") || query.Contains("bạch mã") || query.Contains("lập an");
        bool likesFood = query.Contains("ẩm thực") || query.Contains("ăn") || query.Contains("bánh") || query.Contains("đêm") || query.Contains("chè") || query.Contains("bún bò");
        bool likesCraft = query.Contains("làng nghề") || query.Contains("nón") || query.Contains("hoa giấy") || query.Contains("hương") || query.Contains("thủy biều");

        var scoredList = new List<(Tour tour, int score, string reason)>();

        foreach (var tour in allTours)
        {
            int score = 0;
            string reason = "Hành trình đặc sắc tại Huế.";

            if (likesHistory && (tour.Category.Contains("Cố Đô") || tour.Title.Contains("Đại Nội") || tour.Title.Contains("Lăng")))
            {
                score += 15;
                reason = "Rất phù hợp vì bạn muốn khám phá di sản lịch sử và kiến trúc cung đình triều Nguyễn.";
            }

            if (likesNature && (tour.Category.Contains("Sinh thái") || tour.Title.Contains("Tam Giang") || tour.Title.Contains("Bạch Mã") || tour.Title.Contains("Lập An")))
            {
                score += 15;
                reason = "Lựa chọn lý tưởng để tận hưởng cảnh sắc thiên nhiên kỳ vĩ và vùng đầm phá sông nước Cố Đô.";
            }

            if (likesFood && (tour.Category.Contains("Ẩm thực") || tour.Title.Contains("Ẩm thực") || tour.Title.Contains("Đêm") || tour.Title.Contains("Xích Lô")))
            {
                score += 15;
                reason = "Trải nghiệm văn hóa ẩm thực truyền thống và các món ăn trứ danh Cố Đô.";
            }

            if (likesCraft && (tour.Category.Contains("Làng nghề") || tour.Title.Contains("Thủy Biều") || tour.Title.Contains("Thanh Tiên") || tour.Title.Contains("Làng")))
            {
                score += 15;
                reason = "Hòa mình vào không gian làng nghề thủ công và văn hóa dân gian truyền thống xứ Huế.";
            }

            if (!string.IsNullOrEmpty(request.PreferredDuration) && tour.Duration.Contains(request.PreferredDuration, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            // Tính chi phí đoàn dựa trên Luật Nghiệp Vụ 2 (K1.3)
            decimal adultPrice = BookingBusinessRules.CalculatePassengerFare(tour.BasePrice, "Adult", priceRules);
            decimal childPrice = BookingBusinessRules.CalculatePassengerFare(tour.BasePrice, "Child", priceRules);
            decimal totalEst = (adultPrice * request.AdultCount) + (childPrice * request.ChildCount);

            if (request.MaxBudget.HasValue && request.MaxBudget.Value > 0)
            {
                if (totalEst <= request.MaxBudget.Value)
                    score += 8;
                else
                    score -= 5;
            }

            scoredList.Add((tour, score, reason));
        }

        var topTours = scoredList.OrderByDescending(x => x.score).Take(3).ToList();
        var recommendedItems = new List<TourRecommendationItem>();

        foreach (var item in topTours)
        {
            var deps = await _departureRepository.GetDeparturesByTourIdAsync(item.tour.Id, futureOnly: true);
            var nextDep = deps.OrderBy(d => d.DepartureDate).FirstOrDefault();

            decimal adultPrice = BookingBusinessRules.CalculatePassengerFare(item.tour.BasePrice, "Adult", priceRules);
            decimal childPrice = BookingBusinessRules.CalculatePassengerFare(item.tour.BasePrice, "Child", priceRules);
            decimal totalEst = (adultPrice * request.AdultCount) + (childPrice * request.ChildCount);

            recommendedItems.Add(new TourRecommendationItem
            {
                Tour = item.tour,
                NextDeparture = nextDep,
                EstimatedTotalCost = totalEst,
                WhyRecommended = item.reason
            });
        }

        // 3. Chuẩn bị gọi IChatClient theo Mục 10 (nếu có đăng ký IChatClient)
        string aiAdviceText = string.Empty;
        bool usedLlm = false;
        string? providerName = null;
        string? notice = null;

        if (_chatClient != null)
        {
            try
            {
                var tourCatalogBuilder = new StringBuilder();
                foreach (var t in allTours)
                {
                    tourCatalogBuilder.AppendLine($"- Mã {t.Code}: '{t.Title}', Loại: {t.Category}, Thời lượng: {t.Duration}, Giá gốc: {t.BasePrice:N0} VNĐ. Điểm nhấn: {t.Highlights}.");
                }

                var promptMessages = new List<ChatMessage>
                {
                    new(ChatRole.System,
                        "Bạn là Trợ lý AI Cố Đô Huế (Hue Tour Advisor). Hãy đóng vai một hướng dẫn viên am hiểu, thanh lịch và hiếu khách của xứ Huế. " +
                        "Dưới đây là danh sách tour CHÍNH XÁC từ cơ sở dữ liệu hệ thống:\n" +
                        tourCatalogBuilder.ToString() +
                        "\nNguyên tắc bắt buộc: Chỉ gợi ý các tour có trong danh sách trên. Không được tự bịa thông tin tour ngoài cơ sở dữ liệu."),

                    new(ChatRole.User,
                        $"Yêu cầu của khách: \"{request.UserPrompt}\". Đoàn gồm {request.AdultCount} người lớn, {request.ChildCount} trẻ em. " +
                        $"{(request.MaxBudget.HasValue ? $"Ngân sách tối đa: {request.MaxBudget:N0} VNĐ." : "")} " +
                        $"{(string.IsNullOrWhiteSpace(request.PreferredDuration) ? "" : $"Thời lượng mong muốn: {request.PreferredDuration}.")}. " +
                        $"Hãy đưa ra lời tư vấn ngắn gọn, súc tích (khoảng 3-4 câu), nêu bật các tour phù hợp nhất và lời khuyên chuẩn bị khi tham quan Cố Đô.")
                };

                var completion = await _chatClient.CompleteAsync(promptMessages, cancellationToken: cancellationToken);
                if (!string.IsNullOrWhiteSpace(completion.Message.Text))
                {
                    aiAdviceText = completion.Message.Text;
                    usedLlm = true;
                    providerName = _chatClient.Metadata.ProviderName ?? "IChatClient AI Model";
                    notice = $"Gợi ý được sinh bởi mô hình AI ({providerName}) kết hợp dữ liệu tour CSDL.";
                }
            }
            catch (Exception ex)
            {
                // MỤC 10: Xử lý khi API lỗi hoặc hết hạn mức -> Ứng dụng vẫn chạy, hiển thị thông báo thân thiện
                usedLlm = false;
                notice = $"Dịch vụ AI từ xa tạm thời gián đoạn ({ex.Message}). Hệ thống tự động chuyển sang mô hình chuyên gia nội bộ để đảm bảo phục vụ.";
            }
        }

        if (string.IsNullOrWhiteSpace(aiAdviceText))
        {
            aiAdviceText = $"Dạ chào Quý khách! Dựa trên yêu cầu: \"{request.UserPrompt}\", với đoàn {request.AdultCount} người lớn" +
                $"{(request.ChildCount > 0 ? $", {request.ChildCount} trẻ em" : "")}, " +
                $"Hệ thống Trợ lý Tour Huế trân trọng đề xuất các hành trình tối ưu nhất dưới đây:";
            notice ??= "Đang hoạt động với cơ chế phân tích dữ liệu tour & lịch khởi hành từ CSDL.";
        }

        return new AiAdviceResponse
        {
            AdviceMessage = aiAdviceText,
            RecommendedTours = recommendedItems,
            UsedLlm = usedLlm,
            AiProvider = providerName,
            Notice = notice
        };
    }
}
