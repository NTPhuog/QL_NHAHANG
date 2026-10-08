# Bản sửa admin / nhân viên — 08/10/2026

Bản sửa nằm trong thư mục bàn giao, chưa được áp dụng vào ổ E: vì công cụ chưa được cấp quyền ghi. Lệnh mở VS Code đã được gửi cho thư mục QL_NHAHANG.

Các lỗi đã sửa:
- Thêm món và thêm khuyến mãi: bổ sung trường trạng thái bắt buộc; form thêm/sửa tiếng Việt, có danh sách trạng thái và thông báo toàn bộ lỗi nhập liệu.
- Phân quyền: dùng tên controller/action đã được MVC xác định; xử lý đường dẫn không phân biệt chữ hoa/thường. Controller mới mặc định chỉ admin được truy cập.
- Nhân viên: không duyệt đơn đã quá giờ; chỉ nhận đơn đã duyệt trong lượt 2 giờ; không nhận lượt khác khi cùng bàn còn đơn đang phục vụ. Nhận bàn và kiểm tra bàn được bảo vệ bằng giao dịch và khóa SQL.
- Admin sửa bàn: không giảm sức chứa xuống dưới số khách của đơn còn hiệu lực; không chuyển bàn sang bảo trì/ngừng phục vụ khi còn đơn cần xử lý. Sửa/xóa dùng cùng cơ chế khóa với đặt bàn để tránh xung đột đồng thời.
- Khuyến mãi hết hạn/không hoạt động không còn xem được bằng đường dẫn chi tiết của khách.
- Bỏ liên kết sửa đơn khách hàng dẫn đến chức năng đã bị loại bỏ.

Quy tắc nhận bàn dùng đúng khung giờ đã hẹn đến trước 2 giờ sau đó. Nếu nhà hàng muốn cho nhận sớm, cần thống nhất quy tắc khác trước khi đổi.

Cách áp dụng: chạy Chen-vao-du-an.ps1 trong thư mục outputs. Script kiểm tra thay đổi, sao lưu tệp cũ trong .codex-backups, chép tệp mới và kiểm tra hash. Không ghi đè appsettings*.json. Không thay đổi dữ liệu SQL.

Chạy kiểm thử bằng: dotnet run --project ../Verification/Verification.csproj (từ thư mục RestaurantManagement).
Kiểm thử độc lập bao gồm hash mật khẩu, ràng buộc dữ liệu, phân quyền theo cách viết URL, giới hạn thời gian nhận bàn và điều kiện bảo vệ bàn có đơn đang phục vụ. Việc kiểm thử này không xác nhận các giao dịch SQL hoặc giao diện trình duyệt; cần chạy các trường hợp trong README.md với database thực tế.
