# Ba ứng dụng độc lập trên cùng máy

| Ứng dụng | Thư mục | Địa chỉ | Tài khoản |
|---|---|---|---|
| Khách hàng | Restaurant.Customer | http://localhost:5097 | KhachHang |
| Nhân viên | Restaurant.Staff | http://localhost:5098 | NhanVien |
| Admin | Restaurant.Admin | http://localhost:5099 | Admin |

Mỗi ứng dụng có project, tiến trình, cổng, controller, giao diện và phiên đăng nhập riêng. Có thể đăng nhập ba tài khoản đồng thời trên cùng trình duyệt. Đăng xuất hoặc dừng một ứng dụng không ảnh hưởng hai ứng dụng còn lại. Mỗi trang đăng nhập chỉ xác thực đúng loại tài khoản của ứng dụng đó, không có lựa chọn vai trò.

Restaurant.Shared chứa model, kết nối Entity Framework, kiểm tra mật khẩu và thiết lập phiên đăng nhập. Thư viện này được biên dịch và sao chép vào từng ứng dụng, không phải một máy chủ thứ tư. Cả ba cùng dùng SQL Server QuanLyNhaHang hiện có: khách đặt bàn thì nhân viên và admin xem được. Dữ liệu không tự đồng bộ lên trang đang mở; tải lại danh sách để thấy thay đổi. Nếu SQL Server dừng thì cả ba đều bị ảnh hưởng.

## Chạy trong VS Code

Mở Restaurant-3Apps.code-workspace. Cài .NET 10 SDK và bảo đảm SQL Server đang chạy. Các appsettings.json đã sao chép cấu hình kết nối hiện có; khi đổi máy chủ SQL, sửa kết nối tương ứng trong cả ba ứng dụng.

Dừng bản RestaurantManagement cũ bằng Ctrl+C nếu nó đang chiếm cổng 5097. Mã nguồn cũ được giữ nguyên để tham khảo; không chạy nó đồng thời với Customer trên cùng cổng.

Trong terminal tại thư mục chứa Restaurant.slnx:

```powershell
dotnet build Restaurant.slnx -m:1
```

Mở ba terminal và chạy mỗi terminal một lệnh:

```powershell
dotnet run --project Restaurant.Customer
```

```powershell
dotnet run --project Restaurant.Staff
```

```powershell
dotnet run --project Restaurant.Admin
```

Ctrl+C trong một terminal chỉ dừng ứng dụng tương ứng. Khi sửa mã nguồn của ứng dụng nào, khởi động lại ứng dụng đó; khi sửa Restaurant.Shared, build và khởi động lại cả ba để dùng cùng phiên bản.

Cũng có thể dùng Terminal → Run Task trong VS Code, chọn ứng dụng hoặc “Chạy cả ba”. Nếu muốn quản lý cả ba từ một terminal, chạy `./Start-All.ps1`; Ctrl+C tại terminal đó dừng cả ba tiến trình mà script đã tạo. Script không tự dừng tiến trình khác nếu cổng bị chiếm.

## Đăng nhập

- Khách: http://localhost:5097/TaiKhoan/DangNhap — có đăng ký khách.
- Nhân viên: http://localhost:5098/TaiKhoan/DangNhap — dùng tài khoản NhanVien đã có.
- Admin: http://localhost:5099/TaiKhoan/DangNhap — dùng tài khoản Admin đã có.

Admin quản lý tại /Admin, nhân viên tại /NhanVien trong chính ứng dụng của mình. Đường dẫn vùng khác trả 404 vì controller đó không được đưa vào project. Không có trang đăng ký công khai cho nhân viên hay admin; admin tạo nhân viên trong khu vực quản lý.

Session, antiforgery và TempData dùng tên cookie riêng theo vai trò; Data Protection có tên ứng dụng riêng. Việc chỉ đổi cổng không đủ tách cookie trên localhost, nên các thiết lập này được tách rõ. Phiên đăng nhập dùng bộ nhớ của từng tiến trình; khởi động lại một ứng dụng yêu cầu đăng nhập lại vào ứng dụng đó.

## Kết quả kiểm tra

- Build toàn bộ solution: 0 lỗi, 0 cảnh báo.
- Chạy ba server đồng thời trên cổng kiểm thử riêng.
- Đăng nhập ba vai trò trong cùng một cookie jar: thành công, ba cookie session khác nhau.
- Đăng xuất nhân viên: khách và admin vẫn truy cập được trang có đăng nhập.
- Dừng tiến trình nhân viên: khách và admin vẫn trả HTTP 200.
- Truy cập vùng khác và đăng ký nội bộ: HTTP 404.
- Các tài khoản kiểm thử tạm đã được xóa, xác nhận còn 0 bản ghi thử. Không đổi tài khoản người dùng hiện có.

Các luồng đặt bàn và khuyến mãi dùng lại bản sửa hiện tại. Phép kiểm thử lần này tập trung vào tách ứng dụng và cô lập phiên đăng nhập, không kiểm thử lại toàn bộ nghiệp vụ.
