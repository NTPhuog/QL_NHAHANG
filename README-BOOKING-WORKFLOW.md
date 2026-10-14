# Giữ bàn, check-in và hóa đơn

## Khách hàng – cổng 5097
1. Chọn bàn và giờ, đăng nhập. Khi mở form, hệ thống yêu cầu giữ bàn 5 phút theo ngày/giờ đã chọn; mỗi khách chỉ có một phiên giữ đang hiệu lực. Tải lại không gia hạn phiên còn sống.
2. Bàn 2 chỗ: không đặt trước món. Bàn lớn hơn: được chọn món; nếu có món thì cọc giả lập 50% tổng sau ưu đãi, làm tròn đến đồng. Không chọn món thì chỉ đặt bàn, không cọc.
3. Xem lại món, giá và tiền cọc. Bấm thanh toán thành công/thất bại để thử. Không thu tiền thật. Thất bại hoặc hết hạn không tạo đơn và không ghi nhận cọc.
4. Đặt thành công tự xác nhận (trạng thái nội bộ “Đã duyệt”), hiện mã NH-… và QR trong lịch sử. Không cần lễ tân duyệt trước. Hủy đơn trước check-in tự hoàn cọc giả lập.

## Lễ tân – cổng 5098
1. Mở **Quét QR / Check-in**. Nhập mã, dùng máy quét bàn phím, camera hoặc chọn ảnh QR. Camera cần quyền truy cập; localhost được hỗ trợ, truy cập từ máy khác cần HTTPS để mở camera.
2. Check-in từ giờ hẹn đến trước khi hết lượt 2 giờ. Quét lặp không tạo lượt phục vụ mới. Đơn hủy/đã hoàn tất không check-in lại.
3. Mở **Gọi món / tính tiền**, chọn món trong thực đơn và số lượng. Hệ thống áp dụng mức giảm cao nhất đang hiệu lực, không cộng dồn. Giá và phần trăm giảm lưu theo từng lần gọi; món đặt trước giữ giá đã xác nhận.
4. Kiểm tra tiền còn lại = tổng sau giảm − cọc; thu đủ rồi bấm **Xuất bill**. Nếu món vừa thay đổi ở cửa sổ khác thì phải kiểm tra tổng mới trước khi xác nhận.
5. In hoặc lưu PDF bằng nút trên hóa đơn. Đơn được ẩn khỏi danh sách hoạt động và giữ trong lịch sử, không xóa dữ liệu. Đây là phiếu tính tiền nội bộ, không phải hóa đơn thuế.

## Kỹ thuật & kiểm tra
- Giữ bàn hết hiệu lực theo UTC trong SQL, không phụ thuộc bộ đếm trình duyệt hay việc có đóng tab. Trạng thái trang bàn cập nhật mỗi 15 giây.
- Khóa giao dịch theo bàn ngăn hai khách giữ/đặt trùng; kiểm tra lại thời hạn và trạng thái khi xác nhận. Gửi lặp thanh toán, thêm món và xuất bill không nhân đôi kết quả.
- Tiền và khuyến mãi tính ở máy chủ. QR chỉ chứa mã ngẫu nhiên; ảnh QR và chi tiết đơn chỉ chủ đơn xem được ở cổng khách.
- Cập nhật schema: `Database/20261014_BookingWorkflow.sql` (đã áp dụng trên máy này). Các cột/bảng mới được thêm, dữ liệu cũ được giữ và có mã check-in.
- QR tạo cục bộ bằng [QRCoder 1.6.0](https://github.com/Shane32/QRCoder); giải mã cục bộ bằng [jsQR 1.4.0](https://github.com/cozmo/jsQR). jsQR và giấy phép MIT được lưu trong `Restaurant.Staff/wwwroot/lib/jsqr`.
- Bộ kiểm tra HTTP đã chạy với tài khoản/dữ liệu tạm và được dọn sau khi chạy. Có kiểm tra đồng thời, hết hạn, cô lập khách, cọc, QR, check-in, gọi món, chống gửi lặp và lưu hóa đơn.

Đơn chưa check-in tự hủy khi hết 2 giờ từ giờ hẹn (giờ Việt Nam). Cả ba ứng dụng kiểm tra mỗi 30 giây và khi mở các trang đặt bàn. Khi khởi động lại sẽ xử lý đơn quá hạn. Đơn đã check-in/hoàn tất không bị hủy. Cọc giả lập được đánh dấu hoàn; không phát sinh giao dịch tiền thật. Lý do hủy được giữ trong lịch sử.

Nhắc hẹn trong website: từ 2 giờ trước lịch hẹn đến hạn check-in, tự cập nhật 30 giây khi trang đang mở. Không gửi SMS/email. Khách chỉ thấy đơn của mình; lễ tân thấy các khách sắp đến và đã đến giờ chưa check-in. Đơn đến giờ được ưu tiên đầu danh sách. Cho phép check-in sớm khi bàn trống và không ảnh hưởng lượt đặt/giữ chỗ khác; hạn tự hủy vẫn là giờ hẹn + 2 giờ. Điện thoại hiển thị theo hồ sơ khách hàng trên đơn và hóa đơn.
