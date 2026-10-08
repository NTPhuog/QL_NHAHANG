# RestaurantManagement — bản bổ sung chức năng chính

## Vị trí và trạng thái
Bản này được tạo từ dự án gốc trên ổ E:. Chưa ghi đè dự án gốc vì chưa được cấp quyền ghi vào thư mục đó.

Đã build thành công với .NET SDK 10.0.401: 0 lỗi, 0 cảnh báo. 25 kiểm tra tự động về mật khẩu và ràng buộc dữ liệu đã đạt. Ứng dụng khởi động được; chưa xác minh HTTP end-to-end và các giao dịch trên SQL Server trong môi trường này. Không có dữ liệu SQL hiện hữu nào bị sửa trong quá trình thực hiện.

## Mở và chạy trong VS Code
1. Mở thư mục RestaurantManagement trong VS Code hoặc mở RestaurantManagement.code-workspace.
2. Kiểm tra ConnectionStrings:QuanLyNhaHang trong appsettings.json, trỏ tới SQL Server và database hiện tại của bạn. Bản mã dùng nguyên các bảng hiện có, không tự xóa/tạo lại database.
3. Chạy `dotnet restore`, `dotnet build`, rồi `dotnet run` tại thư mục chứa RestaurantManagement.csproj. Mở địa chỉ ứng dụng in trong terminal.
4. Tại trang đăng nhập, chọn đúng vai trò Khách hàng / Nhân viên / Admin.
5. Nếu đã có tài khoản trong SQL, dùng tài khoản đó. Mật khẩu cũ dạng văn bản được nâng cấp thành hash sau lần đăng nhập đúng đầu tiên. Tài khoản cần trạng thái `Hoạt động`.

Nếu bảng Admin chưa có tài khoản, chạy `./Create-Admin.ps1` tại thư mục dự án. Script hỏi email và mật khẩu riêng, không có tài khoản mặc định. Chế độ khởi tạo từ chối khi bảng Admin đã có dữ liệu.

## Chức năng
- Khách: đăng ký, đăng nhập/đăng xuất, xem thực đơn và khuyến mãi, chọn bàn, đặt bàn, xem lịch sử riêng, hủy đơn đang chờ.
- Nhân viên: lọc theo ngày, trạng thái, tên hoặc điện thoại; duyệt, hủy kèm lý do, nhận bàn, hoàn thành.
- Admin: dashboard số bàn/khách/nhân viên/đơn chờ và 10 đơn gần nhất; thêm/sửa/xóa bàn, món và danh mục; quản lý khuyến mãi; thêm/sửa/khóa nhân viên; tìm/khóa/mở khóa khách; điều phối đặt bàn.
- Phân quyền kiểm tra phía máy chủ, kể cả khi nhập URL trực tiếp. Tài khoản bị khóa mất quyền khi gửi yêu cầu tiếp theo.
- Đăng xuất dùng POST; thao tác thay đổi có chống CSRF. Không hiển thị mật khẩu trên danh sách tài khoản.

## Quy tắc đặt bàn đang áp dụng
- Theo múi giờ Việt Nam; thời gian phải ở tương lai.
- Nhận đặt từ 10:00 đến 20:00; mỗi lượt 2 giờ. Đây là quy ước triển khai cần đối chiếu với yêu cầu của nhóm.
- Chỉ nhận bàn trạng thái `Trống`, số khách từ 1 đến sức chứa.
- Các lượt không hủy phải cách nhau ít nhất 120 phút trên cùng bàn/ngày. Giao dịch SQL dùng khóa bàn để tuần tự hóa yêu cầu đồng thời.
- Chờ xác nhận → Đã duyệt → Đang phục vụ → Hoàn thành. Nhân viên được hủy khi Chờ xác nhận hoặc Đã duyệt; khách chỉ được hủy đơn của mình lúc Chờ xác nhận.
- Không xóa bàn đã có lịch sử đặt; chuyển sang Ngừng phục vụ. Không xóa danh mục còn món.

## Kiểm tra trước khi demo với database thật
Nếu SQL có CHECK constraint giới hạn trạng thái DatBan, cần bổ sung `Đang phục vụ`, `Hoàn thành` vào constraint hiện có. Tương tự, kiểm tra các trạng thái bàn (`Trống`, `Bảo trì`, `Ngừng phục vụ`), món (`Hiển thị`, `Ẩn`, `Hết món`), tài khoản (`Hoạt động`, `Khóa`), khuyến mãi (`Hoạt động`, `Ngừng`). Không tự xóa constraint đang có. Các cột MatKhau phải đủ 255 ký tự như model hiện tại.

Chạy `dotnet run --project ../Verification/Verification.csproj` để lặp lại 25 kiểm tra độc lập. Các kiểm tra này không thay thế kiểm thử SQL hay kiểm thử trình duyệt.

| Mã | Thao tác | Kết quả cần đạt | Trạng thái |
|---|---|---|---|
| TC01 | Đăng ký email mới, mật khẩu >=8 ký tự | Tạo khách, đăng nhập, SQL lưu hash | Chưa chạy SQL |
| TC02 | Email trùng / điện thoại sai / mật khẩu không khớp | Thông báo, không tạo tài khoản | Chưa chạy SQL |
| TC03 | Đặt bàn tương lai, số khách hợp lệ | Lưu Chờ xác nhận, xuất hiện lịch sử | Chưa chạy SQL |
| TC04 | Quá khứ / ngoài giờ / vượt sức chứa | Từ chối và giải thích | Chưa chạy end-to-end |
| TC05 | Hai trình duyệt đặt cùng bàn, giờ trùng đồng thời | Chỉ một yêu cầu được lưu | Chưa chạy SQL |
| TC06 | Hai lượt cách đúng 120 phút | Cho phép | Chưa chạy SQL |
| TC07 | Khách A xem/hủy ID của khách B | Không đọc/hủy được | Chưa chạy SQL |
| TC08 | Nhân viên duyệt, nhận bàn, hoàn thành | Đúng chuỗi trạng thái | Chưa chạy SQL |
| TC09 | Hai nhân viên duyệt/hủy cùng đơn | Không ghi đè trạng thái đã chuyển | Chưa chạy SQL |
| TC10 | Hủy thiếu lý do hoặc đã hoàn thành | Từ chối | Chưa chạy SQL |
| TC11 | Khách nhập URL admin/nhân viên; khách chưa đăng nhập | 403 hoặc chuyển đăng nhập theo trạng thái đăng nhập | Chưa chạy HTTP |
| TC12 | Admin tạo/sửa món, bàn, nhân viên; khóa tài khoản đang dùng | Lưu đúng; người bị khóa hết quyền ở yêu cầu tiếp theo | Chưa chạy SQL |
| TC13 | Xóa bàn có lịch sử / danh mục còn món | Thông báo, giữ nguyên dữ liệu | Chưa chạy SQL |
| TC14 | POST không có antiforgery token | HTTP 400 | Chưa chạy HTTP |

Phạm vi bản này tập trung đặt bàn và quản lý danh mục/tài khoản. Chưa có luồng gọi món theo bàn, hóa đơn, thanh toán hoặc doanh thu vì dự án hiện tại chưa có các bảng nghiệp vụ đó. Dashboard thống kê số lượng, không giả lập doanh thu.

Bản sửa admin/nhân viên mới nhất: xem ADMIN-NHANVIEN-FIXES.md.
