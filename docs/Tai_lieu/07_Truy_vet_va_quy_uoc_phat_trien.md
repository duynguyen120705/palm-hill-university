# 07. TRUY VẾT YÊU CẦU VÀ QUY ƯỚC PHÁT TRIỂN

Ứng dụng quản lý sinh viên Student Portal

Ma trận truy vết FR – NV – UC – Controller – người phụ trách và các quy ước Git, Definition of Done, mã nguồn.

## PHỤ LỤC A: MA TRẬN TRUY VẾT YÊU CẦU

Ma trận liên kết mỗi yêu cầu chức năng với nghiệp vụ, Use Case, Controller, thành viên phụ trách và Sprint thực hiện theo kế hoạch 8 tuần.

| **FR** | **NV** | **UC** | **Controller** | **Phụ trách** | **Sprint** |
|----|:--:|:--:|----|:--:|:--:|
| FR-01 – 05 | NV01 | UC06 – 08 | Users | Khánh | S1 (T4) |
| FR-06, 07 | NV02 | UC01, 02 | Account | Khánh | S1 (T4) |
| FR-08, 09 | NV03 | Tất cả | IScopeService | Khánh | S1 (T4) |
| FR-10 | NV04 | UC03 | Account | Khánh | S1 (T4) |
| FR-11, 12 | NV05 | UC04, 05 | Account | Khánh | S2 (T5) |
| FR-13 | NV06 | UC09 | Students | Đức Anh | S1 – S2 |
| FR-14, 15 | NV07 | UC10, 11 | Students | Đức Anh | S2 (T5) |
| FR-16 | NV08 | UC12 | Students | Đức Anh | S2 (T5) |
| FR-17 | NV09 | UC13 | Students | Đức Anh | S2 (T5) |
| FR-18 | NV10 | UC14 | Students | Đức Anh | S2 (T5) |
| FR-19, 20 | NV11 | UC15, 16 | Students | Đức Anh | S2 (T5) |
| FR-21, 22 | NV12 | UC17, 18 | Students (Index/Search) | Duy | S2 (T5) |
| FR-23 – 26 | NV13 | UC19 | Faculties/Courses/Classes | Duy | S1 (T4) |
| FR-27 | NV14 | UC20 | Statistics | Dũng | S2 (T5) |
| FR-28, 29 | NV14 | UC21, 22 | Students/Statistics Export | Dũng | S3 (T6) |
| FR-30 | NV15 | UC23 | Import | Dũng | S2 – S3 |
| FR-31 | NV16 | UC24 | AuditLogs | Duy | S3 (T6) |
| NFR-05, 07 | NV16 | — | ValidationRules, IAuditLogService | Dũng | S1 (T4) |
| NFR-02 | — | — | Chỉ mục, seed 1.000 SV | Đức Anh | S3 (T6) |

## PHỤ LỤC B: QUY ƯỚC PHÁT TRIỂN

### B.1. Git

- Nhánh chính main (luôn chạy được); mỗi chức năng một nhánh feature/FR-xx-ten-ngan (ví dụ feature/FR-13-them-ho-so).
- Commit message bắt đầu bằng mã yêu cầu: "FR-13: thêm giao dịch tạo hồ sơ + tài khoản".
- Gộp vào main qua Pull Request, tối thiểu 1 thành viên review (ưu tiên cặp kiểm thử chéo); migration mới được thông báo cho cả nhóm trước khi gộp để tránh xung đột.
- Không commit chuỗi kết nối thật, mật khẩu Admin, file dữ liệu cá nhân; dùng user-secrets và appsettings.Development.json mẫu.

### B.2. Định nghĩa hoàn thành (Definition of Done) cho mỗi FR

- Đủ luồng chính và mọi ngoại lệ trong SRS/UC; validate client + server bằng ValidationRules dùng chung.
- Có \[Authorize(Roles)\] và đi qua IScopeService; thử truy cập ngoài phạm vi trả 403.
- POST có anti-forgery; Ajax trả JSON đúng quy ước mục 3.2.1.5.
- Thao tác nhạy cảm có ghi nhật ký, không ghi dữ liệu cá nhân/mật khẩu.
- Kiểm tra giao diện ở desktop và mobile (375 px) trên Chrome và một trình duyệt khác.
- Test case của FR đã chạy đạt; được thành viên kiểm thử chéo xác nhận.

### B.3. Quy ước mã nguồn

- C#: PascalCase cho lớp/phương thức/thuộc tính, camelCase cho biến; async có hậu tố Async; một lớp một file.
- Thực thể và cột CSDL: tiếng Việt không dấu PascalCase (SinhVien, MaSV); Controller/Action tiếng Anh (StudentsController.Create).
- Controller không chứa quy tắc nghiệp vụ; không gọi DbContext trực tiếp từ Controller.
- JavaScript mỗi trang một file wwwroot/js/pages/\<trang\>.js; chỉ dùng helper trong site.js để gọi Ajax.
