using System.Data;
using Dapper;
using HueTour.CoreBusiness;
using HueTour.UseCases.PluginInterfaces.DataStore;

namespace HueTour.Plugins.DataStore.SQL;

public class TourBookingRepository : ITourBookingRepository
{
    private readonly ISqlDbConnectionFactory _connectionFactory;

    public TourBookingRepository(ISqlDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// TIÊU CHÍ K2.2: Ghi cặp Header (TourBooking) và Line (PassengerTicket) trong 1 Transaction CSDL duy nhất.
    /// Đảm bảo tính toàn vẹn ACID, nếu 1 vé thất bại toàn bộ đơn booking sẽ bị Rollback.
    /// </summary>
    public async Task<string> CreateBookingTransactionalAsync(TourBooking booking, List<PassengerTicket> tickets)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        conn.Open();

        using IDbTransaction transaction = conn.BeginTransaction();
        try
        {
            // 1. Chèn Header: TourBooking
            const string insertHeaderSql = @"
                INSERT INTO dbo.TourBooking (BookingCode, DepartureId, CustomerId, BookingDate, TotalAmount, Status, Notes, SpecialRequests, PaymentMethod)
                VALUES (@BookingCode, @DepartureId, @CustomerId, @BookingDate, @TotalAmount, @Status, @Notes, @SpecialRequests, @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            int bookingId = await conn.ExecuteScalarAsync<int>(insertHeaderSql, new
            {
                booking.BookingCode,
                booking.DepartureId,
                booking.CustomerId,
                booking.BookingDate,
                booking.TotalAmount,
                booking.Status,
                booking.Notes,
                booking.SpecialRequests,
                booking.PaymentMethod
            }, transaction);

            booking.Id = bookingId;

            // 2. Chèn từng LineItem: PassengerTicket trong cùng một transaction
            const string insertLineSql = @"
                INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber, Note)
                VALUES (@BookingId, @TicketCode, @FullName, @Gender, @PassengerType, @BirthYear, @Price, @SeatNumber, @Note);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            foreach (var ticket in tickets)
            {
                ticket.BookingId = bookingId;
                int ticketId = await conn.ExecuteScalarAsync<int>(insertLineSql, ticket, transaction);
                ticket.Id = ticketId;
            }

            // 3. Commit transaction thành công
            transaction.Commit();
            return booking.BookingCode;
        }
        catch
        {
            // Rollback nếu có lỗi xảy ra
            transaction.Rollback();
            throw;
        }
    }

    public async Task<TourBooking?> GetBookingByIdAsync(int id)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.TourBooking WHERE Id = @Id";
        return await conn.QuerySingleOrDefaultAsync<TourBooking>(sql, new { Id = id });
    }

    public async Task<TourBooking?> GetBookingByCodeAsync(string bookingCode)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.TourBooking WHERE BookingCode = @Code";
        return await conn.QuerySingleOrDefaultAsync<TourBooking>(sql, new { Code = bookingCode });
    }

    public async Task<IEnumerable<TourBooking>> GetAllBookingsAsync()
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT b.*, c.Id AS Cust_Id, c.FullName, c.PhoneNumber, c.Email,
                   d.Id AS Dep_Id, d.DepartureDate, d.Status AS Dep_Status,
                   t.Id AS Tour_Id, t.Title AS Tour_Title, t.Code AS Tour_Code
            FROM dbo.TourBooking b
            INNER JOIN dbo.Customer c ON b.CustomerId = c.Id
            INNER JOIN dbo.Departure d ON b.DepartureId = d.Id
            INNER JOIN dbo.Tour t ON d.TourId = t.Id
            ORDER BY b.BookingDate DESC";

        var bookings = await conn.QueryAsync<TourBooking, Customer, Departure, Tour, TourBooking>(
            sql,
            (booking, customer, departure, tour) =>
            {
                booking.Customer = customer;
                departure.Tour = tour;
                booking.Departure = departure;
                return booking;
            },
            splitOn: "Cust_Id,Dep_Id,Tour_Id"
        );

        return bookings;
    }

    public async Task<IEnumerable<TourBooking>> GetBookingsByDepartureIdAsync(int departureId)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT b.*, c.Id AS Cust_Id, c.FullName, c.PhoneNumber, c.Email
            FROM dbo.TourBooking b
            INNER JOIN dbo.Customer c ON b.CustomerId = c.Id
            WHERE b.DepartureId = @DepartureId
            ORDER BY b.BookingDate ASC";

        var bookings = await conn.QueryAsync<TourBooking, Customer, TourBooking>(
            sql,
            (booking, customer) =>
            {
                booking.Customer = customer;
                return booking;
            },
            new { DepartureId = departureId },
            splitOn: "Cust_Id"
        );

        return bookings;
    }

    public async Task UpdateBookingStatusAsync(int bookingId, string status)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "UPDATE dbo.TourBooking SET Status = @Status WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = bookingId, Status = status });
    }

    public async Task<IEnumerable<PassengerTicket>> GetTicketsByBookingIdAsync(int bookingId)
    {
        using IDbConnection conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM dbo.PassengerTicket WHERE BookingId = @BookingId ORDER BY Id ASC";
        return await conn.QueryAsync<PassengerTicket>(sql, new { BookingId = bookingId });
    }
}
