# Đối chiếu tiêu chí Capstone 08 — Tour Du Lịch Huế

Bảng này đối chiếu tiêu chí với bằng chứng trong mã nguồn. Hạng mục phụ thuộc SQL Server thật được giữ ở trạng thái cần xác minh.

| Tiêu chí | Vị trí / bằng chứng (số dòng) | Tình trạng |
|---|---|---|
| K1.1 — Clean Architecture, .NET 8 | `HueTour.CoreBusiness/HueTour.CoreBusiness.csproj:4`; `HueTour.UseCases/HueTour.UseCases.csproj:3-4,8`; đăng ký DI tại `HueTour.WebApp/Program.cs:12-65` | Đạt — `dotnet build HueTourApp.sln -m:1` thành công, 0 warning, 0 error; CoreBusiness không có ProjectReference, UseCases chỉ tham chiếu CoreBusiness |
| K1.2 — SQL Server, Dapper, tối thiểu 5 bảng, khóa ngoại | `database/HueTour_SchemaAndData.sql:27-136`; Dapper tại `HueTour.Plugins.DataStore.SQL/TourRepository.cs:31-38` | Cần xác minh trên SQL Server |
| K1.3 — Sức chứa, hạn đặt, giá vé | `HueTour.CoreBusiness/BusinessRules/BookingBusinessRules.cs:20-70,90-166`; gọi kiểm tra giá tại `HueTour.UseCases/Booking/PlaceTourBookingUseCase.cs:142` | Đạt theo mã nguồn; luồng SQL thật cần xác minh |
| K2.1 — CRUD tour/chuyến/booking | `HueTour.UseCases/Tours/ManageToursUseCase.cs:6-35`; `HueTour.WebApp/Components/Pages/Admin/AdminTours.razor:88-90,160-180,237-259`; `AdminDepartures.razor:18-28,50-58,61-76`; `AdminBookings.razor:1-45` | Có triển khai; cần xác minh CRUD đầy đủ với SQL Server |
| K2.2 — Transaction booking, vé và giữ ghế | `HueTour.Plugins.DataStore.SQL/TourBookingRepository.cs:22-89` (giữ ghế ở `30-53`, ghi header/line ở `55-87`, commit ở `89`); gọi với số ghế tại `HueTour.UseCases/Booking/PlaceTourBookingUseCase.cs:149` | Đạt theo mã nguồn; cần xác minh transaction trên SQL Server thật |
| K2.3 — Query có tham số | `HueTour.Plugins.DataStore.SQL/TourRepository.cs:31-38,60-92`; `DepartureRepository.cs:31,87-129`; `UserAccountRepository.cs:21-28` | Đã dùng tham số Dapper tại các truy vấn được dẫn |
| K2.4 — End-to-end SQL Server | Cấu hình lựa chọn plugin tại `HueTour.WebApp/Program.cs:13-38`; schema/seed tại `database/HueTour_SchemaAndData.sql` | Cần xác minh trên SQL Server thật |
| K3.1 — Thành phần giao diện dùng lại | `TourCardComponent.razor:1`; `SearchBarComponent.razor:1`; `PassengerItemComponent.razor:1`; `DepartureSelectorComponent.razor:1` | Đạt — có ít nhất 3 thành phần |
| K3.2 — EditForm và DataAnnotations | `HueTour.WebApp/Components/Pages/Booking.razor:95-97`; `AdminTours.razor:88-90`; `AdminDepartures.razor:18-19,61-76` | Đạt theo mã nguồn |
| K3.3 — Báo lỗi nghiệp vụ thân thiện | `Booking.razor:67-75,97`; `PlaceTourBookingUseCase.cs:80-82,142-146`; quy tắc tại `BookingBusinessRules.cs:28-70,139-166` | Đạt theo mã nguồn |
| K3.4 — Responsive 390px | `HueTour.WebApp/wwwroot/app.css:282-294`; lưới Bootstrap trong các trang | Cần xác minh trực quan ở viewport 390px |
| K4.1 — Đăng nhập Customer/Admin | `HueTour.WebApp/Program.cs:56-68,92-117`; `AuthenticateUserUseCase.cs:8-22`; `UserAccountPasswordHasher.cs:6-40` | Có triển khai; đăng nhập thực tế cần xác minh theo cấu hình database |
| K4.2 — Bảo vệ trang quản trị | `[Authorize]` tại `AdminDashboard.razor:1-2`, `AdminTours.razor:1-2`, `AdminDepartures.razor:1-2`, `AdminBookings.razor:1-2`; middleware `Program.cs:86-87` | Đạt theo cấu hình mã nguồn; cần xác minh luồng đăng nhập thực tế |
| K4.3 — Giỏ đặt tour theo phiên | Đăng ký scoped `Program.cs:39`; giữ tour/chuyến tại `TourDetails.razor:205`; nạp lại trạng thái tại `Booking.razor:274-289` và đồng bộ hành khách/khách hàng sau booking thành công tại `Booking.razor:376-409` | Đạt theo luồng mã nguồn |
| K5.1 — Sơ đồ Use Case | `docs/diagrams/use-case.puml:1-27` | Đã bổ sung |
| K5.2 — Sơ đồ Class | `docs/diagrams/class.puml:1-26` | Đã bổ sung |
| K5.3 — Sơ đồ Sequence, README, hướng dẫn | `docs/diagrams/booking-sequence.puml:1-25`; `README.md` phần “Kiểm chứng đặt tour trên SQL Server” | Hướng dẫn đã bổ sung; ảnh minh chứng cần chụp từ lần chạy thực tế |

## Hướng dẫn kiểm chứng

Các bước chạy build và demo SQL Server nằm trong [README.md](README.md), mục “Kiểm chứng đặt tour trên SQL Server”. Cần SQL Server thật để xác nhận không tạo booking/vé khi từ chối giữ ghế và số ghế tăng đúng khi booking hợp lệ.
