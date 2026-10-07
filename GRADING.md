# Đối chiếu tiêu chí Capstone 08 — Tour Du Lịch Huế

Bảng này đối chiếu đề bài PDF với mã nguồn hiện tại. Tình trạng “Cần xác minh” phụ thuộc SQL Server hoặc ảnh chụp chạy thực tế; không xem là đã đạt chỉ vì có mã nguồn.

| Tiêu chí | Vị trí / bằng chứng (số dòng) | Tình trạng |
|---|---|---|
| K1.1 — Clean Architecture, .NET LTS | `HueTour.CoreBusiness/HueTour.CoreBusiness.csproj:4`, `HueTour.UseCases/HueTour.UseCases.csproj:8`, `HueTour.WebApp/HueTour.WebApp.csproj:11`; đăng ký project ở `HueTour.WebApp/Program.cs:20-65` | Build .NET 8 thành công |
| K1.2 — SQL Server, Dapper, tối thiểu 5 bảng, khóa ngoại | `database/HueTour_SchemaAndData.sql:27-136` (7 bảng/FK); truy vấn Dapper tại `HueTour.Plugins.DataStore.SQL/TourRepository.cs:31-38` | Cần xác minh trên SQL Server |
| K1.3 — Luật sức chứa, hạn đặt và giá theo tuổi | `HueTour.CoreBusiness/BusinessRules/BookingBusinessRules.cs:28-70,90-120`; gọi ở `HueTour.UseCases/Booking/PlaceTourBookingUseCase.cs:81,102` | Có kiểm tra số chỗ, hạn 24 giờ và giá theo tuổi |
| K2.1 — Quản lý tour/chuyến/booking | `HueTour.UseCases/Tours/ManageToursUseCase.cs:6-35`; `HueTour.WebApp/Components/Pages/Admin/AdminTours.razor:1-2,187-224`; `AdminDepartures.razor:1-2,51-65`; `AdminBookings.razor:1-2,38-45` | Có các luồng quản trị; cần xác minh đầy đủ trên SQL |
| K2.2 — Transaction header + ticket line | `HueTour.Plugins.DataStore.SQL/TourBookingRepository.cs:21-63` | Header và vé nằm trong transaction; cập nhật chỗ ở `PlaceTourBookingUseCase.cs:151-152` còn ở ngoài transaction |
| K2.3 — Query có tham số | `HueTour.Plugins.DataStore.SQL/TourRepository.cs:31-38,60-92`; `DepartureRepository.cs:31,87-129`; `UserAccountRepository.cs:21-28` | Đã dùng tham số Dapper ở các query đã rà soát |
| K2.4 — End-to-end trên SQL Server | Chọn plugin ở `HueTour.WebApp/Program.cs:20-37`; schema/seed ở `database/HueTour_SchemaAndData.sql` | Chưa thể xác minh khi chưa kết nối SQL Server thật |
| K3.1 — Tối thiểu 3 thành phần giao diện dùng lại | `HueTour.WebApp/Components/Shared/TourCardComponent.razor:1`, `SearchBarComponent.razor:1`, `PassengerItemComponent.razor:1`, `DepartureSelectorComponent.razor:1` | Có ít nhất 3 thành phần |
| K3.2 — EditForm + DataAnnotations | `HueTour.WebApp/Components/Pages/Booking.razor:95-96,204-212` | Có validator và thông báo hợp lệ |
| K3.3 — Báo lỗi luật nghiệp vụ thân thiện | `HueTour.WebApp/Components/Pages/Booking.razor:63-73`; luật tại `BookingBusinessRules.cs:28-70` | Có cảnh báo trong giao diện |
| K3.4 — Responsive 390px | `HueTour.WebApp/wwwroot/app.css:267-310` và các lớp grid Bootstrap | Cần kiểm tra trực quan ở viewport 390px |
| K4.1 — Đăng nhập Customer/Admin | Cookie và endpoint ở `HueTour.WebApp/Program.cs:56-68,92-117`; xác thực ở `HueTour.UseCases/Auth/AuthenticateUserUseCase.cs:8-22`; hash ở `HueTour.CoreBusiness/UserAccountPasswordHasher.cs:6-40` | Đăng nhập Admin đã kiểm tra cục bộ; SQL seed dùng hash PBKDF2 |
| K4.2 — Bảo vệ trang quản trị | `[Authorize]` tại `HueTour.WebApp/Components/Pages/Admin/AdminDashboard.razor:1-2`, `AdminTours.razor:1-2`, `AdminDepartures.razor:1-2`, `AdminBookings.razor:1-2`; middleware `Program.cs:86-87` | Có bảo vệ theo role; cần xác minh với SQL |
| K4.3 — Giỏ đặt tour theo phiên | `HueTour.Plugins.DataStore.InMemory/BookingCartStateService.cs:7`; đăng ký scoped ở `HueTour.WebApp/Program.cs:39` | Có state scoped cho phiên Blazor |
| K5.1 — Sơ đồ Use Case | `docs/diagrams/use-case.puml:1-27` | Đã bổ sung |
| K5.2 — Sơ đồ Class | `docs/diagrams/class.puml:1-26` | Đã bổ sung |
| K5.3 — Sơ đồ Sequence, README, bằng chứng | `docs/diagrams/booking-sequence.puml:1-25`, `README.md`, bảng này | Tài liệu đã bổ sung; ảnh minh chứng cần chụp từ lần chạy thực tế |

## Hạng mục cần hoàn tất trước khi nộp

- Chạy script trên SQL Server dùng riêng cho đồ án rồi xác nhận đặt tour tạo booking và vé; script xóa bảng trùng tên trước khi tạo lại.
- Đưa cập nhật số chỗ vào transaction đặt tour để ngăn các yêu cầu đồng thời vượt sức chứa.
- Chụp giao diện và trang quản trị thật để đính kèm làm bằng chứng; không tạo ảnh giả.
- Mốc commit trong ba tuần phải dựa theo lịch sử thật, không tạo commit hồi tố.
