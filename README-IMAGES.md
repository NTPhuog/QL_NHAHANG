# Ảnh món ăn và bàn

- Vào Admin → Quản lý món ăn / Quản lý bàn → Thêm hoặc Sửa → Chọn ảnh → Lưu.
- Nhận JPG, PNG, WebP tối đa 5 MB; tên ảnh được tạo ngẫu nhiên. Kiểm tra chữ ký định dạng phía máy chủ, không tin tên tệp người tải lên.
- Không chọn ảnh khi sửa sẽ giữ nguyên ảnh hiện tại. Ảnh là tùy chọn khi thêm mới.
- Ảnh được lưu tại SharedUploads/mon-an và SharedUploads/ban-an, bên cạnh ba thư mục ứng dụng. Cả ba ứng dụng phục vụ cùng thư mục ở /uploads; trang khách không phụ thuộc cổng admin để tải ảnh.
- Có thể cấu hình Images:Root thành đường dẫn tuyệt đối, giống nhau ở cả ba ứng dụng, nếu triển khai ở nơi khác.
- Database/20261010_AddTableImage.sql bổ sung cột BanAn.HinhAnh, chạy lại an toàn. Đã áp dụng trên database hiện tại. Không xóa hoặc đổi dữ liệu bàn cũ.
- Sao lưu SharedUploads cùng với database. Ảnh cũ khi thay thế hoặc xóa món được giữ lại để tránh xóa nhầm tệp đang được tham chiếu. Tệp vừa tải lên được dọn nếu bước lưu dữ liệu thất bại.
- Trang thực đơn khách có ảnh dự phòng khi chưa có ảnh, bộ lọc danh mục, tìm kiếm, sắp xếp giá và phân trang 12 món; bố cục 3/2/1 cột theo kích thước màn hình.

Đã kiểm thử qua HTTP: đăng nhập admin, thêm món có ảnh/không ảnh, sửa giữ ảnh cũ, tải ảnh từ ứng dụng khách và so sánh byte, từ chối tệp giả ảnh/ảnh quá 5 MB, thêm bàn có ảnh và hiển thị bên khách. Dữ liệu và ảnh thử đã được dọn. Đã xem trực tiếp giao diện thực đơn trên màn hình hẹp.
