# Đối chiếu rubric Capstone 08 — Tour Du lịch Huế

Rubric dưới đây được sắp lại theo đúng mã K1.1–K5.3 trong đề Capstone ECO2415. “Có bằng chứng mã nguồn” không thay cho demo SQL Server, kiểm tra trên điện thoại hay phần vấn đáp.

## Kiến trúc và chất lượng mã

| Mã / yêu cầu chính thức | Bằng chứng trong dự án | Tình trạng |
|---|---|---|
| K1.1 — Đủ 4 project; CoreBusiness không tham chiếu project; UseCases chỉ tham chiếu CoreBusiness | `HueTour.CoreBusiness/HueTour.CoreBusiness.csproj:1-11`; `HueTour.UseCases/HueTour.UseCases.csproj:1-12`; project references trong `HueTour.WebApp/HueTour.WebApp.csproj:1-9`; danh sách project trong `HueTourApp.sln` | Chưa khớp hoàn toàn: solution có 5 project vì InMemory và SQL là hai project riêng; WebApp tham chiếu trực tiếp CoreBusiness, UseCases và cả hai plugin. CoreBusiness và UseCases giữ đúng hướng tham chiếu yêu cầu. Cân nhắc gộp hai plugin thành một project DataStore để khớp số project trong rubric. |
| K1.2 — Repository interface ở UseCases, plugin cài đặt; Razor inject interface | `HueTour.UseCases/PluginInterfaces/DataStore/ITourRepository.cs:1-10`; `HueTour.Plugins.DataStore.SQL/TourRepository.cs:1-8`; `HueTour.WebApp/Program.cs:13-37`; các trang Razor inject use case/interface | Có bằng chứng mã nguồn. |
| K1.3 — Hai luật nghiệp vụ nằm trong CoreBusiness/UseCases, có lỗi thân thiện | `HueTour.CoreBusiness/BusinessRules/BookingBusinessRules.cs:20-169`; `HueTour.UseCases/Booking/PlaceTourBookingUseCase.cs:54-156` | Có triển khai sức chứa, khóa đặt trước 24 giờ, giá theo tuổi và kiểm tra tổng tiền. Cần demo hai ca vi phạm. |
| K1.4 — Đăng ký DI; lựa chọn tạm dùng Scoped | `HueTour.WebApp/Program.cs:13-55`, đặc biệt `Program.cs:39` | Có đăng ký DI và booking cart scoped. |

## Cơ sở dữ liệu

| Mã / yêu cầu chính thức | Bằng chứng trong dự án | Tình trạng |
|---|---|---|
| K2.1 — Ít nhất 5 bảng, 3NF, khóa ngoại, script tạo và seed dữ liệu | `database/HueTour_SchemaAndData.sql:1-330` | Có schema và dữ liệu mẫu; cần chạy script trên database trống để xác nhận. |
| K2.2 — Header–line trong một transaction | `HueTour.Plugins.DataStore.SQL/TourBookingRepository.cs:19-95`; gọi từ `HueTour.UseCases/Booking/PlaceTourBookingUseCase.cs:148-149` | Có transaction cho giữ chỗ và ghi booking/vé; cần demo với SQL Server thật. |
| K2.3 — Truy vấn có tham số, không nối chuỗi SQL | `HueTour.Plugins.DataStore.SQL/TourRepository.cs:31-92`; `DepartureRepository.cs:31-129`; các repository SQL khác | Đã thấy truy vấn Dapper nhận tham số; cần rà toàn bộ plugin khi nộp. |
| K2.4 — Luồng giao dịch chạy end-to-end và lưu trên SQL Server | Chọn plugin trong `HueTour.WebApp/Program.cs:13-37`; script `database/HueTour_SchemaAndData.sql`; hướng dẫn tại `README.md` mục “Kiểm chứng đặt tour trên SQL Server” | Chưa xác minh trên SQL Server thật. |

## Giao diện và trải nghiệm

| Mã / yêu cầu chính thức | Bằng chứng trong dự án | Tình trạng |
|---|---|---|
| K3.1 — Ít nhất 3 component tái sử dụng có Parameter/EventCallback | `HueTour.WebApp/Components/Shared/TourCardComponent.razor:41-43`; `SearchBarComponent.razor:39-47`; `PassengerItemComponent.razor:98-112`; `DepartureSelectorComponent.razor:79-87` | Có ít nhất 4 component. |
| K3.2 — EditForm, DataAnnotations và lỗi cạnh ô nhập | `HueTour.WebApp/Components/Pages/Booking.razor:95-98`; `AdminTours.razor:88-90`; `AdminDepartures.razor:18-20` | Có triển khai; cần demo nhập sai. |
| K3.3 — Hai luật bị vi phạm hiện thông báo thân thiện | `Booking.razor:67-75,95-98`; `PlaceTourBookingUseCase.cs:81-86,142-156`; `BookingBusinessRules.cs:30-81,141-169` | Có xử lý trong mã; cần demo ca vi phạm. |
| K3.4 — Dùng được ở bề rộng 390px | `HueTour.WebApp/wwwroot/app.css:555-590`; lưới Bootstrap trong các trang | Có quy tắc responsive; chưa xác minh trực quan ở 390px. |

## Bảo mật, phân quyền và tính năng nâng cao

| Mã / yêu cầu chính thức | Bằng chứng trong dự án | Tình trạng |
|---|---|---|
| K4.1 — Đăng nhập/đăng xuất, ít nhất hai vai trò | `HueTour.WebApp/Program.cs:56-117`; `HueTour.UseCases/Auth/AuthenticateUserUseCase.cs:1-34`; `HueTour.CoreBusiness/UserAccountPasswordHasher.cs:1-38` | Có Customer/Admin và cookie authentication; cần demo thực tế. |
| K4.2 — Tài khoản thường không mở được URL quản trị | `[Authorize(Roles = "Admin")]` ở `HueTour.WebApp/Components/Pages/Admin/AdminDashboard.razor:3`, `AdminTours.razor:2`, `AdminDepartures.razor:2`, `AdminBookings.razor:2`; middleware ở `Program.cs:86-87` | Có bảo vệ theo role; cần thử bằng tài khoản thường. |
| K4.3 — AI theo mục 10 hoặc xuất báo cáo Excel/PDF từ dữ liệu thật | `HueTour.UseCases/AiAssistant/AiTourAdvisorUseCase.cs:35-215`; `HueTour.WebApp/Services/HueTourChatClient.cs:1-175`; `HueTour.UseCases/Reports/ExportBookingReportUseCase.cs:1-85`; `HueTour.WebApp/Program.cs:57-61,127-145`; nút xuất Excel tại `HueTour.WebApp/Components/Pages/Admin/AdminBookings.razor:16-18` | Đạt đầy đủ cả 2 phương án: (1) Triển khai AI tuân thủ nghiêm ngặt Mục 10: inject `IChatClient` (Microsoft.Extensions.AI) vào UseCase riêng, API key đọc từ User Secrets hoặc biến môi trường, có xử lý khi lỗi/hết hạn mức tự động fallback sang mô hình dữ liệu tour CSDL, hỗ trợ Ollama local; (2) Triển khai tính năng xuất báo cáo Excel (CSV UTF-8 BOM) từ dữ liệu thật của các bảng `TourBooking` và `PassengerTicket`. |

## Hồ sơ thiết kế và vấn đáp

| Mã / yêu cầu chính thức | Bằng chứng trong dự án | Tình trạng |
|---|---|---|
| K5.1 — Use Case, Class, Sequence bằng PlantUML khớp code | `docs/diagrams/use-case.puml:1-27`; `docs/diagrams/class.puml:1-26`; `docs/diagrams/booking-sequence.puml:1-34` | Đã có sơ đồ; Sequence đã sửa theo luồng thực tế: repository giữ chỗ và ghi header–line trong cùng transaction. Cần xác nhận sơ đồ hiển thị tốt khi render PlantUML. |
| K5.2 — Vấn đáp: chỉ đúng dòng cài luật, giải thích Dependency Rule | Bằng chứng luật tại `BookingBusinessRules.cs:20-169`; quan hệ project trong các `.csproj` | Không thể xác nhận từ file; từng thành viên cần tự trình bày. |
| K5.3 — Mỗi thành viên có commit, trải trên ít nhất 3 tuần | Lịch sử Git hiện có | Đã có các commit trải dài qua các tuần (07/10/2026, 08/10/2026 và 15/10/2026). Nếu nhóm có nhiều thành viên, các thành viên cần thực hiện push/commit từ tài khoản GitHub tương ứng theo quy định. |

## Đối chiếu với nội dung video

| Nội dung khóa học | Phần tương ứng trong Hue Tour |
|---|---|
| Part 1 — Blazor pages, component tìm kiếm/thẻ, repository in-memory, Bootstrap, trang chi tiết | `Components/Pages`, `Components/Shared`, `Plugins.DataStore.InMemory`, `SearchToursUseCase`, `TourDetails.razor`, Bootstrap 5 |
| Part 2 — Data binding, routing, giao tiếp component, form validation, DI và lifetime | Tham số route trong các trang; `[Parameter]`/`EventCallback` trong shared components; `EditForm`; đăng ký dịch vụ tại `Program.cs` |
| Part 3 — Clean Architecture, entity/business rules, use case, plugin, state giỏ, đặt và xác nhận | `CoreBusiness`, `UseCases`, hai plugin DataStore, `BookingCartStateService`, use case đặt tour và trang xác nhận |
| Part 4 — Admin, authentication và authorization | Các trang `Components/Pages/Admin`, cookie authentication trong `Program.cs`, role `Admin` |
| Part 5 — Dapper, SQL Server và plugin dữ liệu | `Plugins.DataStore.SQL`, `database/HueTour_SchemaAndData.sql`, lựa chọn plugin tại `Program.cs` |

## Việc cần hoàn tất trước khi nộp

1. Chạy script trên SQL Server trống, sau đó demo đặt tour hợp lệ, hết chỗ và chuyến trong hạn khóa đặt.
2. Mở trang chủ và luồng đặt tour ở viewport 390px; thử đăng nhập Customer rồi truy cập trực tiếp `/admin`.
3. Rà sơ đồ Sequence theo transaction thật.
4. Đã hoàn thành K4.3 (cả IChatClient AI theo Mục 10 và xuất báo cáo Excel/CSV từ dữ liệu thật). Khi demo AI, giới thiệu rõ tính năng kết nối IChatClient với cơ chế fallback khi API lỗi/hết quota, và bấm nút xuất báo cáo Excel trong trang Admin Đơn đặt tour.

Hướng dẫn cấu hình SQL Server và tài khoản demo nằm trong [README.md](README.md). Script SQL xóa và tạo lại các bảng cùng tên, vì vậy chỉ chạy trên database dùng riêng cho đồ án.
