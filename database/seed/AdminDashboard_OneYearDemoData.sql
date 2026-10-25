-- Du lieu demo mot nam cho dashboard quan tri HueTour.
-- Chay sau database/HueTour_SchemaAndData.sql tren database HueTourDb.
-- Cac don co ma HUEDEMO-1Y-* la du lieu gia lap, khong phai booking khach that.
-- Script co the chay lai: khong tao trung khach, chuyen, don hoac ve.

USE [HueTourDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Today date = CONVERT(date, GETDATE());
DECLARE @CurrentMonth date = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
DECLARE @FirstMonth date = DATEADD(MONTH, -11, @CurrentMonth);
DECLARE @CustomerCount int = 120;
DECLARE @TourCount int;
DECLARE @RecentBookingCount int = CASE WHEN DAY(@Today) < 5 THEN DAY(@Today) ELSE 5 END;

DECLARE @Tours TABLE
(
    TourOrdinal int NOT NULL PRIMARY KEY,
    TourId int NOT NULL UNIQUE,
    Category nvarchar(100) NOT NULL,
    Duration nvarchar(50) NOT NULL,
    BasePrice decimal(18,2) NOT NULL
);

DECLARE @Months TABLE
(
    MonthOffset int NOT NULL PRIMARY KEY,
    MonthStart date NOT NULL UNIQUE,
    BookingCount int NOT NULL,
    DepartureAt datetime2(0) NOT NULL
);

DECLARE @DepartureMap TABLE
(
    MonthStart date NOT NULL,
    TourId int NOT NULL,
    DepartureId int NOT NULL,
    PRIMARY KEY (MonthStart, TourId)
);

DECLARE @Numbers TABLE (Number int NOT NULL PRIMARY KEY);
;WITH NumberSeries AS
(
    SELECT 1 AS Number
    UNION ALL
    SELECT Number + 1
    FROM NumberSeries
    WHERE Number < @CustomerCount
)
INSERT INTO @Numbers (Number)
SELECT Number FROM NumberSeries
OPTION (MAXRECURSION 120);

BEGIN TRANSACTION;

-- Don cac booking demo cu hon cua so 12 thang truoc; chi xoa cac ma do script nay tao.
DELETE ticket
FROM dbo.PassengerTicket AS ticket
INNER JOIN dbo.TourBooking AS booking ON booking.Id = ticket.BookingId
WHERE booking.BookingCode LIKE N'HUEDEMO-1Y-%'
  AND booking.BookingDate < CONVERT(datetime2(0), @FirstMonth);

DELETE FROM dbo.TourBooking
WHERE BookingCode LIKE N'HUEDEMO-1Y-%'
  AND BookingDate < CONVERT(datetime2(0), @FirstMonth);

-- Don demo da het han va khong con booking nao duoc giu lai cung chuyen.
DELETE departure
FROM dbo.Departure AS departure
WHERE departure.TourGuideName = N'Điều phối demo một năm'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.TourBooking AS booking
      WHERE booking.DepartureId = departure.Id
  );

-- Tao 120 ho so khach demo co email example.com de tranh nham voi khach that.
;WITH CustomerNumbers AS
(
    SELECT Number
    FROM @Numbers
    WHERE Number <= @CustomerCount
)
INSERT INTO dbo.Customer (FullName, Email, PhoneNumber, Address, UserAccountId)
SELECT
    CONCAT(N'Khách demo Huế ', RIGHT(CONCAT('000', CONVERT(varchar(3), Number)), 3)),
    CONCAT('huetour.demo.', RIGHT(CONCAT('000', CONVERT(varchar(3), Number)), 3), '@example.com'),
    CONCAT('090', RIGHT(CONCAT('0000000', CONVERT(varchar(7), Number)), 7)),
    CONCAT(N'Khách du lịch, TP Huế - hồ sơ demo ', Number),
    NULL
FROM CustomerNumbers AS seed
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Customer AS existingCustomer
    WHERE existingCustomer.Email = CONCAT('huetour.demo.', RIGHT(CONCAT('000', CONVERT(varchar(3), seed.Number)), 3), '@example.com')
);

-- Moi danh muc lay mot tour dang mo ban, tranh phu thuoc vao IDENTITY co dinh.
;WITH RankedTours AS
(
    SELECT
        tour.Id,
        tour.Category,
        tour.Duration,
        tour.BasePrice,
        ROW_NUMBER() OVER (PARTITION BY tour.Category ORDER BY tour.Id) AS CategoryRank
    FROM dbo.Tour AS tour
    WHERE tour.IsActive = 1
      AND tour.BasePrice > 0
      AND tour.Code NOT LIKE N'HT-MARKET-%'
),
SelectedTours AS
(
    SELECT TOP (8) Id, Category, Duration, BasePrice
    FROM RankedTours
    WHERE CategoryRank = 1
    ORDER BY Category, Id
)
INSERT INTO @Tours (TourOrdinal, TourId, Category, Duration, BasePrice)
SELECT CONVERT(int, ROW_NUMBER() OVER (ORDER BY Category, Id)), Id, Category, Duration, BasePrice
FROM SelectedTours;

SELECT @TourCount = COUNT(*) FROM @Tours;
IF @TourCount = 0
    THROW 52001, N'Khong tim thay tour dang mo ban de tao du lieu dashboard.', 1;

-- Bao phu 12 thang lich, tu thang hien tai lui 11 thang.
-- Luong dat tour tang vao mua xuan/he va ha vao cuoi nam, tao duong xu huong de doc.
;WITH MonthNumbers AS
(
    SELECT Number - 1 AS MonthOffset
    FROM @Numbers
    WHERE Number <= 12
),
MonthStarts AS
(
    SELECT MonthOffset, DATEADD(MONTH, MonthOffset, @FirstMonth) AS MonthStart
    FROM MonthNumbers
)
INSERT INTO @Months (MonthOffset, MonthStart, BookingCount, DepartureAt)
SELECT
    MonthOffset,
    MonthStart,
    CASE MONTH(MonthStart)
        WHEN 1 THEN 14 WHEN 2 THEN 12 WHEN 3 THEN 16 WHEN 4 THEN 20
        WHEN 5 THEN 22 WHEN 6 THEN 24 WHEN 7 THEN 22 WHEN 8 THEN 19
        WHEN 9 THEN 15 WHEN 10 THEN 14 WHEN 11 THEN 10 WHEN 12 THEN 12
    END,
    DATEADD(MINUTE, 450, CONVERT(datetime2(0), DATEADD(DAY, 19, DATEADD(MONTH, 1, MonthStart))) )
FROM MonthStarts;

-- Moi tour co mot chuyen trong thang, khoi hanh ngay 20 thang tiep theo.
-- Dat gio giua cac thang giup don duoc ghi nhan truoc ngay di va tranh qua han 24 gio.
INSERT INTO dbo.Departure
    (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
SELECT
    tour.TourId,
    period.DepartureAt,
    DATEADD(HOUR,
        CASE
            WHEN tour.Duration LIKE N'%3 Ngày%' THEN 54
            WHEN tour.Duration LIKE N'%2 Ngày%' OR tour.Duration LIKE N'%1 Đêm%' THEN 30
            WHEN tour.Duration LIKE N'%1 Ngày%' THEN 10
            WHEN tour.Duration LIKE N'%4 Giờ%' THEN 4
            WHEN tour.Duration LIKE N'%Nửa Ngày%' THEN 5
            ELSE 8
        END,
        period.DepartureAt),
    32,
    0,
    CASE WHEN period.DepartureAt < DATEADD(DAY, 1, CONVERT(datetime2(0), @Today)) THEN N'Completed' ELSE N'Open' END,
    N'Trung tâm TP Huế',
    N'Điều phối demo một năm',
    NULL
FROM @Months AS period
CROSS JOIN @Tours AS tour
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Departure AS existingDeparture
    WHERE existingDeparture.TourId = tour.TourId
      AND existingDeparture.DepartureDate = period.DepartureAt
      AND existingDeparture.MeetingPoint = N'Trung tâm TP Huế'
      AND existingDeparture.TourGuideName = N'Điều phối demo một năm'
);

INSERT INTO @DepartureMap (MonthStart, TourId, DepartureId)
SELECT period.MonthStart, tour.TourId, departure.Id
FROM @Months AS period
CROSS JOIN @Tours AS tour
INNER JOIN dbo.Departure AS departure
    ON departure.TourId = tour.TourId
   AND departure.DepartureDate = period.DepartureAt
   AND departure.MeetingPoint = N'Trung tâm TP Huế'
   AND departure.TourGuideName = N'Điều phối demo một năm';

-- Khi chay lai sau ngay khoi hanh, cap nhat trang thai cac chuyen va don demo.
UPDATE departure
SET Status = N'Completed'
FROM dbo.Departure AS departure
INNER JOIN @DepartureMap AS departureMap ON departureMap.DepartureId = departure.Id
WHERE departure.Status = N'Open'
  AND departure.DepartureDate < DATEADD(DAY, 1, CONVERT(datetime2(0), @Today));

UPDATE booking
SET Status = N'Completed'
FROM dbo.TourBooking AS booking
INNER JOIN @DepartureMap AS departureMap ON departureMap.DepartureId = booking.DepartureId
INNER JOIN dbo.Departure AS departure ON departure.Id = booking.DepartureId
WHERE booking.BookingCode LIKE N'HUEDEMO-1Y-%'
  AND booking.Status IN (N'Pending', N'Confirmed')
  AND departure.Status = N'Completed';

DECLARE @Bookings TABLE
(
    BookingCode nvarchar(50) NOT NULL PRIMARY KEY,
    MonthStart date NOT NULL,
    SequenceNumber int NOT NULL,
    DepartureId int NOT NULL,
    CustomerId int NOT NULL,
    BookingDate datetime2(0) NOT NULL,
    Status nvarchar(50) NOT NULL,
    BasePrice decimal(18,2) NOT NULL,
    PassengerCount int NOT NULL
);

INSERT INTO @Bookings
    (BookingCode, MonthStart, SequenceNumber, DepartureId, CustomerId, BookingDate, Status, BasePrice, PassengerCount)
SELECT
    CONCAT(N'HUEDEMO-1Y-', CONVERT(char(6), period.MonthStart, 112), N'-', RIGHT(CONCAT('000', CONVERT(varchar(3), number.Number)), 3)),
    period.MonthStart,
    number.Number,
    departureMap.DepartureId,
    customer.Id,
    CASE
        WHEN bookingDay.BookingDay = @Today THEN CONVERT(datetime2(0), GETDATE())
        ELSE DATEADD(MINUTE,
            510 + ((number.Number * 37 + period.MonthOffset * 13) % 720),
            CONVERT(datetime2(0), bookingDay.BookingDay))
    END,
    CASE
        WHEN period.DepartureAt < DATEADD(DAY, 1, CONVERT(datetime2(0), @Today))
            THEN CASE WHEN number.Number % 10 IN (0, 1) THEN N'Cancelled' ELSE N'Completed' END
        WHEN number.Number % 7 = 0 THEN N'Cancelled'
        WHEN number.Number % 5 = 0 THEN N'Pending'
        ELSE N'Confirmed'
    END,
    tour.BasePrice,
    1 + ((number.Number * 7 + period.MonthOffset * 3) % 5)
FROM @Months AS period
INNER JOIN @Numbers AS number ON number.Number <= period.BookingCount
CROSS APPLY
(
    SELECT ((number.Number - 1 + period.MonthOffset * 3) % @TourCount) + 1 AS TourOrdinal
) AS selectedTour
INNER JOIN @Tours AS tour ON tour.TourOrdinal = selectedTour.TourOrdinal
INNER JOIN @DepartureMap AS departureMap ON departureMap.MonthStart = period.MonthStart AND departureMap.TourId = tour.TourId
CROSS APPLY
(
    SELECT CASE
        WHEN period.MonthStart = @CurrentMonth AND number.Number <= @RecentBookingCount
            THEN CONVERT(date, DATEADD(DAY, 1 - number.Number, @Today))
        WHEN period.MonthStart = @CurrentMonth
            THEN DATEADD(DAY,
                (number.Number * 7 + period.MonthOffset * 11) % (CASE WHEN DAY(@Today) > 5 THEN DAY(@Today) - 5 ELSE 1 END),
                period.MonthStart)
        ELSE DATEADD(DAY, (number.Number * 7 + period.MonthOffset * 11) % 20, period.MonthStart)
    END AS BookingDay
) AS bookingDay
INNER JOIN dbo.Customer AS customer
    ON customer.Email = CONCAT('huetour.demo.', RIGHT(CONCAT('000', CONVERT(varchar(3), ((period.MonthOffset * 19 + number.Number - 1) % @CustomerCount) + 1)), 3), '@example.com');

DECLARE @TicketCandidates TABLE
(
    BookingCode nvarchar(50) NOT NULL,
    TicketCode nvarchar(80) NOT NULL PRIMARY KEY,
    DepartureId int NOT NULL,
    TicketNumber int NOT NULL,
    FullName nvarchar(150) NOT NULL,
    Gender nvarchar(20) NOT NULL,
    PassengerType nvarchar(50) NOT NULL,
    BirthYear int NOT NULL,
    Price decimal(18,2) NOT NULL,
    Status nvarchar(50) NOT NULL,
    SeatNumber nvarchar(20) NULL
);

INSERT INTO @TicketCandidates
    (BookingCode, TicketCode, DepartureId, TicketNumber, FullName, Gender, PassengerType, BirthYear, Price, Status)
SELECT
    booking.BookingCode,
    CONCAT(booking.BookingCode, N'-', RIGHT(CONCAT('00', CONVERT(varchar(2), passenger.Number)), 2)),
    booking.DepartureId,
    passenger.Number,
    CASE
        WHEN passenger.Number = 1 THEN customer.FullName
        WHEN passengerType.PassengerType = N'Child' THEN CONCAT(N'Bé đi cùng ', RIGHT(booking.BookingCode, 3))
        ELSE CONCAT(N'Khách đi cùng ', RIGHT(booking.BookingCode, 3), N'-', passenger.Number)
    END,
    CASE WHEN (booking.SequenceNumber + passenger.Number) % 2 = 0 THEN N'Nam' ELSE N'Nữ' END,
    passengerType.PassengerType,
    CASE passengerType.PassengerType
        WHEN N'Child' THEN YEAR(@Today) - 5 - (booking.SequenceNumber % 7)
        WHEN N'Infant' THEN YEAR(@Today) - (booking.SequenceNumber % 2)
        ELSE YEAR(@Today) - 20 - (booking.SequenceNumber % 24)
    END,
    CONVERT(decimal(18,2), booking.BasePrice * CASE passengerType.PassengerType WHEN N'Child' THEN 0.5 WHEN N'Infant' THEN 0 ELSE 1 END),
    booking.Status
FROM @Bookings AS booking
INNER JOIN dbo.Customer AS customer ON customer.Id = booking.CustomerId
INNER JOIN @Numbers AS passenger ON passenger.Number <= booking.PassengerCount
CROSS APPLY
(
    SELECT CASE
        WHEN passenger.Number = 2 AND booking.PassengerCount >= 2 AND booking.SequenceNumber % 6 = 0 THEN N'Child'
        WHEN passenger.Number = 3 AND booking.PassengerCount >= 3 AND booking.SequenceNumber % 17 = 0 THEN N'Infant'
        ELSE N'Adult'
    END AS PassengerType
) AS passengerType;

;WITH SeatNumbers AS
(
    SELECT
        TicketCode,
        ROW_NUMBER() OVER (PARTITION BY DepartureId ORDER BY BookingCode, TicketNumber) AS SeatNumber
    FROM @TicketCandidates
    WHERE Status <> N'Cancelled'
)
UPDATE candidate
SET SeatNumber = CONCAT(N'G', RIGHT(CONCAT('00', CONVERT(varchar(3), seats.SeatNumber)), 2))
FROM @TicketCandidates AS candidate
INNER JOIN SeatNumbers AS seats ON seats.TicketCode = candidate.TicketCode;

-- Tong tien khop voi cac ve: nguoi lon 100%, tre em 50%, em be mien phi.
INSERT INTO dbo.TourBooking
    (BookingCode, DepartureId, CustomerId, BookingDate, TotalAmount, Status, Notes, SpecialRequests, PaymentMethod)
SELECT
    booking.BookingCode,
    booking.DepartureId,
    booking.CustomerId,
    booking.BookingDate,
    COALESCE(SUM(ticket.Price), 0),
    booking.Status,
    N'Dữ liệu demo tổng hợp cho dashboard; không phải đơn đặt thật.',
    NULL,
    CASE booking.SequenceNumber % 3
        WHEN 0 THEN N'Chuyển khoản QR'
        WHEN 1 THEN N'Thanh toán trực tiếp'
        ELSE N'Thẻ tín dụng'
    END
FROM @Bookings AS booking
INNER JOIN @TicketCandidates AS ticket ON ticket.BookingCode = booking.BookingCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.TourBooking AS existingBooking WHERE existingBooking.BookingCode = booking.BookingCode)
GROUP BY booking.BookingCode, booking.DepartureId, booking.CustomerId, booking.BookingDate, booking.Status, booking.SequenceNumber;

-- Luu ve cho cac don demo; ma ve duoc tao co dinh de script chay lai an toan.
INSERT INTO dbo.PassengerTicket
    (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber, Note)
SELECT
    booking.Id,
    ticket.TicketCode,
    ticket.FullName,
    ticket.Gender,
    ticket.PassengerType,
    ticket.BirthYear,
    ticket.Price,
    ticket.SeatNumber,
    N'Vé demo dashboard'
FROM @TicketCandidates AS ticket
INNER JOIN dbo.TourBooking AS booking ON booking.BookingCode = ticket.BookingCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.PassengerTicket AS existingTicket WHERE existingTicket.TicketCode = ticket.TicketCode);

-- So ghe luon bang so ve cua cac don chua huy, khong tinh cac booking da huy.
UPDATE departure
SET BookedSeats = bookingSeats.ActiveSeats
FROM dbo.Departure AS departure
INNER JOIN @DepartureMap AS departureMap ON departureMap.DepartureId = departure.Id
CROSS APPLY
(
    SELECT COUNT(*) AS ActiveSeats
    FROM dbo.TourBooking AS booking
    INNER JOIN dbo.PassengerTicket AS ticket ON ticket.BookingId = booking.Id
    WHERE booking.DepartureId = departure.Id
      AND booking.Status <> N'Cancelled'
) AS bookingSeats
WHERE departure.BookedSeats <> bookingSeats.ActiveSeats;

COMMIT TRANSACTION;

SELECT
    MIN(BookingDate) AS FirstBooking,
    MAX(BookingDate) AS LastBooking,
    COUNT(*) AS DemoBookings,
    SUM(CASE WHEN Status IN (N'Confirmed', N'Completed') THEN TotalAmount ELSE 0 END) AS RecognizedRevenue
FROM dbo.TourBooking
WHERE BookingCode LIKE N'HUEDEMO-1Y-%'
  AND BookingDate >= CONVERT(datetime2(0), @FirstMonth);

SELECT Status, COUNT(*) AS BookingCount
FROM dbo.TourBooking
WHERE BookingCode LIKE N'HUEDEMO-1Y-%'
  AND BookingDate >= CONVERT(datetime2(0), @FirstMonth)
GROUP BY Status
ORDER BY Status;
GO
