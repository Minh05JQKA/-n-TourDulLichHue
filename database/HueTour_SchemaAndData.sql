-- =========================================================================================
-- ĐỒ ÁN CAPSTONE ECO2415 - ĐỀ TÀI 08: TOUR DU LỊCH HUẾ (HUE TOUR BOOKING)
-- HỆ CƠ SỞ DỮ LIỆU SQL SERVER - CHUẨN 3NF CÓ KHÓA NGOẠI & DỮ LIỆU MẪU ĐẶC SẮC CỐ ĐÔ HUẾ
-- Đáp ứng tiêu chí K2.1 (>= 5 bảng, 3NF, FK) & K2.2 (Header-Line Transaction)
-- =========================================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HueTourDb')
BEGIN
    CREATE DATABASE [HueTourDb];
END
GO

USE [HueTourDb];
GO

-- XÓA BẢNG NẾU ĐÃ TỒN TẠI (THEO THỨ TỰ KHÓA NGOẠI)
IF OBJECT_ID('dbo.PassengerTicket', 'U') IS NOT NULL DROP TABLE dbo.PassengerTicket;
IF OBJECT_ID('dbo.TourBooking', 'U') IS NOT NULL DROP TABLE dbo.TourBooking;
IF OBJECT_ID('dbo.Departure', 'U') IS NOT NULL DROP TABLE dbo.Departure;
IF OBJECT_ID('dbo.Tour', 'U') IS NOT NULL DROP TABLE dbo.Tour;
IF OBJECT_ID('dbo.PriceRule', 'U') IS NOT NULL DROP TABLE dbo.PriceRule;
IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL DROP TABLE dbo.Customer;
IF OBJECT_ID('dbo.UserAccount', 'U') IS NOT NULL DROP TABLE dbo.UserAccount;
GO

-- 1. BẢNG QUẢN LÝ TÀI KHOẢN VÀ PHÂN QUYỀN (K4.1, K4.2)
CREATE TABLE dbo.UserAccount (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Customer', -- 'Admin', 'Customer'
    Email NVARCHAR(150) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. BẢNG KHÁCH HÀNG
CREATE TABLE dbo.Customer (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    IdentityCard NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO
CREATE NONCLUSTERED INDEX IX_Customer_Phone ON dbo.Customer(PhoneNumber);
CREATE NONCLUSTERED INDEX IX_Customer_Email ON dbo.Customer(Email);
GO

-- 3. BẢNG QUY ĐỊNH GIÁ VÉ THEO LOẠI HÀNH KHÁCH (LUẬT NGHIỆP VỤ 2 - K1.3)
CREATE TABLE dbo.PriceRule (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PassengerType NVARCHAR(50) NOT NULL UNIQUE, -- 'Adult', 'Child', 'Infant'
    DisplayName NVARCHAR(100) NOT NULL,
    RateMultiplier DECIMAL(5,2) NOT NULL DEFAULT 1.00, -- 1.0 (100%), 0.5 (50%), 0.0 (Miễn phí)
    MinAge INT NOT NULL,
    MaxAge INT NULL,
    Description NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- 4. BẢNG TOUR DU LỊCH HUẾ
CREATE TABLE dbo.Tour (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(255) NOT NULL,
    Category NVARCHAR(100) NOT NULL, -- Di tích Cố Đô, Sinh thái & Đầm phá, Thiên nhiên & Nghỉ dưỡng, Văn hóa & Ẩm thực, Sông Hương & Làng nghề
    ShortDescription NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Itinerary NVARCHAR(MAX) NOT NULL,
    Highlights NVARCHAR(1000) NOT NULL,
    Duration NVARCHAR(50) NOT NULL DEFAULT '1 Ngày',
    StartingLocation NVARCHAR(200) NOT NULL DEFAULT 'Trung tâm TP Huế',
    BasePrice DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 5. BẢNG CHUYẾN KHỞI HÀNH CỐ ĐỊNH (DEPARTURE) (LUẬT NGHIỆP VỤ 1 - K1.3)
CREATE TABLE dbo.Departure (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TourId INT NOT NULL,
    DepartureDate DATETIME2 NOT NULL,
    ReturnDate DATETIME2 NOT NULL,
    MaxCapacity INT NOT NULL DEFAULT 25,
    BookedSeats INT NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- 'Open' (Mở bán), 'Closed' (Chốt đoàn), 'Completed', 'Cancelled'
    MeetingPoint NVARCHAR(255) NOT NULL DEFAULT 'Bến thuyền Tòa Khâm / Phu Văn Lâu',
    TourGuideName NVARCHAR(100) NULL,
    TourGuidePhone NVARCHAR(20) NULL,
    CONSTRAINT FK_Departure_Tour FOREIGN KEY (TourId) REFERENCES dbo.Tour(Id) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX IX_Departure_TourDate ON dbo.Departure(TourId, DepartureDate);
GO

-- 6. BẢNG ĐƠN ĐẶT TOUR (HEADER - K2.2)
CREATE TABLE dbo.TourBooking (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(50) NOT NULL UNIQUE,
    DepartureId INT NOT NULL,
    CustomerId INT NOT NULL,
    BookingDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Completed', 'Cancelled'
    Notes NVARCHAR(500) NULL,
    SpecialRequests NVARCHAR(500) NULL,
    PaymentMethod NVARCHAR(100) NOT NULL DEFAULT 'Thanh toán trực tiếp / QR Code',
    CONSTRAINT FK_Booking_Departure FOREIGN KEY (DepartureId) REFERENCES dbo.Departure(Id),
    CONSTRAINT FK_Booking_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer(Id)
);
GO
CREATE NONCLUSTERED INDEX IX_TourBooking_Code ON dbo.TourBooking(BookingCode);
CREATE NONCLUSTERED INDEX IX_TourBooking_Departure ON dbo.TourBooking(DepartureId);
GO

-- 7. BẢNG VÉ HÀNH KHÁCH (LINEITEM - K2.2)
CREATE TABLE dbo.PassengerTicket (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NOT NULL,
    TicketCode NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Gender NVARCHAR(20) NOT NULL DEFAULT 'Nam',
    PassengerType NVARCHAR(50) NOT NULL, -- 'Adult', 'Child', 'Infant'
    BirthYear INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    SeatNumber NVARCHAR(20) NULL,
    Note NVARCHAR(255) NULL,
    CONSTRAINT FK_Ticket_Booking FOREIGN KEY (BookingId) REFERENCES dbo.TourBooking(Id) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX IX_Ticket_Booking ON dbo.PassengerTicket(BookingId);
GO

-- =========================================================================================
-- DỮ LIỆU MẪU KHỞI TẠO (SEED DATA)
-- =========================================================================================

-- Seed Tài khoản
INSERT INTO dbo.UserAccount (Username, PasswordHash, FullName, Role, Email, PhoneNumber)
VALUES
('admin', 'pbkdf2-sha256$210000$TYpsCxfi81mkwnGOZQO5Dw==$ee1h28DQ2N+52LzN226i2AjDWQbHpxT7I/OYftdo7rU=', N'Quản Trị Viên Tour Huế', 'Admin', 'admin@huetour.vn', '0905123456'),
('khachhang', 'pbkdf2-sha256$210000$oZw35LYF0o9xo8lOC1bygQ==$NLXpsp0sk3NANY+daDzdrc6sVxJXu441JuHH/VsI5xQ=', N'Nguyễn Văn An', 'Customer', 'an.nguyen@gmail.com', '0912345678');

-- Seed PriceRule (Luật 2: Người lớn 100%, Trẻ em 5-11t 50%, Em bé <5t 0% miễn phí)
INSERT INTO dbo.PriceRule (PassengerType, DisplayName, RateMultiplier, MinAge, MaxAge, Description, IsActive)
VALUES
('Adult', N'Người lớn', 1.00, 12, NULL, N'Hành khách từ 12 tuổi trở lên tính 100% giá gốc', 1),
('Child', N'Trẻ em (5 - 11 tuổi)', 0.50, 5, 11, N'Trẻ em từ 5 đến 11 tuổi tính 50% giá gốc', 1),
('Infant', N'Em bé (Dưới 5 tuổi)', 0.00, 0, 4, N'Em bé dưới 5 tuổi miễn phí vé tour (0 VNĐ)', 1);

-- Seed Tours
INSERT INTO dbo.Tour (Code, Title, Category, ShortDescription, Description, Itinerary, Highlights, Duration, StartingLocation, BasePrice, ImageUrl, IsActive)
VALUES
(
    'HT-DAINOI01',
    N'Tour Khám Phá Di Sản Hoàng Cung Đại Nội & Các Lăng Tẩm Triều Nguyễn',
    N'Di tích Cố Đô',
    N'Hành trình xuyên qua 143 năm vàng son của triều Nguyễn, chiêm ngưỡng Ngọ Môn, Điện Thái Hòa, Lăng Khải Định tráng lệ và Lăng Tự Đức thơ mộng.',
    N'Trải nghiệm văn hóa di sản hoàng gia trọn vẹn trong ngày. Bạn sẽ được hướng dẫn viên thuyết minh chi tiết về điển tích, nghệ thuật kiến trúc cung đình đỉnh cao, thưởng ngoạn cảnh quan sơn thủy hữu tình.',
    N'08:00 Đón khách tại trung tâm TP Huế -> 08:30 Tham quan Hoàng Thành Đại Nội (Ngọ Môn, Điện Thái Hòa, Tử Cấm Thành) -> 11:30 Thưởng thức bữa trưa cơm niêu Cố Đô -> 13:30 Viếng Lăng Khải Định lộng lẫy -> 15:30 Chiêm bái Lăng Tự Đức thi vị -> 17:00 Trả khách tại điểm hẹn.',
    N'Vé vào cổng trọn gói các điểm di tích; Xe du lịch đời mới máy lạnh; Thuyết minh viên chuyên nghiệp am hiểu lịch sử; Ăn trưa đặc sản Huế.',
    N'1 Ngày',
    N'Cột cờ Phu Văn Lâu, TP Huế',
    550000,
    '/images/imperial-city-2025.jpg',
    1
),
(
    'HT-TAMGIANG02',
    N'Tour Hoàng Hôn Đầm Phá Tam Giang & Trải Nghiệm Chèo Sup Đầm Chuồn',
    N'Sinh thái & Đầm phá',
    N'Ngắm nhìn phá Tam Giang - vùng đầm phá nước lợ lớn nhất Đông Nam Á lung linh trong ánh hoàng hôn, thưởng thức hải sản đầm Chuồn tươi sống.',
    N'Rời xa khói bụi thành phố, cùng ngư dân lênh đênh trên mặt nước mênh mông, đổ nò bắt tôm cá, chèo Sup thư giãn và thưởng thức tiệc hải sản tươi rói ngay tại nhà chồ giữa đầm.',
    N'14:00 Xe đón khách tại trung tâm Huế -> 14:45 Đến bến đầm Chuồn, lên thuyền tham quan -> 15:30 Trải nghiệm đổ nò, quăng chài bắt cá tôm cùng ngư dân -> 16:30 Chèo Sup, chụp ảnh hoàng hôn rực rỡ -> 18:00 Tiệc hải sản tươi sống tại nhà hàng nổi -> 19:30 Trở về TP Huế.',
    N'Thuyền du lịch đầm phá an toàn; Chèo Sup và áo phao miễn phí; Bữa tối hải sản đầm phá cực ngon (tôm, ghẹ, bánh khoái cá kình); Chụp ảnh hoàng hôn.',
    N'Nửa Ngày (Chiều tối)',
    N'Bến thuyền Đầm Chuồn, Phú Vang',
    480000,
    '/images/tam-giang-2024.jpg',
    1
),
(
    'HT-SONGHUONG03',
    N'Du Thuyền Sông Hương Ngắm Cố Đô & Nghe Ca Huế Thính Phòng Về Đêm',
    N'Sông Hương & Làng nghề',
    N'Thả hồn theo dòng Hương giang thơ mộng trên thuyền rồng đôi, lắng nghe điệu Nam ai, Nam bình da diết và thả hoa đăng cầu may mắn.',
    N'Ca Huế là Di sản văn hóa phi vật thể quốc gia. Trong không gian tĩnh lặng lung linh ánh đèn của cầu Tràng Tiền, những giai điệu nhã nhạc thính phòng đưa bạn về với hoài niệm Cố Đô sâu lắng.',
    N'19:00 Đón khách tại bến Tòa Khâm -> 19:30 Thuyền rồng rời bến, xuôi dòng Hương qua cầu Tràng Tiền, cầu Phú Xuân -> 20:00 Thưởng thức đêm biểu diễn Ca Huế của các nghệ sĩ lão thành -> 20:45 Trải nghiệm thả hoa đăng giấy màu -> 21:15 Cập bến Tòa Khâm.',
    N'Vé thuyền rồng đôi ngắm sông Hương; Đoàn nghệ nhân biểu diễn Ca Huế mộc; Mỗi khách được tặng hoa đăng thả sông; Trà cung đình Huế.',
    N'3 Giờ',
    N'Bến thuyền Tòa Khâm, 49 Lê Lợi, TP Huế',
    220000,
    'https://upload.wikimedia.org/wikipedia/commons/thumb/e/e1/Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg/1280px-Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg',
    1
),
(
    'HT-THUYBIEU04',
    N'Tour Sinh Thái Làng Cổ Thủy Biều & Ngâm Chân Thảo Dược Thanh Trà',
    N'Sông Hương & Làng nghề',
    N'Đạp xe dưới bóng râm mát rượi của những vườn thanh trà trĩu quả, tham quan nhà rường cổ hàng trăm năm tuổi và ngâm chân thảo mộc thảo dược làng quê.',
    N'Hành trình chữa lành và thư giãn tuyệt đối bên dòng sông Hương. Khám phá nét đẹp mộc mạc của làng cổ Thủy Biều, học làm mè xửng, làm bánh gói truyền thống Huế.',
    N'08:30 Đón khách tại chùa Thiên Mụ -> 09:00 Đi thuyền ngược dòng Hương lên Thủy Biều -> 09:30 Đạp xe quanh đường làng rợp bóng cây -> 10:30 Trải nghiệm làm bánh lọc, gói bánh nậm -> 11:30 Thư giãn ngâm chân thảo dược lá bưởi thanh trà -> 12:30 Ăn trưa ẩm thực nhà vườn.',
    N'Thuyền đưa đón dọc sông Hương; Xe đạp miễn phí suốt hành trình; Ngâm chân nước ấm thảo dược vườn nhà; Thưởng thức thanh trà và bữa trưa quê.',
    N'Nửa Ngày',
    N'Chùa Thiên Mụ / Làng Thủy Biều',
    390000,
    '/images/thien-mu-2025.jpg',
    1
),
(
    'HT-AMTHUCDEM05',
    N'Food Tour Ẩm Thực Cố Đô Đêm Bằng Xe Xích Lô Truyền Thống',
    N'Văn hóa & Ẩm thực',
    N'Ngồi xích lô dạo qua 36 phố phường đêm Huế, nếm thử 10 món ăn đặc sản trứ danh từ bánh bèo, nậm, lọc đến bún bò Huế và chè hẻm 20 món.',
    N'Huế là cái nôi ẩm thực tinh hoa của Việt Nam. Chuyến đi đưa bạn đến đúng những quán ăn gia truyền có tuổi đời hàng chục năm, nếm vị đậm đà cay nồng đúng chất Huế.',
    N'17:30 Bác xích lô đón khách tại khách sạn -> 18:00 Thưởng thức Bánh bèo - nậm - lọc bà Đỏ -> 19:00 Ăn Bún bò mụ Rơi nổi tiếng -> 20:00 Bánh ép Thuận An giòn rụm -> 20:45 Giải nhiệt với chè hẻm Cung Đình 20 món -> 21:30 Dạo mát quanh sông Hương và về lại điểm hẹn.',
    N'Xe xích lô và tài xế suốt buổi tối; Trọn gói chi phí thưởng thức 10 món ăn; Hướng dẫn viên bản địa sành ăn dẫn đường.',
    N'4 Giờ',
    N'Đón tại các khách sạn trung tâm TP Huế',
    420000,
    '/images/bun-bo-hue-2024.jpg',
    1
),
(
    'HT-BACHMA06',
    N'Tour Trekking Vườn Quốc Gia Bạch Mã & Chinh Phục Ngũ Hồ - Hải Vọng Đài',
    N'Sinh thái & Đầm phá',
    N'Khám phá thiên nhiên kỳ vĩ tại Bạch Mã, tắm suối mát lạnh ở cụm Ngũ Hồ và phóng tầm mắt chiêm ngưỡng toàn cảnh phá Cầu Hai từ đỉnh Hải Vọng Đài.',
    N'Dành cho những tâm hồn yêu thiên nhiên hoang sơ và vận động. Khí hậu mát lạnh quanh năm tựa Đà Lạt giữa lòng Cố Đô, ngắm thác Đỗ Quyên hùng vĩ cao hơn 300 mét.',
    N'07:30 Xe đón tại trung tâm Huế -> 09:00 Đến chân núi Bạch Mã -> 10:00 Chinh phục Hải Vọng Đài độ cao 1.450m -> 11:30 Trekking đường mòn Ngũ Hồ, ăn trưa picnic bên suối -> 13:30 Chiêm ngưỡng thác Đỗ Quyên tráng lệ -> 15:30 Xuống núi -> 17:30 Về đến TP Huế.',
    N'Xe trung chuyển hai chiều và xe chuyên dụng lên đỉnh Bạch Mã; Vé tham quan vườn quốc gia; Bữa trưa picnic thơm ngon; Hướng dẫn viên trekking có kinh nghiệm.',
    N'1 Ngày',
    N'Trung tâm TP Huế',
    680000,
    '/images/bach-ma-waterfall.jpg',
    1
),
(
    'HT-TUDUC07',
    N'Tour Lăng Tự Đức – Làng Hương Thủy Xuân – Đồi Vọng Cảnh',
    N'Di tích Cố Đô',
    N'Khám phá Lăng Tự Đức, sắc màu làng hương Thủy Xuân và ngắm sông Hương từ đồi Vọng Cảnh.',
    N'Hành trình kết hợp di sản và làng nghề truyền thống. Du khách tham quan lăng Tự Đức, tìm hiểu nghề làm hương tại Thủy Xuân và dừng chân ngắm cảnh sông Hương từ đồi Vọng Cảnh.',
    N'08:00 Đón khách tại trung tâm Huế -> 08:30 Tham quan Lăng Tự Đức -> 10:30 Ghé làng hương Thủy Xuân, trò chuyện cùng nghệ nhân -> 11:30 Ngắm cảnh tại đồi Vọng Cảnh -> 12:30 Trả khách tại trung tâm.',
    N'Di tích Lăng Tự Đức; Trải nghiệm làng nghề làm hương; Ngắm sông Hương từ đồi Vọng Cảnh; Hướng dẫn viên địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 520000, '/images/tu-duc-tomb.jpg', 1
),
(
    'HT-THANHTOAN08',
    N'Tour Cầu Ngói Thanh Toàn – Chợ Quê – Làng Hoa Giấy Thanh Tiên',
    N'Sông Hương & Làng nghề',
    N'Thăm cây cầu ngói cổ, khám phá nhịp sống chợ quê và tìm hiểu nghề làm hoa giấy truyền thống xứ Huế.',
    N'Một buổi khám phá vùng quê Huế với kiến trúc cầu ngói Thanh Toàn, không gian chợ quê và hoạt động tìm hiểu nghề làm hoa giấy tại làng Thanh Tiên.',
    N'08:00 Đón khách tại trung tâm Huế -> 08:45 Tham quan Cầu Ngói Thanh Toàn và nhà trưng bày nông cụ -> 10:00 Dạo chợ quê -> 10:45 Đến làng Thanh Tiên, tìm hiểu cách làm hoa giấy -> 12:00 Thưởng thức bữa trưa địa phương -> 13:00 Trở về trung tâm.',
    N'Cầu ngói Thanh Toàn; Không gian chợ quê; Gặp gỡ nghệ nhân làng hoa giấy Thanh Tiên; Thưởng thức món ăn địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 450000, '/images/thanh-toan-2018.jpg', 1
),
(
    'HT-LANGCO09',
    N'Tour Biển Lăng Cô – Đầm Lập An – Hải Vân Quan',
    N'Sinh thái & Đầm phá',
    N'Kết hợp biển xanh Lăng Cô, cảnh đầm Lập An dưới chân núi và điểm dừng ngắm cảnh Hải Vân Quan.',
    N'Khám phá vùng ven biển phía nam Huế với các điểm dừng nổi bật: ngắm cảnh đầm Lập An, thư giãn tại vịnh Lăng Cô và tham quan Hải Vân Quan. Lịch trình có thời gian di chuyển đường dài, phù hợp cho chuyến đi trong ngày.',
    N'07:00 Đón khách tại trung tâm Huế -> 08:30 Dừng ngắm cảnh tại Hải Vân Quan -> 10:00 Tham quan đầm Lập An -> 11:30 Ăn trưa hải sản tại Lăng Cô -> 13:00 Nghỉ ngơi và dạo biển -> 15:00 Khởi hành về Huế -> 17:00 Trả khách.',
    N'Vịnh biển Lăng Cô; Cảnh quan đầm Lập An; Hải Vân Quan; Xe đưa đón trong ngày và hướng dẫn viên.',
    N'1 Ngày', N'Trung tâm TP Huế', 850000, '/images/lang-co-2023.jpg', 1
),
(
    'HT-MINHMANG10',
    N'Tour Di Tích Lăng Minh Mạng – Lăng Gia Long – Đồi Vọng Cảnh',
    N'Di tích Cố Đô',
    N'Khám phá lăng tẩm uy nghiêm nhất triều Nguyễn, chiêm ngưỡng kiến trúc phong thủy đăng đối đỉnh cao bên dòng Hương giang.',
    N'Hành trình đưa quý khách đến với Lăng Gia Long giữa rừng thông bạt ngàn và Lăng Minh Mạng thanh tịnh, tôn nghiêm.',
    N'08:00 Đón khách tại trung tâm TP Huế -> 08:45 Khám phá Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng uy nghiêm -> 11:45 Ngắm sông Hương từ Đồi Vọng Cảnh -> 12:30 Thưởng thức bữa trưa đặc sản Huế.',
    N'Xe du lịch đời mới; Vé tham quan các lăng; Hướng dẫn viên am tường lịch sử; Nước suối và khăn lạnh.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 520000, '/images/minh-mang-2024.jpg', 1
),
(
    'HT-NONLA11',
    N'Tour Trải Nghiệm Làng Nghề Nón Lá Tây Hồ & Hoa Giấy Thanh Tiên',
    N'Sông Hương & Làng nghề',
    N'Tự tay chằm chiếc nón bài thơ xứ Huế và học gấp hoa sen giấy Thanh Tiên cùng các nghệ nhân lão thành.',
    N'Tour văn hóa làng nghề truyền thống đặc sắc. Du khách được tự tay làm nón bài thơ và hoa giấy truyền thống mang về làm kỷ niệm.',
    N'08:30 Đón khách -> 09:00 Đến làng nón Tây Hồ, trải nghiệm chằm nón bài thơ -> 10:30 Thăm làng hoa giấy Thanh Tiên -> 11:30 Thưởng trà sen và bánh đậu xanh trái cây -> 12:30 Trả khách.',
    N'Tự tay làm nón bài thơ mang về; Trò chuyện với nghệ nhân; Xe đưa đón; Trà sen cung đình.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 420000, '/images/hue-conical-hat.jpg', 1
),
(
    'HT-SUOIVOI12',
    N'Tour Sinh Thái Suối Voi – Đầm Lập An – Vịnh Lăng Cô',
    N'Sinh thái & Đầm phá',
    N'Tuyến Suối Voi – Lập An – Lăng Cô đang tạm ẩn trong khi xác minh tình trạng khai thác tại điểm đến.',
    N'Tuyến gợi ý phía Nam Huế được giữ trong dữ liệu capstone để quản trị tham khảo. Suối Voi đang có dự án du lịch dang dở và thông tin về hoạt động tại điểm cần được xác nhận trước khi mở bán lại.',
    N'Tạm ngưng nhận đặt chỗ. Chỉ kích hoạt lại sau khi xác minh được hoạt động, dịch vụ và điều kiện tham quan tại Suối Voi.',
    N'Đầm Lập An; Vịnh Lăng Cô; Tình trạng Suối Voi cần xác minh trước khi tổ chức.',
    N'1 Ngày', N'Trung tâm TP Huế', 720000, '/images/lang-co-2023.jpg', 0
),
(
    'HT-DAINOI-THIENMU13', N'Đại Nội – Chùa Thiên Mụ – Nhà vườn An Hiên', N'Di tích Cố Đô',
    N'Khám phá Hoàng thành, ghé ngôi chùa bên sông Hương và tìm hiểu nếp nhà vườn Huế.',
    N'Lịch trình gợi ý kết hợp di sản trung tâm, cảnh quan sông Hương và kiến trúc nhà vườn. Thứ tự điểm đến có thể điều chỉnh theo giờ mở cửa và điều kiện thực tế.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Đại Nội -> 11:30 Nghỉ trưa -> 13:30 Tham quan chùa Thiên Mụ -> 15:00 Ghé nhà vườn An Hiên -> 16:30 Kết thúc tại trung tâm.',
    N'Đại Nội Huế; chùa Thiên Mụ; nhà vườn An Hiên; tuyến di sản trong thành phố.',
    N'1 Ngày', N'Trung tâm TP Huế', 590000, '/images/thien-mu-2025.jpg', 1
),
(
    'HT-RUCHATAMGIANG14', N'Bình Minh Rú Chá – Phá Tam Giang – Làng chài', N'Sinh thái & Đầm phá',
    N'Ngắm bình minh giữa rừng ngập mặn Rú Chá và tìm hiểu nhịp sống ven phá Tam Giang.',
    N'Chuyến đi gợi ý dành cho người thích thiên nhiên và đời sống địa phương. Thời điểm ngắm bình minh, hoạt động trên thuyền và điểm dừng cần xác nhận trước với đơn vị vận hành.',
    N'05:00 Đón tại trung tâm -> 05:45 Dạo rừng ngập mặn Rú Chá -> 07:00 Ăn sáng địa phương -> 08:00 Tham quan vùng phá Tam Giang và làng chài -> 10:30 Trở về Huế.',
    N'Rừng ngập mặn Rú Chá; cảnh quan đầm phá; tìm hiểu sinh kế địa phương; khung giờ bình minh.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 460000, 'https://hue.gov.vn/Portals/0/Uploads/00.00.H57/Nam2025/Thang6/z5862841503198458c3a50b5d9fa36ebde75e83e601fed_17271732773451054397863_28_4_2025_17_12_01_832_CH.jpg?v=1773705600033', 1
),
(
    'HT-SONGHUONGAMTHUC15', N'Dạo phố Huế – Chợ Đông Ba – Ẩm thực địa phương', N'Văn hóa & Ẩm thực',
    N'Đi bộ qua những điểm phố trung tâm, ghé chợ Đông Ba và thưởng thức các món Huế theo mùa.',
    N'Tuyến tham quan gợi ý kết hợp cảnh quan đô thị và ẩm thực. Món ăn, quán dừng và thời lượng có thể thay đổi theo ngày; giá đang hiển thị là dữ liệu mẫu.',
    N'16:00 Hẹn gần cầu Trường Tiền -> 16:30 Dạo bờ sông Hương -> 17:00 Tham quan khu vực chợ Đông Ba -> 18:00 Thưởng thức các món địa phương -> 19:30 Kết thúc tại trung tâm.',
    N'Cầu Trường Tiền; chợ Đông Ba; món ăn địa phương; trải nghiệm đi bộ trung tâm.',
    N'4 Giờ', N'Cầu Trường Tiền, TP Huế', 350000, '/images/dong-ba-market-2024.jpg', 1
),
(
    'HT-LANGMINHMANG16', N'Lăng Gia Long – Lăng Minh Mạng – Lăng Khải Định', N'Di tích Cố Đô',
    N'So sánh dấu ấn kiến trúc và cảnh quan của ba lăng vua Nguyễn trong chuyến đi một ngày.',
    N'Tuyến di sản gợi ý nối các lăng tẩm ở phía nam thành phố. Thời gian di chuyển giữa các điểm khá dài, cần kiểm tra giờ mở cửa, vé và phương tiện trước ngày đi.',
    N'07:30 Hẹn tại trung tâm -> 08:30 Tham quan Lăng Gia Long -> 10:30 Tham quan Lăng Minh Mạng -> 12:00 Nghỉ trưa -> 14:00 Tham quan Lăng Khải Định -> 16:30 Trở về trung tâm.',
    N'Lăng Gia Long; lăng Minh Mạng; lăng Khải Định; kiến trúc và cảnh quan lăng tẩm triều Nguyễn.',
    N'1 Ngày', N'Trung tâm TP Huế', 690000, '/images/minh-mang-2024.jpg', 1
),
(
    'HT-THUANAN-TAMGIANG17', N'Biển Thuận An – Phá Tam Giang – Đầm Chuồn', N'Sinh thái & Đầm phá',
    N'Kết hợp dạo biển Thuận An với cảnh quan đầm phá và trải nghiệm ẩm thực vùng ven Huế.',
    N'Lịch trình gợi ý theo mùa, kết hợp vùng biển và đầm phá phía đông Huế. Điều kiện thời tiết, tắm biển và hoạt động thuyền cần được xác nhận trước chuyến đi.',
    N'08:00 Hẹn tại trung tâm -> 08:45 Dạo biển Thuận An -> 11:00 Di chuyển đến vùng phá Tam Giang -> 12:00 Ăn trưa địa phương -> 14:00 Tham quan Đầm Chuồn -> 16:00 Trở về Huế.',
    N'Biển Thuận An; cảnh quan phá Tam Giang; Đầm Chuồn; ẩm thực địa phương.',
    N'1 Ngày', N'Trung tâm TP Huế', 560000, '/images/thuan-an-beach-2023.jpg', 1
),
(
    'HT-HOQUYEN18', N'Hổ Quyền – Điện Voi Ré – Lăng Tự Đức', N'Di tích Cố Đô',
    N'Tìm hiểu dấu tích trường đấu voi độc đáo, Điện Voi Ré và không gian lăng Tự Đức.',
    N'Tuyến di sản gợi ý ở phía tây nam Huế, nối Hổ Quyền với Điện Voi Ré và lăng Tự Đức. Cần kiểm tra giờ tham quan, vé và phương tiện trước khi khởi hành.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:30 Tham quan Hổ Quyền -> 09:45 Tìm hiểu Điện Voi Ré -> 11:00 Tham quan lăng Tự Đức -> 12:30 Kết thúc tại trung tâm.',
    N'Di tích Hổ Quyền; Điện Voi Ré; lăng Tự Đức; kiến trúc và câu chuyện triều Nguyễn.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 420000, '/images/ho-quyen-2008.jpg', 1
),
(
    'HT-HONCHEN19', N'Điện Hòn Chén – Đồi Vọng Cảnh – Sông Hương', N'Sông Hương & Làng nghề',
    N'Ghé không gian tín ngưỡng Điện Hòn Chén và ngắm khúc sông Hương từ đồi Vọng Cảnh.',
    N'Hành trình gợi ý kết hợp điểm di tích bên sông và cảnh quan đồi Vọng Cảnh. Nếu đi thuyền, cần xác nhận bến, giờ chạy, điều kiện thời tiết và chương trình lễ hội với đơn vị tổ chức.',
    N'07:30 Hẹn tại trung tâm Huế -> 08:15 Tham quan Điện Hòn Chén -> 10:30 Dừng ngắm cảnh tại đồi Vọng Cảnh -> 11:30 Trở về trung tâm. Phương án đường thủy chỉ thực hiện khi được xác nhận.',
    N'Điện Hòn Chén; đồi Vọng Cảnh; cảnh quan sông Hương; trải nghiệm văn hóa tín ngưỡng địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 390000, '/images/hon-chen-2008.jpg', 1
),
(
    'HT-THANHTAN20', N'Suối khoáng nóng Thanh Tân – Nghỉ dưỡng giữa thiên nhiên', N'Thiên nhiên & Nghỉ dưỡng',
    N'Dành một ngày thư giãn với trải nghiệm suối khoáng nóng ở phía tây bắc Huế.',
    N'Tuyến nghỉ dưỡng tham khảo đến khu suối khoáng nóng Thanh Tân, cách trung tâm Huế khoảng 30 km. Hạng mục dịch vụ, giờ hoạt động, vé và phương tiện cần xác nhận trực tiếp với khu du lịch trước chuyến đi.',
    N'08:00 Khởi hành từ trung tâm Huế -> 09:00 Đến khu vực Thanh Tân -> 09:30–14:00 Tham quan và sử dụng dịch vụ đang mở -> 15:00 Trở về Huế. Hoạt động cụ thể phụ thuộc dịch vụ được xác nhận.',
    N'Khu suối khoáng nóng Thanh Tân; cảnh quan thiên nhiên; lịch trình nghỉ dưỡng trong ngày; dịch vụ phụ thuộc tình trạng vận hành.',
    N'1 Ngày', N'Trung tâm TP Huế', 750000, 'https://huetourism.gov.vn/Medias/Nam2023/Thang8/suoi-khoang-nong-thanh-tan.jpg', 1
),
(
    'HT-VANCU21', N'Làng bún Vân Cù – Phố cổ Bao Vinh – Làng Kim Long', N'Văn hóa & Ẩm thực',
    N'Tìm hiểu nghề làm bún truyền thống, dạo phố Bao Vinh và khám phá nếp sống ven sông.',
    N'Tuyến trải nghiệm cộng đồng gợi ý kết nối làng nghề Vân Cù, khu phố cổ Bao Vinh và làng Kim Long. Hoạt động gặp nghệ nhân, làm thử và nếm món cần đặt trước, tùy lịch của cộng đồng địa phương.',
    N'08:00 Hẹn tại trung tâm Huế -> 08:45 Tham quan làng nghề bún Vân Cù -> 10:30 Dạo khu phố Bao Vinh -> 11:30 Ghé làng Kim Long, tìm hiểu nghề và ẩm thực địa phương -> 13:00 Trở về trung tâm.',
    N'Nghề làm bún Vân Cù; không gian phố Bao Vinh; làng Kim Long; trải nghiệm ẩm thực và đời sống địa phương.',
    N'Nửa Ngày', N'Trung tâm TP Huế', 430000, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/bd/Bao_Vinh_street.jpg/1280px-Bao_Vinh_street.jpg', 1
),
(
    'HT-HONCHAO22', N'Đảo Hòn Chảo – Vịnh Lăng Cô', N'Sinh thái & Đầm phá',
    N'Ngắm cảnh biển Lăng Cô và trải nghiệm vùng đảo Hòn Chảo theo điều kiện thời tiết.',
    N'Tuyến biển tham khảo kết hợp vịnh Lăng Cô với hoạt động tham quan Hòn Chảo. Tàu thuyền, lặn ngắm san hô và các dịch vụ phải được xác nhận với đơn vị có tổ chức; lịch trình phụ thuộc thời tiết và an toàn hàng hải.',
    N'07:00 Khởi hành từ trung tâm Huế -> 08:30 Đến vùng Lăng Cô -> 09:00 Tham quan Hòn Chảo nếu tàu được phép hoạt động và thời tiết phù hợp -> 12:00 Nghỉ trưa -> 14:00 Trở về Huế. Nếu điều kiện không đảm bảo, đổi lịch hoặc dừng hoạt động.',
    N'Vịnh Lăng Cô; cảnh quan đảo Hòn Chảo; hoạt động biển theo điều kiện thực tế; ưu tiên an toàn thời tiết.',
    N'1 Ngày', N'Trung tâm TP Huế', 980000, '/images/lang-co-2023.jpg', 1
),
(
    'HT-DISANDEM23', N'Đại Nội Huế về đêm – Trải nghiệm di sản theo chương trình', N'Di tích Cố Đô',
    N'Gợi ý khám phá không gian Hoàng cung vào buổi tối khi có chương trình được công bố.',
    N'Một số chương trình đêm và dạ yến cung đình được tổ chức theo sự kiện hoặc lịch riêng. Tuyến này chỉ là gợi ý trong capstone; không mặc định hoạt động hằng ngày và cần xác nhận chương trình, vé, thời gian với đơn vị tổ chức.',
    N'Lịch trình thay đổi theo chương trình được công bố. Chỉ đặt chỗ sau khi xác nhận ngày tổ chức, vé vào cửa và nội dung dịch vụ.',
    N'Không gian Hoàng cung; hoạt động văn hóa buổi tối theo lịch; cần xác nhận chương trình trước khi đặt.',
    N'Theo chương trình', N'Đại Nội Huế', 650000, '/images/hue-imperial-night-2024.jpg', 1
);

-- Seed Chuyến khởi hành (Departures)
-- Lưu ý: Tạo chuyến có đủ thời gian đặt bình thường, chuyến trong thời hạn khóa và chuyến gần đầy để test Luật 1.
DECLARE @Now DATETIME2 = GETDATE();

-- Chuyến 1 cho Tour Đại Nội (Cách 3 ngày, còn 18 chỗ) -> Đặt được
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 9, @Now)), 25, 7, 'Open', N'Cột cờ Phu Văn Lâu', N'Lê Văn Hoàng', '0905111222');

-- Chuyến 2 cho Tour Đại Nội (Cách 6 ngày, còn 22 chỗ) -> Đặt được
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 6, @Now), DATEADD(DAY, 6, DATEADD(HOUR, 9, @Now)), 25, 3, 'Open', N'Cột cờ Phu Văn Lâu', N'Nguyễn Thị Thu', '0905333444');

-- Chuyến 3 cho Tour Đại Nội (ĐÃ ĐẦY CHỖ: MaxCapacity 25, BookedSeats 25) -> TEST LUẬT 1: Quá số chỗ!
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 9, @Now)), 25, 25, 'Open', N'Cột cờ Phu Văn Lâu', N'Trần Minh Đức', '0905555666');

-- Chuyến 4 cho Tour Đại Nội còn ít giờ trước khởi hành -> TEST LUẬT 1: Thời hạn khóa đặt!
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (1, DATEADD(HOUR, 10, @Now), DATEADD(HOUR, 19, @Now), 25, 12, 'Open', N'Cột cờ Phu Văn Lâu', N'Lê Văn Hoàng', '0905111222');

-- Chuyến cho Tour Đầm Phá Tam Giang
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (2, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 6, @Now)), 20, 8, 'Open', N'Bến thuyền Đầm Chuồn', N'Phan Thanh Sang', '0905777888');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (2, DATEADD(DAY, 5, @Now), DATEADD(DAY, 5, DATEADD(HOUR, 6, @Now)), 20, 2, 'Open', N'Bến thuyền Đầm Chuồn', N'Phan Thanh Sang', '0905777888');

-- Chuyến cho Tour Du Thuyền Sông Hương Nghe Ca Huế
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (3, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 3, @Now)), 30, 14, 'Open', N'Bến thuyền Tòa Khâm', N'Hoàng Yến Nhi', '0905999000');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (3, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 3, @Now)), 30, 6, 'Open', N'Bến thuyền Tòa Khâm', N'Hoàng Yến Nhi', '0905999000');

-- Chuyến cho Tour Thủy Biều Làng Cổ
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (4, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 5, @Now)), 15, 5, 'Open', N'Chùa Thiên Mụ', N'Đặng Văn Phú', '0905222111');

-- Chuyến cho Tour Food Tour Đêm Huế
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (5, DATEADD(DAY, 2, @Now), DATEADD(DAY, 2, DATEADD(HOUR, 4, @Now)), 16, 4, 'Open', N'Cổng Khách sạn Hương Giang', N'Võ Thị Mỹ Linh', '0905444333');

-- Chuyến cho Tour Bạch Mã Trekking
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (6, DATEADD(DAY, 5, @Now), DATEADD(DAY, 5, DATEADD(HOUR, 10, @Now)), 20, 6, 'Open', N'Trung tâm TP Huế', N'Bùi Quang Dũng', '0905666777');

-- Lịch cho các tour khác
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (7, DATEADD(HOUR, 8, DATEADD(DAY, 4, CAST(@Now AS DATE))), DATEADD(MINUTE, 270, DATEADD(HOUR, 8, DATEADD(DAY, 4, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (8, DATEADD(HOUR, 8, DATEADD(DAY, 3, CAST(@Now AS DATE))), DATEADD(HOUR, 13, DATEADD(DAY, 3, CAST(@Now AS DATE))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (9, DATEADD(HOUR, 7, DATEADD(DAY, 5, CAST(@Now AS DATE))), DATEADD(HOUR, 17, DATEADD(DAY, 5, CAST(@Now AS DATE))), 20, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Chuyến cho Tour 10, 11, 12
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (10, DATEADD(DAY, 4, @Now), DATEADD(DAY, 4, DATEADD(HOUR, 5, @Now)), 20, 5, 'Open', N'Cột cờ Phu Văn Lâu', N'Phan Thanh Sang', '0905777888');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (11, DATEADD(DAY, 3, @Now), DATEADD(DAY, 3, DATEADD(HOUR, 4, @Now)), 16, 4, 'Open', N'Chợ Đông Ba', N'Lê Thị Thu', '0905333444');

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (12, DATEADD(DAY, 7, @Now), DATEADD(DAY, 7, DATEADD(HOUR, 9, @Now)), 25, 0, 'Closed', N'Trung tâm TP Huế', N'Bùi Quang Dũng', '0905666777');

-- Lịch khởi hành mẫu cho các tour gợi ý bổ sung (13-17)
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (13, DATEADD(HOUR, 8, DATEADD(DAY, 5, CAST(@Now AS DATE))), DATEADD(MINUTE, 510, DATEADD(HOUR, 8, DATEADD(DAY, 5, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (14, DATEADD(HOUR, 5, DATEADD(DAY, 6, CAST(@Now AS DATE))), DATEADD(MINUTE, 330, DATEADD(HOUR, 5, DATEADD(DAY, 6, CAST(@Now AS DATE)))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (15, DATEADD(HOUR, 16, DATEADD(DAY, 3, CAST(@Now AS DATE))), DATEADD(HOUR, 20, DATEADD(DAY, 3, CAST(@Now AS DATE))), 12, 0, 'Open', N'Cầu Trường Tiền', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (16, DATEADD(MINUTE, 450, DATEADD(DAY, 8, CAST(@Now AS DATE))), DATEADD(MINUTE, 990, DATEADD(DAY, 8, CAST(@Now AS DATE))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES (17, DATEADD(HOUR, 8, DATEADD(DAY, 9, CAST(@Now AS DATE))), DATEADD(HOUR, 16, DATEADD(DAY, 9, CAST(@Now AS DATE))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Lịch mẫu cho các tour gợi ý bổ sung (18-22); chỉ dùng thử luồng đặt chỗ capstone.
INSERT INTO dbo.Departure (TourId, DepartureDate, ReturnDate, MaxCapacity, BookedSeats, Status, MeetingPoint, TourGuideName, TourGuidePhone)
VALUES
(18, DATEADD(HOUR, 8, DATEADD(DAY, 6, CAST(@Now AS DATE))), DATEADD(MINUTE, 270, DATEADD(HOUR, 8, DATEADD(DAY, 6, CAST(@Now AS DATE)))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(19, DATEADD(MINUTE, 450, DATEADD(DAY, 7, CAST(@Now AS DATE))), DATEADD(HOUR, 12, DATEADD(DAY, 7, CAST(@Now AS DATE))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(20, DATEADD(HOUR, 8, DATEADD(DAY, 8, CAST(@Now AS DATE))), DATEADD(HOUR, 15, DATEADD(DAY, 8, CAST(@Now AS DATE))), 18, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(21, DATEADD(HOUR, 8, DATEADD(DAY, 5, CAST(@Now AS DATE))), DATEADD(HOUR, 13, DATEADD(DAY, 5, CAST(@Now AS DATE))), 16, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL),
(22, DATEADD(HOUR, 7, DATEADD(DAY, 10, CAST(@Now AS DATE))), DATEADD(HOUR, 16, DATEADD(DAY, 10, CAST(@Now AS DATE))), 12, 0, 'Open', N'Trung tâm TP Huế', N'Công ty điều phối', NULL);

-- Seed Khách hàng mẫu
INSERT INTO dbo.Customer (FullName, Email, PhoneNumber, Address)
VALUES
(N'Trần Minh Tuấn', 'tuan.tran@gmail.com', '0988776655', N'12 Lê Lợi, TP Huế'),
(N'Lê Thị Mai Anh', 'maianh.le@gmail.com', '0977889900', N'84 Nguyễn Huệ, TP Huế'),
(N'Nguyễn Hoàng Long', 'long.nguyen@outlook.com', '0903112233', N'158 Phố Huế, Q. Hai Bà Trưng, Hà Nội'),
(N'Vũ Minh Châu', 'minhchau.vu@gmail.com', '0918445566', N'45 Nguyễn Đình Chiểu, Q.1, TP. Hồ Chí Minh'),
(N'Đặng Thu Thảo', 'thuthao.dang@gmail.com', '0935667788', N'22 Bạch Đằng, Q. Hải Châu, Đà Nẵng'),
(N'Hoàng Đức Anh', 'ducanh.hoang@gmail.com', '0983990011', N'08 Hùng Vương, TP Huế');

-- Seed Booking mẫu
INSERT INTO dbo.TourBooking (BookingCode, DepartureId, CustomerId, BookingDate, TotalAmount, Status, Notes, SpecialRequests, PaymentMethod)
VALUES
('HUE-202610-001', 1, 1, DATEADD(DAY, -1, @Now), 825000, 'Confirmed', N'Khách gia đình có 1 bé nhỏ', N'Sắp xếp xe ngồi gần cửa sổ', N'Chuyển khoản QR'),
('HUE-202610-002', 5, 3, DATEADD(DAY, -2, @Now), 1200000, 'Confirmed', N'Đoàn tham gia chèo Sup hoàng hôn', N'Áo phao cho trẻ em', N'Chuyển khoản VietQR'),
('HUE-202610-003', 7, 4, DATEADD(DAY, -1, @Now), 550000, 'Pending', N'Muốn ngồi tầng 2 du thuyền rồng', N'Thả hoa đăng giấy màu', N'Thanh toán trực tiếp'),
('HUE-202610-004', 11, 5, DATEADD(DAY, -3, @Now), 1360000, 'Confirmed', N'Đoàn khám phá Bạch Mã', N'Ăn trưa picnic bên suối', N'Chuyển khoản QR'),
('HUE-202610-005', 1, 6, DATEADD(DAY, -5, @Now), 550000, 'Completed', N'Khách đi 1 mình công tác', NULL, N'Thẻ tín dụng'),
('HUE-202610-006', 2, 2, DATEADD(DAY, -1, @Now), 480000, 'Cancelled', N'Bận việc đột xuất xin hủy', NULL, N'Chuyển khoản');

-- Seed Vé mẫu của các Booking trên (Áp dụng chuẩn Luật 2: Người lớn 100%, Trẻ em 50%, Em bé 0%)
-- Booking 1: 1 Người lớn 550k + 1 Trẻ em 275k = 825k
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(1, 'HUE-202610-001-01', N'Trần Minh Tuấn', 'Nam', 'Adult', 1990, 550000, 'G01'),
(1, 'HUE-202610-001-02', N'Trần Tuấn Khang', 'Nam', 'Child', 2018, 275000, 'G02');

-- Booking 2: 2 Người lớn (480k x 2 = 960k) + 1 Trẻ em (240k) + 1 Em bé (0k) = 1.200.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(2, 'HUE-202610-002-01', N'Nguyễn Hoàng Long', 'Nam', 'Adult', 1988, 480000, 'G01'),
(2, 'HUE-202610-002-02', N'Lê Ngọc Bích', 'Nữ', 'Adult', 1991, 480000, 'G02'),
(2, 'HUE-202610-002-03', N'Nguyễn Hoàng Nam', 'Nam', 'Child', 2016, 240000, 'G03'),
(2, 'HUE-202610-002-04', N'Nguyễn Ngọc An', 'Nữ', 'Infant', 2023, 0, 'G04');

-- Booking 3: 2 Người lớn (220k x 2 = 440k) + 1 Trẻ em (110k) = 550.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(3, 'HUE-202610-003-01', N'Vũ Minh Châu', 'Nữ', 'Adult', 1995, 220000, 'G05'),
(3, 'HUE-202610-003-02', N'Phạm Nhật Minh', 'Nam', 'Adult', 1993, 220000, 'G06'),
(3, 'HUE-202610-003-03', N'Phạm Minh Nhi', 'Nữ', 'Child', 2019, 110000, 'G07');

-- Booking 4: 2 Người lớn (680k x 2) = 1.360.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(4, 'HUE-202610-004-01', N'Đặng Thu Thảo', 'Nữ', 'Adult', 1997, 680000, 'G01'),
(4, 'HUE-202610-004-02', N'Nguyễn Quang Vinh', 'Nam', 'Adult', 1996, 680000, 'G02');

-- Booking 5: 1 Người lớn = 550.000 VNĐ
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(5, 'HUE-202610-005-01', N'Hoàng Đức Anh', 'Nam', 'Adult', 1992, 550000, 'G08');

-- Booking 6: 1 Người lớn = 480.000 VNĐ (đã hủy)
INSERT INTO dbo.PassengerTicket (BookingId, TicketCode, FullName, Gender, PassengerType, BirthYear, Price, SeatNumber)
VALUES
(6, 'HUE-202610-006-01', N'Lê Thị Mai Anh', 'Nữ', 'Adult', 1994, 480000, 'G09');

GO

PRINT N'Cơ sở dữ liệu HueTourDb đã được khởi tạo và nạp dữ liệu mẫu hoàn tất thành công!';

