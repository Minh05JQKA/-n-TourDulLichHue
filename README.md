# Hue Tour Booking

Website đặt tour du lịch Huế bằng Blazor Server và .NET 8 LTS, tổ chức theo Clean Architecture. Dự án gồm tour mẫu, tìm kiếm, chi tiết tour, đặt chỗ theo đoàn, quy tắc giá theo tuổi, tài khoản Customer/Admin và repository cho SQL Server hoặc bộ nhớ.

## Thông tin đồ án

| Mục | Thông tin |
|---|---|
| Họ và tên | Lương Quang Hoàng Minh |
| MSSV | 23K4080038 |
| Lớp / Khóa | K57HTTTQL |
| Môn học | Lập Trình Web / Phát Triển Ứng Dụng Web (.NET) |
| Giảng viên hướng dẫn | Hà Ngọc Long |
| Khoa / Trường | Khoa Tin Học Kinh Tế, Đại Học Kinh Tế Huế |
| Ngày hoàn thành / nộp bài | 29/09/2026 |
| GitHub | [Minh05JQKA](https://github.com/Minh05JQKA) · [Repository Hue Tour](https://github.com/Minh05JQKA/-n-TourDulLichHue) |

## Mở và chạy

1. Cài .NET 8 SDK và mở `HueTourApp.sln` bằng Visual Studio 2022.
2. Chọn `HueTour.WebApp` làm Startup Project. Project này có biểu tượng web và file `HueTour.WebApp.csproj`.
3. Nhấn **F5**. Môi trường Development dùng dữ liệu mẫu trong bộ nhớ; ứng dụng không cần SQL Server để xem giao diện.

Nếu Visual Studio báo *“A project with an Output Type of Class Library cannot be started directly”*, hiện bạn đang chạy nhầm một project thư viện. Trong Solution Explorer, nhấp phải `HueTour.WebApp` rồi chọn **Set as Startup Project**, sau đó nhấn **F5**. Các project `CoreBusiness`, `UseCases` và `Plugins` chỉ là thư viện được ứng dụng web sử dụng.

Solution cũng có profile `Run HueTour.WebApp` trong `HueTourApp.slnLaunch` cho Visual Studio 2022 bản 17.11 trở lên khi bật **Enable Multi-Project Launch Profiles**. Nếu profile chưa hiện trên thanh chạy, dùng menu chuột phải **Set as Startup Project** ở trên.

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
2. Mở `database/HueTour_SchemaAndData.sql` trong SSMS và chạy trên đúng SQL Server của đồ án. Script chỉ tạo `HueTourDb` khi database chưa tồn tại và sẽ dừng nếu database đã có để tránh xóa dữ liệu. Tám bảng vật lý và cột dùng tên tiếng Việt; các view tên tiếng Anh giữ tương thích với ứng dụng. Khi chạy tệp UTF-8 bằng `sqlcmd`, truyền `-f 65001` để giữ nguyên tiếng Việt.
3. Sau khi tạo schema, chạy `database/seed/DiaDiemAmThuc_Seed.sql` để nạp 600 địa điểm ẩm thực. Tệp seed này thay thế dữ liệu trong bảng `DiaDiemAmThuc`.
4. Tạo sơ đồ trong Object Explorer → `HueTourDb` → `Database Diagrams` → `New Database Diagram`, rồi chọn tám bảng tiếng Việt để xem đầy đủ các bảng và khóa ngoại.
5. Cấu hình chuỗi kết nối `ConnectionStrings:HueTourDb` trong `HueTour.WebApp/appsettings.json` theo SQL Server trên máy bạn (máy hiện tại báo server name `Minnh`, default instance; ví dụ `Server=localhost;Database=HueTourDb;Trusted_Connection=True;TrustServerCertificate=True`).
6. Để bật SQL trong môi trường Development mà không sửa cấu hình dùng chung, tạo `HueTour.WebApp/appsettings.Local.json`; tệp này được bỏ qua bởi Git và tự nạp sau cấu hình môi trường. Đặt `UseSqlDatabase` thành `true` và thêm chuỗi kết nối máy cục bộ của bạn vào tệp này. Nếu không có tệp local, Development tiếp tục chạy bằng dữ liệu InMemory.
7. Đặt thông tin VietQR thật trong cùng tệp local, gồm `VietQrPayment.BankCode`, `BankName`, `AccountHolder`, `AccountNumber`, `QrImagePath` và `TransferContentTemplate`. Không commit thông tin ngân hàng thật hoặc ảnh QR cá nhân. Có thể thay từng giá trị bằng biến môi trường `HUETOUR_BANK_CODE`, `HUETOUR_BANK_NAME`, `HUETOUR_BANK_ACCOUNT_NUMBER`, `HUETOUR_BANK_ACCOUNT_HOLDER`, `HUETOUR_BANK_QR_IMAGE_PATH` và `HUETOUR_BANK_TRANSFER_CONTENT_TEMPLATE`.
8. Mọi thông tin ngân hàng có trong repository chỉ là placeholder/demo, không phải thông tin thật. Cấu hình thông tin thật trong `appsettings.Local.json` (đã được Git bỏ qua) hoặc các biến môi trường ở bước trên; không commit số tài khoản, tên chủ tài khoản hay ảnh QR cá nhân. Ứng dụng vẫn hiển thị hướng dẫn và nội dung chuyển khoản mà không cần ảnh QR local.

Tài khoản được seed trong SQL là `admin` / `Admin@123` và `khachhang` / `User@123`. Mật khẩu seed đã được băm PBKDF2.

## Cấu trúc

- `HueTour.CoreBusiness`: mô hình miền, luật đặt tour và băm mật khẩu.
- `HueTour.UseCases`: ca sử dụng và các interface repository.
- `HueTour.Plugins.DataStore.InMemory`: dữ liệu và repository mẫu.
- `HueTour.Plugins.DataStore.SQL`: truy cập SQL Server bằng Dapper.
- `HueTour.WebApp`: giao diện Blazor Server, xác thực cookie và phân quyền.
- `database/HueTour_SchemaAndData.sql`: schema và dữ liệu minh họa.
- `docs/diagrams/relational-schema.puml`: sơ đồ quan hệ CSDL SQL Server và các khóa chính/ngoại.
- Trong SSMS, mở **Database Diagrams > New Database Diagram** và thêm tám bảng tiếng Việt; SQL Server sẽ tự hiện các quan hệ từ khóa ngoại trong script.
- `docs/diagrams`: các sơ đồ PlantUML theo yêu cầu đồ án.
- `GRADING.md`: bảng đối chiếu tiêu chí với vị trí triển khai và phần còn cần xác minh.
- `HueTour.WebApp/wwwroot/images`: ảnh Huế đã lưu cùng dự án; ghi công tại `ASSETS.md`.

Giỏ đặt chỗ dùng state theo phạm vi phiên Blazor. Hệ thống áp dụng quy tắc không vượt sức chứa, khóa đặt chỗ theo hạn định của luật nghiệp vụ và tính giá trẻ em theo độ tuổi.

## Kiểm chứng đặt tour trên SQL Server

1. Đóng ứng dụng đang chạy để giải phóng các DLL, sau đó chạy `dotnet build HueTourApp.sln -m:1` và xác nhận build hoàn tất không có lỗi.
2. Chạy `database/HueTour_SchemaAndData.sql` trong SSMS, nạp `database/seed/DiaDiemAmThuc_Seed.sql`, đặt `UseSqlDatabase=true` trong `HueTour.WebApp/appsettings.Local.json`, rồi chạy ứng dụng.
3. Chọn chuyến gần đầy hoặc khởi hành trong thời hạn khóa đặt, thử đặt tour và xác nhận thông báo thân thiện xuất hiện, không có booking/vé mới và số ghế không đổi.
4. Đặt chuyến hợp lệ và xác nhận booking cùng `PassengerTicket` được tạo, đồng thời `Departure.BookedSeats` tăng đúng số hành khách.

SQL Server thật cần thiết để xác minh transaction và truy vấn cập nhật số ghế.

### Kết quả kiểm chứng gần nhất

Ngày 22/10/2026, app chạy với plugin SQL Server trên localhost. Một booking thử được tạo từ giao diện; truy vấn SQL xác nhận có header và một vé, số ghế đã đặt tăng từ 0 lên 1. Sau đó `ManageBookingsUseCase` cùng repository SQL hủy booking; truy vấn xác nhận trạng thái `Cancelled`, vé vẫn được lưu, số ghế trở về 0 và không có chuyến nào lệch giữa số vé với số ghế. Do phiên trình duyệt bị dừng, bước hủy được gọi bằng harness cục bộ thay vì nút hủy trên giao diện. Hai ca vi phạm thời hạn đặt và sức chứa chưa được demo SQL.

## Khám phá Huế: bản đồ, thời tiết và cẩm nang

Trang mới `/kham-pha` bổ sung tìm địa điểm quanh thành phố hoặc một điểm tham quan đang có trên bản đồ, dự báo thời tiết 7 ngày, cẩm nang Huế, lịch trình theo sở thích và gợi ý từ trợ lý. Trang tiếp tục dùng dữ liệu tour hiện tại của HueTour; danh sách quán và điểm tham quan được lấy trực tiếp từ OpenStreetMap.

### Nguồn dữ liệu và cấu hình

| Nguồn | Dữ liệu | API key |
|---|---|---|
| [Nominatim](https://nominatim.openstreetmap.org/) | Chuyển tên thành phố/khu vực người dùng nhập thành tọa độ | Không |
| [Overpass API](https://dev.overpass-api.de/overpass-doc/en/) + [OpenStreetMap](https://www.openstreetmap.org/copyright) | Nhà hàng, quán cà phê, khách sạn, điểm tham quan, tên/địa chỉ, tọa độ và thẻ giờ mở cửa/ảnh/giá nếu cộng đồng đã ghi | Không |
| [Open-Meteo](https://open-meteo.com/en/docs) | Thời tiết hiện tại và dự báo 7 ngày, gồm nhiệt độ, cảm giác như, độ ẩm, gió và xác suất mưa | Không cần cho sử dụng phi thương mại thông thường |
| [Leaflet](https://leafletjs.com/) + bản đồ nền OpenStreetMap | Bản đồ, điểm đánh dấu và cửa sổ thông tin | Không |

Các địa chỉ dịch vụ có thể thay trong `TravelApis` ở `HueTour.WebApp/appsettings.json` (hoặc nguồn cấu hình .NET tương ứng): `NominatimEndpoint`, `OverpassEndpoints`, `WeatherEndpoint`. Không cần tạo hay lưu API key cho các cấu hình mặc định. Dữ liệu thời tiết được giữ trong bộ nhớ 15 phút; kết quả địa điểm 15 phút; địa chỉ tìm kiếm 24 giờ. Nếu một máy chủ Overpass không phản hồi, ứng dụng thử máy chủ dự phòng.

Nominatim chỉ được gọi sau thao tác tìm kiếm của người dùng, có bộ đệm và giới hạn tối thiểu một giây giữa các truy vấn. Dịch vụ công cộng có giới hạn tải; khi vận hành lượng truy cập lớn, hãy cấu hình dịch vụ bản đồ/thời tiết do mình quản lý hoặc nhà cung cấp có SLA. Không dùng dịch vụ Nominatim công cộng cho tự động gợi ý khi gõ hoặc truy vấn hàng loạt.

Ứng dụng không bịa giá, đánh giá, giờ mở cửa hay ảnh địa điểm. Khi OpenStreetMap không có trường dữ liệu tương ứng, trang ghi “chưa có dữ liệu”; ảnh dự phòng chỉ là biểu tượng danh mục. Các món ăn trong cẩm nang là nội dung biên tập chung, được phân biệt với quán thật lấy từ API. Nếu tìm địa điểm hoặc thời tiết lỗi, trang hiển thị thông báo và vẫn giữ cẩm nang, tour trong cơ sở dữ liệu cùng gợi ý dự phòng.

Trình duyệt chỉ hỏi vị trí khi người dùng bấm **Vị trí hiện tại**. Tọa độ dùng để truy vấn địa điểm/thời tiết và không được lưu trong ứng dụng hay gửi cho trợ lý AI. Nếu đã cấu hình AI từ xa, câu hỏi cùng tên địa điểm, thời tiết và tour hiện trên trang sẽ được gửi tới nhà cung cấp AI đã cấu hình; trang thông báo điều này cạnh câu trả lời.

Giữ dòng ghi công OpenStreetMap trên bản đồ và dòng nguồn Open-Meteo ở phần dự báo. Dịch vụ API và bản đồ công cộng là best-effort, không có cam kết luôn sẵn sàng; giá và độ đầy đủ của thẻ OSM tùy đóng góp cộng đồng.

HueTour hiện là ứng dụng Blazor Server/.NET 8, có cookie đăng nhập và backend dữ liệu. Kiến trúc này cần máy chủ .NET, nên bản hiện tại không thể được triển khai như website tĩnh trên GitHub Pages. Yêu cầu GitHub Pages chỉ áp dụng khi website đang dùng kiến trúc tĩnh; để chuyển dự án này cần một lần triển khai kiến trúc riêng.
