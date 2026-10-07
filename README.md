# Hue Tour Booking

Website đặt tour du lịch Huế bằng Blazor Server và .NET 8 LTS, tổ chức theo Clean Architecture. Dự án gồm tour mẫu, tìm kiếm, chi tiết tour, đặt chỗ theo đoàn, quy tắc giá theo tuổi, tài khoản Customer/Admin và repository cho SQL Server hoặc bộ nhớ.

## Mở và chạy

1. Cài .NET 8 SDK và mở `HueTourApp.sln` bằng Visual Studio 2022.
2. Chọn `HueTour.WebApp` làm Startup Project.
3. Nhấn **F5**. Môi trường Development dùng dữ liệu mẫu trong bộ nhớ; ứng dụng không cần SQL Server để xem giao diện.

Hoặc chạy từ thư mục dự án:

```powershell
dotnet run --project HueTour.WebApp/HueTour.WebApp.csproj
```

Tài khoản demo của môi trường Development:

- Admin: `admin` / `Admin@123`
- Customer: `khachhang` / `User@123`

Mật khẩu demo được băm PBKDF2 khi ứng dụng khởi động. Không dùng tài khoản này trong môi trường thật.

## Bật SQL Server

1. Cài SQL Server và SQL Server Management Studio.
2. Mở `database/HueTour_SchemaAndData.sql` trong SSMS và chạy trên một database dùng riêng cho đồ án. **Script xóa các bảng hiện có cùng tên trước khi tạo lại và seed dữ liệu**, vì vậy không chạy trên database đang có dữ liệu cần giữ.
3. Chỉnh `ConnectionStrings:HueTourDb` trong `HueTour.WebApp/appsettings.json` theo cấu hình máy bạn.
4. Đổi `UseSqlDatabase` thành `true` trong `HueTour.WebApp/appsettings.Development.json`.
5. Chạy lại ứng dụng.

Tài khoản được seed trong SQL là `admin` / `Admin@123` và `khachhang` / `User@123`. Mật khẩu seed đã được băm PBKDF2.

## Cấu trúc

- `HueTour.CoreBusiness`: mô hình miền, luật đặt tour và băm mật khẩu.
- `HueTour.UseCases`: ca sử dụng và các interface repository.
- `HueTour.Plugins.DataStore.InMemory`: dữ liệu và repository mẫu.
- `HueTour.Plugins.DataStore.SQL`: truy cập SQL Server bằng Dapper.
- `HueTour.WebApp`: giao diện Blazor Server, xác thực cookie và phân quyền.
- `database/HueTour_SchemaAndData.sql`: schema và dữ liệu minh họa.
- `docs/diagrams`: sơ đồ PlantUML theo yêu cầu đồ án.
- `GRADING.md`: bảng đối chiếu tiêu chí với vị trí triển khai và phần còn cần xác minh.
- `HueTour.WebApp/wwwroot/images`: ảnh Huế đã lưu cùng dự án; ghi công tại `ASSETS.md`.

Giỏ đặt chỗ dùng state theo phạm vi phiên Blazor. Hệ thống áp dụng quy tắc không vượt sức chứa, khóa đặt chỗ trước giờ khởi hành 24 tiếng và tính giá trẻ em theo độ tuổi.
