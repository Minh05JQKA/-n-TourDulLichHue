using System.Globalization;
using HueTour.CoreBusiness;

namespace HueTour.UseCases.Reports;

public enum DashboardTimeRange
{
    All,
    Month,
    Week,
    Today,
    Custom
}

public sealed class AdminDashboardChartData
{
    public List<string> TrendLabels { get; init; } = new();
    public List<decimal> TrendRevenue { get; init; } = new();
    public List<int> TrendBookings { get; init; } = new();
    public List<string> StatusLabels { get; init; } = new();
    public List<int> StatusCounts { get; init; } = new();
    public List<string> OccupancyLabels { get; init; } = new();
    public List<int> OccupancyBookedSeats { get; init; } = new();
    public List<int> OccupancyAvailableSeats { get; init; } = new();
    public List<string> CategoryLabels { get; init; } = new();
    public List<decimal> CategoryRevenue { get; init; } = new();
}

public interface IBuildAdminDashboardChartDataUseCase
{
    AdminDashboardChartData Execute(
        IEnumerable<Tour> tours,
        IEnumerable<Departure> departures,
        IEnumerable<TourBooking> bookings,
        DashboardTimeRange timeRange,
        DateTime currentTime,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null);
}

/// <summary>
/// Builds data-backed chart series for the admin dashboard.
/// Revenue includes confirmed and completed bookings only.
/// </summary>
public sealed class BuildAdminDashboardChartDataUseCase : IBuildAdminDashboardChartDataUseCase
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    public AdminDashboardChartData Execute(
        IEnumerable<Tour> tours,
        IEnumerable<Departure> departures,
        IEnumerable<TourBooking> bookings,
        DashboardTimeRange timeRange,
        DateTime currentTime,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null)
    {
        var tourList = tours.ToList();
        var departureList = departures.ToList();
        var bookingList = bookings.ToList();
        var periodBookings = bookingList
            .Where(booking => IsInSelectedPeriod(booking.BookingDate, timeRange, currentTime, customStartDate, customEndDate))
            .ToList();

        var chartData = new AdminDashboardChartData();
        AddTrendData(chartData, periodBookings, timeRange, currentTime, customStartDate, customEndDate);
        AddStatusData(chartData, periodBookings);
        AddOccupancyData(chartData, tourList, departureList, currentTime);
        AddCategoryRevenueData(chartData, tourList, departureList, periodBookings);
        return chartData;
    }

    private static bool IsInSelectedPeriod(
        DateTime bookingDate,
        DashboardTimeRange timeRange,
        DateTime currentTime,
        DateTime? customStartDate,
        DateTime? customEndDate)
    {
        if (timeRange == DashboardTimeRange.All)
        {
            return true;
        }

        if (bookingDate > currentTime)
        {
            return false;
        }

        if (timeRange == DashboardTimeRange.Custom)
        {
            return customStartDate.HasValue && customEndDate.HasValue &&
                   bookingDate.Date >= customStartDate.Value.Date &&
                   bookingDate.Date <= customEndDate.Value.Date;
        }

        var start = timeRange switch
        {
            DashboardTimeRange.Month => new DateTime(currentTime.Year, currentTime.Month, 1),
            DashboardTimeRange.Week => currentTime.Date.AddDays(-6),
            DashboardTimeRange.Today => currentTime.Date,
            _ => DateTime.MinValue
        };

        return bookingDate >= start;
    }

    private static void AddTrendData(
        AdminDashboardChartData chartData,
        IReadOnlyCollection<TourBooking> bookings,
        DashboardTimeRange timeRange,
        DateTime currentTime,
        DateTime? customStartDate,
        DateTime? customEndDate)
    {
        var today = currentTime.Date;
        DateTime start;
        int bucketCount;

        switch (timeRange)
        {
            case DashboardTimeRange.Month:
                start = new DateTime(today.Year, today.Month, 1);
                bucketCount = today.Day;
                chartData.TrendLabels.AddRange(Enumerable.Range(1, bucketCount)
                    .Select(day => new DateTime(today.Year, today.Month, day).ToString("dd/MM", VietnameseCulture)));
                break;
            case DashboardTimeRange.Week:
                start = today.AddDays(-6);
                bucketCount = 7;
                chartData.TrendLabels.AddRange(Enumerable.Range(0, bucketCount)
                    .Select(offset => start.AddDays(offset).ToString("ddd dd/MM", VietnameseCulture)));
                break;
            case DashboardTimeRange.Today:
                start = today;
                bucketCount = 12;
                chartData.TrendLabels.AddRange(Enumerable.Range(0, bucketCount)
                    .Select(bucket => $"{bucket * 2:00}–{(bucket + 1) * 2:00}h"));
                break;
            case DashboardTimeRange.Custom:
            {
                var rangeStart = customStartDate?.Date ?? today;
                var rangeEnd = customEndDate?.Date ?? rangeStart;
                var dailyBuckets = (rangeEnd - rangeStart).TotalDays <= 30;

                if (dailyBuckets)
                {
                    start = rangeStart;
                    bucketCount = (rangeEnd - rangeStart).Days + 1;
                    chartData.TrendLabels.AddRange(Enumerable.Range(0, bucketCount)
                        .Select(offset => rangeStart.AddDays(offset).ToString("dd/MM", VietnameseCulture)));
                }
                else
                {
                    start = new DateTime(rangeStart.Year, rangeStart.Month, 1);
                    var lastMonth = new DateTime(rangeEnd.Year, rangeEnd.Month, 1);
                    bucketCount = (lastMonth.Year - start.Year) * 12 + lastMonth.Month - start.Month + 1;
                    chartData.TrendLabels.AddRange(Enumerable.Range(0, bucketCount)
                        .Select(offset => start.AddMonths(offset).ToString("MM/yyyy", VietnameseCulture)));
                }

                break;
            }
            default:
                start = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
                bucketCount = 6;
                chartData.TrendLabels.AddRange(Enumerable.Range(0, bucketCount)
                    .Select(offset => start.AddMonths(offset).ToString("MM/yyyy", VietnameseCulture)));
                break;
        }

        var revenue = new decimal[bucketCount];
        var orders = new int[bucketCount];

        foreach (var booking in bookings)
        {
            int? bucket = GetBucketIndex(
                booking.BookingDate,
                start,
                currentTime,
                timeRange,
                bucketCount,
                customStartDate,
                customEndDate);
            if (bucket is null)
            {
                continue;
            }

            orders[bucket.Value]++;
            if (IsRevenueRecognized(booking.Status))
            {
                revenue[bucket.Value] += booking.TotalAmount;
            }
        }

        chartData.TrendRevenue.AddRange(revenue);
        chartData.TrendBookings.AddRange(orders);
    }

    private static int? GetBucketIndex(
        DateTime bookingDate,
        DateTime start,
        DateTime currentTime,
        DashboardTimeRange timeRange,
        int bucketCount,
        DateTime? customStartDate,
        DateTime? customEndDate)
    {
        if (bookingDate > currentTime)
        {
            return null;
        }

        int index = timeRange switch
        {
            DashboardTimeRange.Month => bookingDate.Date < start ? -1 : bookingDate.Day - 1,
            DashboardTimeRange.Week => (bookingDate.Date - start.Date).Days,
            DashboardTimeRange.Today => bookingDate.Date != start.Date ? -1 : bookingDate.Hour / 2,
            DashboardTimeRange.Custom => GetCustomBucketIndex(bookingDate, start, customStartDate, customEndDate),
            _ => (bookingDate.Year - start.Year) * 12 + bookingDate.Month - start.Month
        };

        return index >= 0 && index < bucketCount ? index : null;
    }

    private static int GetCustomBucketIndex(
        DateTime bookingDate,
        DateTime start,
        DateTime? customStartDate,
        DateTime? customEndDate)
    {
        if (!customStartDate.HasValue || !customEndDate.HasValue)
        {
            return -1;
        }

        return (customEndDate.Value.Date - customStartDate.Value.Date).TotalDays <= 30
            ? (bookingDate.Date - customStartDate.Value.Date).Days
            : (bookingDate.Year - start.Year) * 12 + bookingDate.Month - start.Month;
    }

    private static void AddStatusData(AdminDashboardChartData chartData, IReadOnlyCollection<TourBooking> bookings)
    {
        chartData.StatusLabels.AddRange(new[] { "Chờ xử lý", "Đã xác nhận", "Hoàn thành", "Đã hủy" });
        chartData.StatusCounts.AddRange(new[]
        {
            bookings.Count(booking => booking.Status == TourBookingStatus.Pending),
            bookings.Count(booking => booking.Status == TourBookingStatus.Confirmed),
            bookings.Count(booking => booking.Status == TourBookingStatus.Completed),
            bookings.Count(booking => booking.Status == TourBookingStatus.Cancelled)
        });
    }

    private static void AddOccupancyData(
        AdminDashboardChartData chartData,
        IReadOnlyCollection<Tour> tours,
        IReadOnlyCollection<Departure> departures,
        DateTime currentTime)
    {
        var tourNames = tours.ToDictionary(tour => tour.Id, tour => tour.Title);
        var topTours = departures
            .Where(departure => departure.DepartureDate.Date >= currentTime.Date &&
                                (departure.Status is DepartureStatus.Open or DepartureStatus.Closed) &&
                                departure.MaxCapacity > 0)
            .OrderBy(departure => departure.DepartureDate)
            .ThenByDescending(departure => departure.BookedSeats)
            .Take(6)
            .ToList();

        foreach (var departure in topTours)
        {
            var title = tourNames.GetValueOrDefault(departure.TourId, $"Tour #{departure.TourId}");
            var label = $"{title} · {departure.DepartureDate:dd/MM}";
            chartData.OccupancyLabels.Add(label.Length > 34 ? label[..31] + "…" : label);
            chartData.OccupancyBookedSeats.Add(Math.Clamp(departure.BookedSeats, 0, departure.MaxCapacity));
            chartData.OccupancyAvailableSeats.Add(Math.Max(0, departure.MaxCapacity - departure.BookedSeats));
        }
    }

    private static void AddCategoryRevenueData(
        AdminDashboardChartData chartData,
        IReadOnlyCollection<Tour> tours,
        IReadOnlyCollection<Departure> departures,
        IReadOnlyCollection<TourBooking> bookings)
    {
        var categoryByTourId = tours.ToDictionary(tour => tour.Id, tour => tour.Category);
        var tourIdByDepartureId = departures.ToDictionary(departure => departure.Id, departure => departure.TourId);
        var categoryRevenue = bookings
            .Where(booking => IsRevenueRecognized(booking.Status))
            .Select(booking =>
            {
                var tourId = booking.Departure?.TourId ?? tourIdByDepartureId.GetValueOrDefault(booking.DepartureId);
                return new
                {
                    Booking = booking,
                    Category = categoryByTourId.GetValueOrDefault(tourId, "Khác")
                };
            })
            .GroupBy(item => string.IsNullOrWhiteSpace(item.Category) ? "Chưa phân loại" : item.Category)
            .Select(group => new
            {
                Category = group.Key,
                Revenue = group.Sum(item => item.Booking.TotalAmount)
            })
            .OrderByDescending(item => item.Revenue)
            .Take(6);

        foreach (var item in categoryRevenue)
        {
            chartData.CategoryLabels.Add(item.Category);
            chartData.CategoryRevenue.Add(item.Revenue);
        }
    }

    private static bool IsRevenueRecognized(string status) =>
        status == TourBookingStatus.Confirmed || status == TourBookingStatus.Completed;
}
