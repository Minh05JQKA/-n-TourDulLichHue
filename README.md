# Hue Tour Booking

Website đặt tour du lịch Huế bằng Blazor Server và .NET 8 LTS, tổ chức theo Clean Architecture. Dự án gồm tour mẫu, tìm kiếm, chi tiết tour, đặt chỗ theo đoàn, quy tắc giá theo tuổi, tài khoản Customer/Admin và repository cho SQL Server hoặc bộ nhớ.

## Thông tin đồ án

| Mục | Thông tin |
|---|---|
| Họ và tên | Lương Quang Hoàng Minh |
| Hình thức | Đồ án cá nhân (01 thành viên) |
| MSSV | 23K4080038 |
| Lớp / Khóa | K57HTTTQL |
| Môn học | Lập Trình Web / Phát Triển Ứng Dụng Web (.NET) |
| Giảng viên hướng dẫn | Hà Ngọc Long |
| Khoa / Trường | Khoa Tin Học Kinh Tế, Đại Học Kinh Tế Huế |
| Ngày hoàn thành / nộp bài | 23/10/2026 (bản hiện tại) |
| GitHub | [Minh05JQKA](https://github.com/Minh05JQKA) · [Repository Hue Tour](https://github.com/Minh05JQKA/-n-TourDulLichHue) |

## Công nghệ sử dụng

| Nhóm | Công nghệ | Vai trò trong dự án |
|---|---|---|
| Ngôn ngữ và nền tảng | C#, .NET 8, ASP.NET Core | Toàn bộ solution nhắm `net8.0`; ASP.NET Core khởi chạy website và quản lý cấu hình, dependency injection, HTTP client, xác thực và các endpoint báo cáo. |
| Giao diện ứng dụng | Blazor Server, Razor Components, Interactive Server | Trang được xây bằng component `.razor`; tương tác chạy qua Blazor Server, gồm trang chủ, tour, đặt chỗ, khám phá, ẩm thực và quản trị. |
| Kiến trúc | Clean Architecture, dependency injection | Tách mô hình nghiệp vụ (`HueTour.CoreBusiness`), ca sử dụng và interface (`HueTour.UseCases`), các plugin lưu trữ (`HueTour.Plugins.*`) và giao diện (`HueTour.WebApp`). |
| Cơ sở dữ liệu | Microsoft SQL Server, Dapper `2.1.89`, Microsoft.Data.SqlClient `7.1.1` | Plugin SQL truy vấn và ghi dữ liệu bằng Dapper/SqlClient; script SQL tạo schema, view tương thích, seed và migration. Dự án không dùng Entity Framework Core. |
| Lưu trữ khi phát triển | In-memory repositories | Plugin `HueTour.Plugins.DataStore.InMemory` cung cấp dữ liệu mẫu để chạy giao diện khi chưa cấu hình SQL Server; chọn SQL bằng `UseSqlDatabase=true`. |
| Giao diện và bố cục | HTML, CSS, JavaScript, Bootstrap `5.3.3`, Bootstrap Icons `1.11.3` | Bootstrap và icon được tải từ CDN; CSS riêng tạo giao diện Huế, bố cục responsive, giao diện sáng/tối và các trang đặt tour/khám phá. Font Playfair Display và Plus Jakarta Sans được tải từ Google Fonts. |
| Biểu đồ | Chart.js `4.4.3`, JavaScript interop | `admin-charts.js` vẽ biểu đồ doanh thu, số booking, trạng thái đơn, công suất chuyến và doanh thu theo danh mục từ dữ liệu do Blazor tổng hợp. |
| Bản đồ và dữ liệu địa điểm | Leaflet `1.9.4`, OpenStreetMap, Nominatim, Overpass API | Leaflet hiển thị bản đồ và marker; Nominatim tìm tọa độ theo tên địa điểm, Overpass lấy các điểm được cộng đồng OpenStreetMap đóng góp. Dữ liệu địa điểm Huế đã biên tập trong SQL được giữ làm nguồn dự phòng khi dịch vụ công cộng không phản hồi. |
| Thời tiết | Open-Meteo REST API, `HttpClient`, `MemoryCache` | Lấy thời tiết hiện tại và dự báo; cache kết quả trong bộ nhớ để giảm yêu cầu lặp lại. |
| Trợ lý hành trình | Microsoft.Extensions.AI `9.0.1-preview.1.24570.5`, Ollama hoặc API tương thích OpenAI | Trợ lý hỗ trợ Ollama chạy tại máy, endpoint mô hình đám mây có thể cấu hình, và bộ suy luận Huế tích hợp làm phương án dự phòng. Khóa API lấy từ cấu hình bảo mật hoặc biến môi trường, không lưu trong Git. |
| Đăng nhập và an toàn cơ bản | ASP.NET Core Cookie Authentication, role-based authorization, PBKDF2 | Cookie xác thực và quyền `Admin`/`Customer` bảo vệ các trang; mật khẩu được băm PBKDF2-SHA256 với salt riêng. |
| Báo cáo | Endpoint ASP.NET Core, XLSX và CSV | Quản trị viên có thể xuất dữ liệu booking/chuyến qua endpoint báo cáo. |

Bootstrap, Bootstrap Icons, Chart.js, Leaflet và Google Fonts hiện được tải từ CDN; website cần kết nối tới các CDN này để tải đủ thư viện giao diện. SQL Server chỉ bắt buộc khi muốn lưu dữ liệu qua plugin SQL; chế độ In-memory dùng cho phát triển và xem giao diện.

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
2. Mở `database/HueTour_SchemaAndData.sql` trong SSMS và chạy trên đúng SQL Server của đồ án. Script chỉ tạo `HueTourDb` khi database chưa tồn tại và sẽ dừng nếu database đã có để tránh xóa dữ liệu. Chín bảng vật lý và cột dùng tên tiếng Việt; các view tên tiếng Anh giữ tương thích với ứng dụng. Khi chạy tệp UTF-8 bằng `sqlcmd`, truyền `-f 65001` để giữ nguyên tiếng Việt.
3. Chạy `database/seed/AddHueMarketTours_2026.sql`, sau đó `database/migrations/AlignMarketTourIds_1002_1012.sql`. Hai script bảo đảm đủ 11 tour thị trường với ID 1002–1012, nhà cung cấp/nguồn giá/ngày kiểm tra; migration dừng nếu ID đích bị tour khác chiếm hoặc tour thị trường đã có chuyến. 11 tour này chỉ tham khảo, không có chuyến giả.
4. Chạy `database/seed/AdminDashboard_OneYearDemoData.sql` để tạo dữ liệu minh họa dashboard theo 12 tháng gần nhất: booking, khách mẫu, vé hành khách và chuyến tương ứng. Script chạy lặp không tạo trùng; chỉ xóa booking demo đã cũ và chuyến demo không còn booking.
5. Với database cũ được tạo trước chuẩn hóa 3NF, chạy `database/migrations/NormalizeFoodCategories_3NF.sql` một lần. Trên schema mới, bảng danh mục đã có sẵn. Sau đó chạy `database/seed/DiaDiemAmThuc_Seed.sql` để nạp 600 địa điểm; tệp seed thay thế dữ liệu địa điểm hiện tại.
6. Tạo sơ đồ trong Object Explorer → `HueTourDb` → `Database Diagrams` → `New Database Diagram`, rồi chọn chín bảng tiếng Việt để xem đầy đủ các bảng và khóa ngoại.
7. Cấu hình chuỗi kết nối `ConnectionStrings:HueTourDb` trong `HueTour.WebApp/appsettings.json` theo SQL Server trên máy bạn (máy hiện tại báo server name `Minnh`, default instance; ví dụ `Server=localhost;Database=HueTourDb;Trusted_Connection=True;TrustServerCertificate=True`).
8. Để bật SQL trong môi trường Development mà không sửa cấu hình dùng chung, tạo `HueTour.WebApp/appsettings.Local.json`; tệp này được bỏ qua bởi Git và tự nạp sau cấu hình môi trường. Đặt `UseSqlDatabase` thành `true` và thêm chuỗi kết nối máy cục bộ của bạn vào tệp này. Nếu không có tệp local, Development tiếp tục chạy bằng dữ liệu InMemory.
9. Mặc định trong repository dùng giá trị ngân hàng placeholder, `VietQrPayment:IsDemo=true` và `QrImagePath=/images/vietqr-demo.png`. QR demo có thể sinh lại bằng `python -m pip install segno` rồi `python tools/make-demo-qr.py`; payload chỉ là nội dung minh họa, không dùng để chuyển tiền.
10. Mọi thông tin ngân hàng được Git theo dõi chỉ là placeholder/demo. Khi cần dùng tài khoản thật trên máy local, cấu hình trong `appsettings.Local.json` (đã được Git bỏ qua), đặt `IsDemo=false` và trỏ `QrImagePath` tới một ảnh QR cũng đã được Git bỏ qua. Sinh ảnh VietQR chuẩn EMVCo bằng `python tools/make-vietqr.py --bank-bin <BIN> --account <STK> --holder "<TEN CHU TK>" --output HueTour.WebApp/wwwroot/images/vietqr-local.png`; script không lưu thông tin tài khoản trong mã nguồn. Không commit số tài khoản, tên chủ tài khoản hoặc ảnh QR thật. Có thể dùng biến môi trường `HUETOUR_BANK_*` thay cho tệp local nếu môi trường triển khai hỗ trợ.

Tài khoản được seed trong SQL là `admin` / `Admin@123` và `khachhang` / `User@123`. Mật khẩu seed đã được băm PBKDF2.

## Cấu trúc

- `HueTour.CoreBusiness`: mô hình miền, luật đặt tour và băm mật khẩu.
- `HueTour.UseCases`: ca sử dụng và các interface repository.
- `HueTour.Plugins.DataStore.InMemory`: dữ liệu và repository mẫu.
- `HueTour.Plugins.DataStore.SQL`: truy cập SQL Server bằng Dapper.
- `HueTour.WebApp`: giao diện Blazor Server, xác thực cookie và phân quyền.
- `HueTour.UseCases/MapExplorer`: use case hợp nhất dữ liệu ẩm thực curated với điểm OSM cho bản đồ dùng chung.
- `HueTour.WebApp/Components/Shared/UnifiedMapExplorerComponent.razor`, `wwwroot/js/unified-map.js`, `wwwroot/css/unified-map.css`: component bản đồ, Leaflet interop và giao diện responsive cho `/kham-pha`. `/am-thuc` không render bản đồ riêng mà liên kết sang bản đồ khám phá với bộ lọc ẩm thực.
- `HueTour.WebApp/wwwroot/css/booking.css`: giao diện thẻ hỗ trợ lưu trú, gợi ý khu vực, bộ chọn số phòng và thanh đặt tour cố định trên mobile.
- `database/HueTour_SchemaAndData.sql`: schema và dữ liệu minh họa.
- `docs/diagrams/relational-schema.puml`: sơ đồ quan hệ CSDL SQL Server và các khóa chính/ngoại.
- Trong SSMS, mở **Database Diagrams > New Database Diagram** và thêm chín bảng tiếng Việt; SQL Server sẽ tự hiện các quan hệ từ khóa ngoại trong script.
- `docs/diagrams`: các sơ đồ PlantUML theo yêu cầu đồ án.
- `GRADING.md`: bảng đối chiếu tiêu chí với vị trí triển khai và phần còn cần xác minh.
- `HueTour.WebApp/wwwroot/images`: ảnh Huế đã lưu cùng dự án; ghi công tại `ASSETS.md`.

Giỏ đặt chỗ dùng state theo phạm vi phiên Blazor. Hệ thống áp dụng quy tắc không vượt sức chứa, khóa đặt chỗ theo hạn định của luật nghiệp vụ và tính giá trẻ em theo độ tuổi.

Form đặt tour có mục **Yêu cầu hỗ trợ lưu trú** tùy chọn: khách chọn khu vực/khách sạn mong muốn, ngày nhận/trả phòng, số phòng và ghi chú. Giao diện dùng thẻ bật/tắt, gợi ý nhanh khu vực, nút tăng/giảm số phòng, tự tính số đêm và hiển thị bản tóm tắt ngay trong cột đặt tour. Đây chỉ là yêu cầu để nhân viên liên hệ tư vấn, chưa phải đặt phòng và không cộng vào tổng tiền tour; nội dung được lưu vào cột `YeuCauDacBiet` của đơn đặt tour và hiển thị lại ở trang xác nhận, tra cứu vé và danh sách đơn của quản trị viên. Trên mobile có thanh tổng tiền và nút đặt tour cố định ở cuối màn hình; nút tóm tắt và thanh mobile đều gọi `EditContext.Validate()` trước khi chạy nghiệp vụ.

## Kiểm chứng đặt tour trên SQL Server

1. Đóng ứng dụng đang chạy để giải phóng các DLL, sau đó chạy `dotnet build HueTourApp.sln -m:1` và xác nhận build hoàn tất không có lỗi.
2. Chạy `database/HueTour_SchemaAndData.sql` trong SSMS, nạp `database/seed/DiaDiemAmThuc_Seed.sql`, đặt `UseSqlDatabase=true` trong `HueTour.WebApp/appsettings.Local.json`, rồi chạy ứng dụng.
3. Chọn chuyến gần đầy hoặc khởi hành trong thời hạn khóa đặt, thử đặt tour và xác nhận thông báo thân thiện xuất hiện, không có booking/vé mới và số ghế không đổi.
4. Đặt chuyến hợp lệ và xác nhận booking cùng `PassengerTicket` được tạo, đồng thời `Departure.BookedSeats` tăng đúng số hành khách.
5. Chọn **Yêu cầu hỗ trợ lưu trú**, nhập khu vực, ngày nhận/trả phòng, số phòng và ghi chú, sau đó đặt tour. Xác nhận nội dung hiển thị ở trang xác nhận, tra cứu vé và danh sách đơn quản trị; kiểm tra cột `YeuCauDacBiet` trong `dbo.DonDatTour` bằng `sqlcmd`.

SQL Server thật cần thiết để xác minh transaction và truy vấn cập nhật số ghế.

### Kết quả kiểm chứng gần nhất

Trong phiên rà soát ngày 23/10/2026, `dotnet build HueTourApp.sln -m:1` thành công với 0 cảnh báo và 0 lỗi. Trong lần kiểm thử trước ngày 23/10, SQL Server `localhost\Minnh` đã hoạt động và đã chạy migration `NormalizeFoodCategories_3NF.sql`, xác nhận `DanhMucAmThuc` có 10 dòng, `DiaDiemAmThuc` còn 600 địa điểm và khóa ngoại danh mục đang bật. Admin đã sửa ngày kiểm tra giá của tour thị trường `HT-MARKET-HITOUR1D34` thành 23/10/2026 (nguồn Hitour công bố 850.000đ/người lớn); SQL xác nhận giá trị mới và trang `/tour/1012` hiển thị nhà cung cấp, ngày kiểm tra cùng liên kết nguồn (ảnh `17-admin-edit-tour-sql.png`, `22-market-tour-provider-source.png`). Đã đăng nhập Admin và tải trực tiếp `/api/reports/bookings.xlsx`: file Office Open XML hợp lệ, 8 dòng gồm 1 tiêu đề và 7 đơn từ SQL. Ảnh Luật 2 `21-rule-age-mismatch.png` hiển thị mã `RULE2_AGE_TYPE_MISMATCH`; Ảnh booking riêng ở 390×844 được bỏ theo yêu cầu; website đã có ảnh responsive 390×844 tại docs/evidence/15-mobile-390.png.

Ngày 23/10/2026, cấu hình QR ngân hàng thật được kiểm tra cục bộ qua `appsettings.Local.json` (Git bỏ qua): UI hiển thị MB Bank, đúng chủ tài khoản, ảnh QR tải thành công và được giải mã lại đúng BIN/số tài khoản/đồng tiền/quốc gia. Ảnh QR thật và tệp cấu hình không được commit.

Ngày 23/10/2026, app chạy với plugin SQL Server (`UseSqlDatabase=true`, server `Minnh`, database `HueTourDb`). Truy vấn `sqlcmd -C` xác nhận 34 tour (33 tour mở bán), 28 chuyến, 600 địa điểm ẩm thực, 7 đơn đặt tour (2 đơn `Cancelled`); 11 tour thị trường mã 1002–1012 chưa có chuyến nên chỉ để tham khảo. Booking thử `HUE-20261022-5BC0` đã được hủy, vé vẫn được lưu và số ghế trở về 0. Trang `/booking/14` hiển thị QR demo 294×294 từ `/images/vietqr-demo.png`, cảnh báo “Mã QR minh họa phục vụ demo, không dùng để chuyển tiền.” và số placeholder `0123456789`; ảnh `docs/evidence/20-booking-qr-demo.png`. Đăng nhập `khachhang` rồi mở `/admin` bị chuyển về `/login?ReturnUrl=%2Fadmin` (ảnh `14-customer-admin-blocked.png`). Bản đồ hợp nhất chỉ còn được render tại `/kham-pha` và đã kiểm tra ở desktop (ảnh `19-unified-map-kham-pha.png`) cùng đúng 390×844 (ảnh `15-mobile-390.png`): 600 marker curated từ SQL; ảnh `15`/`19` ghi nhận cảnh báo nguồn bản đồ khi Overpass công cộng lỗi, trang không treo. Ảnh `18-food-list-no-map.png` xác nhận `/am-thuc` chỉ còn danh sách và nút mở bản đồ ẩm thực. Các mirror Overpass công cộng không ổn định trong lúc chụp, nên chưa có ảnh chứng minh nhánh OSM live thành công; luồng GPS mới được thử bằng tọa độ mô phỏng, chưa phải thiết bị thật.

## Khám phá Huế: bản đồ, thời tiết và cẩm nang

Trang `/kham-pha` bổ sung tìm địa điểm quanh thành phố hoặc một điểm tham quan đang có trên bản đồ, dự báo thời tiết 7 ngày, cẩm nang Huế, lịch trình theo sở thích và gợi ý từ trợ lý. Trang `/am-thuc` chỉ còn danh sách quán và bộ lọc ẩm thực; nút **Mở bản đồ ẩm thực** đưa sang `/kham-pha?category=food#map`, nơi bản đồ duy nhất của hệ thống tự tải dữ liệu Huế. Hai route cũ được giữ để không phá liên kết.

Bản đồ hợp nhất nằm trong `HueTour.WebApp/Components/Shared/UnifiedMapExplorerComponent.razor`, dùng `HueTour.UseCases/MapExplorer/MapPoiCatalogUseCase.cs` để hợp nhất dữ liệu ẩm thực curated từ SQL với điểm tham quan/quán lấy từ OpenStreetMap, sau đó dedupe theo tên chuẩn hóa. Dữ liệu curated (600 địa điểm) được hiển thị trước; enrichment OSM chỉ chạy sau khi JS interop sẵn sàng, không gọi trong prerender/SSR. Bộ lọc gồm từ khóa, loại địa điểm, bán kính và khung nhìn. `/kham-pha` ẩn bộ chọn bán kính và giữ bản đồ quanh điểm đến 10 km. `/am-thuc` không còn bản đồ riêng hay bộ lọc không gian; trang giữ danh sách, tìm kiếm, danh mục và phân trang, sau đó liên kết sang bản đồ duy nhất tại `/kham-pha?category=food#map`.

### Nguồn dữ liệu và cấu hình

| Nguồn | Dữ liệu | API key |
|---|---|---|
| [Nominatim](https://nominatim.openstreetmap.org/) | Chuyển tên thành phố/khu vực người dùng nhập thành tọa độ | Không |
| [Overpass API](https://dev.overpass-api.de/overpass-doc/en/) + [OpenStreetMap](https://www.openstreetmap.org/copyright) | Nhà hàng, quán cà phê, khách sạn, điểm tham quan, tên/địa chỉ, tọa độ và thẻ giờ mở cửa/ảnh/giá nếu cộng đồng đã ghi | Không |
| [Open-Meteo](https://open-meteo.com/en/docs) | Thời tiết hiện tại và dự báo 7 ngày, gồm nhiệt độ, cảm giác như, độ ẩm, gió và xác suất mưa | Không cần cho sử dụng phi thương mại thông thường |
| [Leaflet](https://leafletjs.com/) + bản đồ nền OpenStreetMap | Bản đồ, điểm đánh dấu và cửa sổ thông tin | Không |

Các địa chỉ dịch vụ có thể thay trong `TravelApis` ở `HueTour.WebApp/appsettings.json` (hoặc nguồn cấu hình .NET tương ứng): `NominatimEndpoint`, `OverpassEndpoints`, `WeatherEndpoint`. Danh sách Overpass mặc định gồm `overpass-api.de`, `overpass.kumi.systems` và mirror `overpass.openstreetmap.fr`; nếu một máy không phản hồi, ứng dụng thử máy kế tiếp. Không cần tạo hay lưu API key cho các cấu hình mặc định. Dữ liệu thời tiết được giữ trong bộ nhớ 15 phút; kết quả địa điểm 15 phút; địa chỉ tìm kiếm 24 giờ. Trang `/kham-pha` tải thời tiết và địa điểm độc lập, hiển thị khung chờ cho từng phần và giới hạn yêu cầu thời tiết trong 6 giây, nên một mirror Overpass chậm không kéo theo dự báo. `TravelDiscoveryMapDataSource` tái sử dụng kết quả OSM trong bán kính 3 km/10 phút khi người dùng pan nhỏ, và cache theo ô địa lý. Khi tất cả mirror lỗi hoặc quá chậm, bản đồ vẫn giữ dữ liệu curated từ SQL và hiện cảnh báo thay vì treo trang.

Nominatim chỉ được gọi sau thao tác tìm kiếm của người dùng, có bộ đệm và giới hạn tối thiểu một giây giữa các truy vấn. Dịch vụ công cộng có giới hạn tải; khi vận hành lượng truy cập lớn, hãy cấu hình dịch vụ bản đồ/thời tiết do mình quản lý hoặc nhà cung cấp có SLA. Không dùng dịch vụ Nominatim công cộng cho tự động gợi ý khi gõ hoặc truy vấn hàng loạt.

Ứng dụng không bịa giá, đánh giá, giờ mở cửa hay ảnh địa điểm. Khi OpenStreetMap không có trường dữ liệu tương ứng, trang ghi “chưa có dữ liệu”; ảnh dự phòng chỉ là biểu tượng danh mục. Dữ liệu ẩm thực curated lấy từ bảng `DiaDiemAmThuc` (600 địa điểm) được hiển thị trước, sau đó marker OSM mới được bổ sung; nếu OSM lỗi, trang giữ marker curated và ghi rõ nguồn bản đồ đang bận. Các món ăn trong cẩm nang là nội dung biên tập chung, được phân biệt với quán thật lấy từ API. Nếu tìm địa điểm hoặc thời tiết lỗi, trang hiển thị thông báo và vẫn giữ cẩm nang, tour trong cơ sở dữ liệu cùng gợi ý dự phòng.

Trình duyệt chỉ hỏi vị trí khi người dùng bấm **Vị trí hiện tại** hoặc **Tìm quanh tôi** trên bản đồ. Tọa độ dùng để truy vấn địa điểm/thời tiết và không được lưu trong ứng dụng hay gửi cho trợ lý AI. Luồng này đã được thử bằng tọa độ mô phỏng trong trình duyệt (16.4637, 107.5909) và cần kiểm tra lại trên thiết bị thật. Nếu đã cấu hình AI từ xa, câu hỏi cùng tên địa điểm, thời tiết và tour hiện trên trang sẽ được gửi tới nhà cung cấp AI đã cấu hình; trang thông báo điều này cạnh câu trả lời.

Giữ dòng ghi công OpenStreetMap trên bản đồ và dòng nguồn Open-Meteo ở phần dự báo. Dịch vụ API và bản đồ công cộng là best-effort, không có cam kết luôn sẵn sàng; giá và độ đầy đủ của thẻ OSM tùy đóng góp cộng đồng.

HueTour hiện là ứng dụng Blazor Server/.NET 8, có cookie đăng nhập và backend dữ liệu. Kiến trúc này cần máy chủ .NET, nên bản hiện tại không thể được triển khai như website tĩnh trên GitHub Pages. Yêu cầu GitHub Pages chỉ áp dụng khi website đang dùng kiến trúc tĩnh; để chuyển dự án này cần một lần triển khai kiến trúc riêng.
