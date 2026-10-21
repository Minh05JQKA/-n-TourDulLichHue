# Báo cáo các chức năng còn thiếu – HueTour Booking

## Tóm tắt

Đã rà README, GRADING, solution, toàn bộ 17 trang Razor, trạng thái/lịch sử Git, luồng booking, SQL repository, xác thực, AI, báo cáo và sơ đồ. Đã khắc phục quyền truy cập CSV và trạng thái lỗi/loading. Các điểm còn cần chuẩn bị trước demo là xác minh SQL Server thật, lệch số project K1.1 và khoảng thời gian commit K5.3.

Đã kiểm tra giao diện tại viewport rộng 390px: menu chuyển sang nút thu gọn; trang đặt tour hiển thị một cột và các trường nhập không tràn. Chưa xác nhận trực quan trang bảng Admin ở kích thước này. Đã truy cập `/admin` khi chưa đăng nhập và sau đăng nhập Customer; cả hai trường hợp đều được đưa về `/login?ReturnUrl=%2Fadmin`.

## Bảng chi tiết

| STT | Rubric | Chức năng | Mức độ (Cao/Trung bình/Thấp) | Trạng thái | Vị trí file | Đề xuất sửa |
|---:|---|---|---|---|---|---|
| 1 | K4.2 / K4.3 | Endpoint CSV bookings và danh sách hành khách cần giới hạn Admin vì dữ liệu xuất có thông tin khách/hành khách | Cao | Đã yêu cầu role Admin ở cả hai endpoint; chưa kiểm tra request HTTP do giới hạn socket cục bộ | `HueTour.WebApp/Program.cs` | Đã thêm authorization role Admin, khớp với quyền ở các trang quản trị. |
| 2 | K3.3 | Lỗi ngoài dự kiến lúc đặt tour trước đó có thể hiển thị nguyên văn `Exception.Message` | Trung bình | Đã thay bằng thông báo chung và ghi lỗi qua ILogger | `HueTour.WebApp/Components/Pages/Booking.razor` | Đã sửa; giao diện không đưa nội dung exception nội bộ ra khách hàng. |
| 3 | K3.2 / trải nghiệm demo | Tra cứu booking trước đó không hiển thị loading và không có xử lý lỗi thân thiện | Trung bình | Đã thêm trạng thái đang tra cứu, kiểm tra mã rỗng và lỗi chung | `HueTour.WebApp/Components/Pages/MyBooking.razor` | Đã sửa; nút bị khóa trong lúc truy vấn, có thông báo khi mã rỗng hoặc repository lỗi. |
| 4 | K1.1 | Rubric ghi 4 project; solution hiện có 5: CoreBusiness, UseCases, SQL, InMemory và WebApp. WebApp tham chiếu trực tiếp hai plugin | Cao | Chưa khớp số project rubric | `HueTourApp.sln`, các file `.csproj` | Xác nhận rubric có bắt buộc đúng 4 project. Nếu bắt buộc, hợp nhất plugin DataStore thành một project; đây là thay đổi kiến trúc riêng, cần cân nhắc kỹ vì working tree đang có sửa đổi. |
| 5 | K2.4 | Chưa có bằng chứng chạy schema và giao dịch end-to-end trên SQL Server thật trong lượt rà soát | Cao | Chưa xác minh môi trường SQL Server | `database/HueTour_SchemaAndData.sql`, `README.md`, `HueTour.Plugins.DataStore.SQL/TourBookingRepository.cs` | Chạy trên database riêng, kiểm tra đặt hợp lệ, hết chỗ và dưới hạn 24 giờ; xác nhận header-line cùng cập nhật ghế trong một transaction. |
| 6 | K3.4 | Home và booking đã xem ở 390px và hiển thị gọn; phần bảng Admin mới xác minh bằng `table-responsive`, chưa xem trực quan | Thấp | Đạt một phần; Admin chưa xác minh hình ảnh | `HueTour.WebApp/Components/Pages/Admin/*.razor`, `HueTour.WebApp/wwwroot/app.css` | Khi chuẩn bị demo, mở các bảng Admin ở 390px và chụp bằng chứng; hiện chưa thấy lý do sửa CSS. |
| 7 | K5.3 | Lịch sử Git hiện thấy commit từ 07/10/2026 đến 20/10/2026, khoảng 13 ngày, ngắn hơn yêu cầu tối thiểu 3 tuần | Cao | Chưa đủ khoảng thời gian trong lịch sử hiện có | `git log` | Không tạo commit giả hoặc đổi ngày. Kiểm tra rubric/trao đổi giảng viên về minh chứng lịch sử hợp lệ; code không thể bù cho thời gian commit đã qua. |

Các mục đã có bằng chứng mã nguồn và không cần sửa trong lượt này: K1.2–K1.4; K2.1–K2.3; K3.1–K3.2 form chính; K4.1 và chặn URL Admin; K4.3 có `IChatClient` trong UseCase, cấu hình key từ cấu hình/User Secrets hoặc biến môi trường, fallback, cùng CSV từ repository. Luồng TravelDiscovery truyền địa điểm/thời tiết/tour cho trợ lý nhưng không truyền tọa độ GPS. Sơ đồ sequence booking thể hiện transaction. Chưa thêm trang chi tiết Ẩm thực vì không phải yêu cầu bắt buộc của rubric.

## Ưu tiên thực hiện

1. Chuẩn bị và chạy demo SQL Server end-to-end trên database riêng.
2. Xác nhận yêu cầu K1.1 về đúng 4 project; nếu bắt buộc, lập kế hoạch gộp plugin DataStore trước khi chỉnh kiến trúc.
3. Làm rõ cách chứng minh K5.3: lịch sử hiện có chưa đủ 3 tuần; không tạo lịch sử giả.
4. Chụp bằng chứng các trang Admin ở 390px và demo hai luật nghiệp vụ trên UI.
5. Sau khi chạy ứng dụng, xác nhận Admin tải được CSV và người chưa đăng nhập bị chuyển tới trang login.

## Ghi chú an toàn

Tại thời điểm rà soát, working tree đã có nhiều file `M` và `??`, gồm các trang Khám phá/Ẩm thực, `Register.razor`, service, dữ liệu ảnh, SQL, README, GRADING và sơ đồ. Các nội dung này là thay đổi có sẵn; không xóa, reset, checkout hoặc commit chúng. Báo cáo này là file mới của lượt rà soát. Build theo lệnh chuẩn bị lỗi sao chép do các DLL đầu ra bị tiến trình khác giữ khóa; build cô lập với `BaseOutputPath` hoàn tất 0 warning, 0 error. Kiểm tra HTTP localhost không chạy được do môi trường chặn quyền mở socket.
