using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.UseCases.AiAssistant;

public class AiAdviceRequest
{
    public string UserPrompt { get; set; } = string.Empty;
    public decimal? MaxBudget { get; set; }
    public string? PreferredDuration { get; set; } // "Nửa Ngày", "1 Ngày", "2 Ngày 1 Đêm"
    public int AdultCount { get; set; } = 1;
    public int ChildCount { get; set; } = 0;
}

public class AiAdviceResponse
{
    public string AdviceMessage { get; set; } = string.Empty;
    public List<TourRecommendationItem> RecommendedTours { get; set; } = new();
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
    Task<AiAdviceResponse> GetTourAdviceAsync(AiAdviceRequest request);
}

public class AiTourAdvisorUseCase : IAiTourAdvisorUseCase
{
    private readonly ITourRepository _tourRepository;
    private readonly IDepartureRepository _departureRepository;
    private readonly IPriceRuleRepository _priceRuleRepository;

    public AiTourAdvisorUseCase(
        ITourRepository tourRepository,
        IDepartureRepository departureRepository,
        IPriceRuleRepository priceRuleRepository)
    {
        _tourRepository = tourRepository;
        _departureRepository = departureRepository;
        _priceRuleRepository = priceRuleRepository;
    }

    public async Task<AiAdviceResponse> GetTourAdviceAsync(AiAdviceRequest request)
    {
        var allTours = (await _tourRepository.GetAllToursAsync(activeOnly: true)).ToList();
        var priceRules = (await _priceRuleRepository.GetAllPriceRulesAsync()).ToList();

        string query = (request.UserPrompt ?? string.Empty).ToLowerInvariant();

        // 1. Phân loại theo sở thích từ khóa
        bool likesHistory = query.Contains("đại nội") || query.Contains("lăng") || query.Contains("lịch sử") || query.Contains("cung đình") || query.Contains("hoàng");
        bool likesNature = query.Contains("sông") || query.Contains("tam giang") || query.Contains("đầm phá") || query.Contains("hoàng hôn") || query.Contains("thiên nhiên") || query.Contains("bạch mã");
        bool likesFood = query.Contains("ẩm thực") || query.Contains("ăn") || query.Contains("bánh") || query.Contains("đêm") || query.Contains("chè");

        var scoredList = new List<(Tour tour, int score, string reason)>();

        foreach (var tour in allTours)
        {
            int score = 0;
            string reason = "Tour nổi bật tại Huế.";

            if (likesHistory && (tour.Category.Contains("Cố Đô") || tour.Title.Contains("Đại Nội") || tour.Title.Contains("Lăng")))
            {
                score += 10;
                reason = "Rất phù hợp vì bạn muốn khám phá di sản lịch sử và kiến trúc cung đình triều Nguyễn.";
            }

            if (likesNature && (tour.Category.Contains("Sinh thái") || tour.Title.Contains("Tam Giang") || tour.Title.Contains("Sông Hương") || tour.Title.Contains("Bạch Mã")))
            {
                score += 10;
                reason = "Lựa chọn lý tưởng để tận hưởng vẻ đẹp thiên nhiên thơ mộng của sông Hương và đầm phá Tam Giang.";
            }

            if (likesFood && (tour.Category.Contains("Ẩm thực") || tour.Title.Contains("Ẩm thực") || tour.Title.Contains("Đêm")))
            {
                score += 10;
                reason = "Trải nghiệm văn hóa ẩm thực truyền thống và các món ăn trứ danh Cố Đô.";
            }

            // Lọc theo thời lượng
            if (!string.IsNullOrEmpty(request.PreferredDuration) && tour.Duration.Contains(request.PreferredDuration, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            // Tính chi phí ước tính
            decimal adultPrice = BookingBusinessRules.CalculatePassengerFare(tour.BasePrice, "Adult", priceRules);
            decimal childPrice = BookingBusinessRules.CalculatePassengerFare(tour.BasePrice, "Child", priceRules);
            decimal totalEst = (adultPrice * request.AdultCount) + (childPrice * request.ChildCount);

            if (request.MaxBudget.HasValue && request.MaxBudget.Value > 0)
            {
                if (totalEst <= request.MaxBudget.Value)
                {
                    score += 8;
                }
                else
                {
                    score -= 5;
                }
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

        string intro = $"Dạ chào Quý khách! Dựa trên yêu cầu: \"{request.UserPrompt}\", " +
            $"với đoàn {request.AdultCount} người lớn{(request.ChildCount > 0 ? $", {request.ChildCount} trẻ em" : "")}, " +
            $"Trợ lý AI Du Lịch Huế trân trọng gợi ý các hành trình phù hợp nhất:";

        return new AiAdviceResponse
        {
            AdviceMessage = intro,
            RecommendedTours = recommendedItems
        };
    }
}
