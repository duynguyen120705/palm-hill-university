# 06. KẾT LUẬN

Ứng dụng quản lý sinh viên Student Portal

Đánh giá so với mục tiêu, kết quả đạt được, hạn chế, hướng phát triển và bài học kinh nghiệm.

## 5.1. Đánh giá sản phẩm so với mục tiêu đã đề ra

| **Mục tiêu** | **Kết quả** | **Minh chứng** |
|----|----|----|
| MT1: CSDL tập trung, mã SV duy nhất, hồ sơ và tài khoản cùng giao dịch, xóa mềm, kiểm soát chuyển trạng thái và chuyển lớp | Đạt. Lỗi thêm đồng thời cùng mã trả 500 thay vì 409 đã được sửa và kiểm thử lại (TC045, TC158, TC161) | Các ca UC09 – UC16, UC19 |
| MT2: Danh sách, tìm kiếm không dấu bằng Ajax, thống kê, xuất Excel/PDF, nhập Excel có báo lỗi từng dòng | Đạt. Lỗi tìm kiếm bỏ sót họ tên viết bằng chữ Ð (U+00D0) đã được sửa và kiểm thử lại (TC150) | Các ca UC17, UC18, UC20 – UC23 |
| MT3: Identity, ba vai trò, phạm vi theo khoa ở mọi endpoint, nhật ký thao tác, chống dò mật khẩu | Đạt | Các ca UC01 – UC08, UC24, NFR-01 |
| MT4: Cổng sinh viên xem hồ sơ, cập nhật liên hệ, đổi mật khẩu; giao diện responsive | Đạt | Các ca UC03 – UC05, NFR-03 |

## 5.2. Kết quả đạt được

- Xây dựng ứng dụng web Student Portal trên ASP.NET Core MVC đáp ứng 31/31 yêu cầu chức năng FR-01 – FR-31 của SRS, gồm 11 Controller, 9 dịch vụ (trong đó 3 dịch vụ dùng chung: phạm vi dữ liệu, tài khoản, nhật ký) và 9 file script riêng cho từng trang; mỗi FR đều có ít nhất một ca kiểm thử ở mục 4.5.
- Phân quyền ba vai trò, phạm vi dữ liệu theo khoa áp dụng thống nhất ở tầng dịch vụ, kể cả xuất và nhập file.
- Bộ 161 ca kiểm thử xây dựng từ đặc tả 24 Use Case, chạy tự động trên trình duyệt, 161 ca đạt; phát hiện hai lỗi (xử lý đồng thời ở FR-13, tìm kiếm chữ Ð ở FR-22), đã sửa và chạy lại để xác nhận.
- Quy trình làm việc nhóm theo mã yêu cầu: tên nhánh và commit gắn FR/NFR, gộp qua Pull Request, kiểm tra chất lượng bằng SonarCloud Quality Gate.
- Bộ tài liệu phân tích – thiết kế (Chương 2, 3) bám sát mã nguồn: tên Controller/Action, interface dùng chung, quy ước JSON và cấu trúc CSDL trong mã đúng với mục 3.2.

## 5.3. Hạn chế của hệ thống

- Chu kỳ hỗ trợ của .NET 8 kết thúc ngày 10/11/2026. Trước khi triển khai thật cần nâng TargetFramework, các gói EF Core/Identity và global.json lên .NET 10, bản hỗ trợ dài hạn hiện hành.
- Chưa có kiểm thử đơn vị và kiểm thử tích hợp trong solution; bộ kiểm thử ở mục 4.5 chạy ngoài quy trình build.
- Kiểm thử tải mới chạy trên máy phát triển.
- Không có chức năng khôi phục mật khẩu qua email; người dùng quên mật khẩu phải liên hệ Admin hoặc Giáo vụ (theo phạm vi SRS).
- Chưa có quản lý điểm, đăng ký học phần, tuyển sinh (ngoài phạm vi đề tài).

## 5.4. Hướng phát triển

- Nâng cấp lên .NET 10.
- Bổ sung project kiểm thử (xUnit) cho các quy tắc nghiệp vụ trong StudentService, ImportService, ScopeService và đưa bộ kiểm thử giao diện vào GitHub Actions.
- Khôi phục mật khẩu qua email/OTP; đăng nhập một lần (SSO) với hệ thống của trường.
- Biểu đồ thống kê trực quan trên trang Thống kê; lịch sử biến động hồ sơ theo từng sinh viên.
- Tích hợp đăng ký học phần, quản lý điểm khi mở rộng phạm vi.

## 5.5. Bài học kinh nghiệm

- Chốt sớm các interface dùng chung (IScopeService, IAccountService, IAuditLogService) và quy ước JSON giúp bốn thành viên làm song song các phân hệ mà vẫn ghép được với nhau.
- Đặt quy tắc nghiệp vụ ở tầng dịch vụ thay vì Controller cho phép chức năng nhập Excel dùng lại toàn bộ quy tắc thêm, sửa, chuyển lớp, đổi trạng thái của hồ sơ.
- Kiểm tra phân quyền phải làm ở phía server cho từng API; ẩn nút trên giao diện là chưa đủ, các ca gọi API trực tiếp (TC048, TC066, TC095) cho thấy điều đó.
- Kiểm tra tồn tại rồi mới ghi chưa đủ khi có yêu cầu đồng thời; ràng buộc trong CSDL cần được bắt và chuyển thành thông báo nghiệp vụ.
- Gắn mã FR vào nhánh, commit và ca kiểm thử giúp truy vết từ yêu cầu đến mã nguồn và kết quả kiểm thử.
