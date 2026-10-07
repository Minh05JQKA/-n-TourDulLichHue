using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class DepartureRepository : IDepartureRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public DepartureRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Departure>> GetDeparturesByTourIdAsync(int tourId, bool futureOnly = false)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        string sql = @"
            SELECT * FROM dbo.Departure
            WHERE TourId = @TourId";

        if (futureOnly)
        {
            sql += " AND DepartureDate >= @Now";
        }

        sql += " ORDER BY DepartureDate ASC";

        return await conn.QueryAsync<Departure>(sql, new { TourId = tourId, Now = DateTime.Now });
    }

    public async Task<IEnumerable<Departure>> GetAllDeparturesAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT d.*, t.Id AS Tour_Id, t.Title, t.Code, t.BasePrice
            FROM dbo.Departure d
            INNER JOIN dbo.Tour t ON d.TourId = t.Id
            ORDER BY d.DepartureDate DESC";

        var departures = await conn.QueryAsync<Departure, Tour, Departure>(
            sql,
            (departure, tour) =>
            {
                departure.Tour = tour;
                return departure;
            },
            splitOn: "Tour_Id"
        );

        return departures;
    }

    public async Task<Departure?> GetDepartureByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT d.*, t.Id AS Tour_Id, t.Title, t.Code, t.BasePrice, t.Duration, t.StartingLocation
            FROM dbo.Departure d
            LEFT JOIN dbo.Tour t ON d.TourId = t.Id
            WHERE d.Id = @Id";

        var result = await conn.QueryAsync<Departure, Tour, Departure>(
            sql,
            (departure, tour) =>
            {
                departure.Tour = tour;
                return departure;
            },
            new { Id = id },
            splitOn: "Tour_Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<int> AddDepartureAsync(Departure departure)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
            VALUES (@TourId, @DepartureDate, @ReturnDate, @MaxCapacity, @BookedSeats, @Status, @MeetingPoint, @TourGuideName, @TourGuidePhone);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(sql, departure);
    }

    public async Task UpdateDepartureAsync(Departure departure)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Departure
            SET DepartureDate = @DepartureDate, ReturnDate = @ReturnDate, MaxCapacity = @MaxCapacity,
                BookedSeats = @BookedSeats, Status = @Status, MeetingPoint = @MeetingPoint,
                TourGuideName = @TourGuideName, TourGuidePhone = @TourGuidePhone
            WHERE Id = @Id";

        await conn.ExecuteAsync(sql, departure);
    }

    public async Task UpdateStatusAsync(int departureId, string newStatus)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "UPDATE dbo.Departure SET Status = @Status WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = departureId, Status = newStatus });
    }

    public async Task<bool> IncrementBookedSeatsAsync(int departureId, int seatsCount)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE dbo.Departure
            SET BookedSeats = CASE
                WHEN BookedSeats + @SeatsCount < 0 THEN 0
                ELSE BookedSeats + @SeatsCount
            END
            WHERE Id = @Id";

        int rows = await conn.ExecuteAsync(sql, new { Id = departureId, SeatsCount = seatsCount });
        return rows > 0;
    }

    public async Task DeleteDepartureAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM dbo.Departure WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
