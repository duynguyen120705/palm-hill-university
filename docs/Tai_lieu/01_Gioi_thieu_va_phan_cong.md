# 01. GIỚI THIỆU CHUNG VÀ PHÂN CÔNG

Ứng dụng quản lý sinh viên Student Portal

Bối cảnh bài toán, các giải pháp liên quan, mục tiêu đề tài và phân công nhiệm vụ của Nhóm 01.

## 1.1. Giới thiệu bài toán

### 1.1.1. Bối cảnh và nhu cầu thực tế

Chuyển đổi số trong giáo dục đại học đang được đẩy mạnh, trong đó số hóa hồ sơ người học là nhiệm vụ nền tảng. Quyết định số 131/QĐ-TTg ngày 25/01/2022 của Thủ tướng Chính phủ phê duyệt Đề án "Tăng cường ứng dụng công nghệ thông tin và chuyển đổi số trong giáo dục và đào tạo giai đoạn 2022 – 2025, định hướng đến năm 2030" đặt mục tiêu xây dựng cơ sở dữ liệu ngành và quản trị cơ sở giáo dục trên môi trường số.

Tại một trường đại học, hồ sơ sinh viên được tổ chức theo ba cấp: Khoa chuyên môn, Khóa tuyển sinh và Lớp hành chính. Phòng Công tác sinh viên và giáo vụ các khoa thường xuyên phải tiếp nhận sinh viên mới, cập nhật thông tin, xử lý biến động học tập (bảo lưu, thôi học, tốt nghiệp, chuyển lớp), lập danh sách và báo cáo thống kê theo yêu cầu của nhà trường. Sinh viên cũng cần một kênh để tra cứu và tự cập nhật thông tin liên hệ của mình.

Đề tài "Xây dựng ứng dụng quản lý sinh viên – Student Portal" hướng tới một hệ thống web tập trung trên nền tảng ASP.NET Core MVC, phục vụ ba nhóm người dùng: Quản trị viên (Admin), Giáo vụ khoa và Sinh viên. Phạm vi gồm quản lý tài khoản – xác thực – phân quyền; quản lý hồ sơ kèm tài khoản sinh viên; tra cứu, thống kê, xuất/nhập dữ liệu; quản lý danh mục Khoa/Khóa/Lớp và nhật ký thao tác. Các nghiệp vụ đăng ký học phần, tính điểm và tuyển sinh nằm ngoài phạm vi.

### 1.1.2. Thực trạng và vấn đề tồn tại

Qua khảo sát cách quản lý hồ sơ sinh viên phổ biến hiện nay (mục 1.2), nhóm nhận thấy bốn vấn đề chính:

**Vấn đề 1: Dữ liệu phân tán, thiếu nhất quán.** Hồ sơ sinh viên nằm rải rác trong nhiều file Excel của từng khoa, từng khóa. Một sinh viên chuyển lớp hoặc bảo lưu phải sửa ở nhiều nơi; mã sinh viên có thể bị trùng hoặc bị dùng lại; xóa nhầm một dòng là mất dữ liệu vĩnh viễn, không có dấu vết.

**Vấn đề 2: Tra cứu, thống kê và báo cáo thủ công.** Muốn tìm một sinh viên phải mở nhiều file; tìm kiếm phụ thuộc chính xác dấu tiếng Việt; số liệu thống kê theo khoa, khóa, lớp, trạng thái phải tổng hợp tay, dễ sai lệch và chậm khi nhà trường yêu cầu gấp.

**Vấn đề 3: Phân quyền lỏng lẻo, rủi ro lộ dữ liệu cá nhân.** File Excel được gửi qua email, ai có file cũng xem được toàn bộ thông tin cá nhân của sinh viên các khoa khác; không ghi nhận ai đã sửa, xóa hay xuất dữ liệu – trái với yêu cầu bảo vệ dữ liệu cá nhân của Nghị định 13/2023/NĐ-CP.

**Vấn đề 4: Sinh viên khó tiếp cận và cập nhật thông tin của mình.** Sinh viên muốn kiểm tra hoặc sửa số điện thoại, email phải liên hệ trực tiếp giáo vụ; thông tin liên hệ vì vậy thường lỗi thời, gây khó khăn khi nhà trường cần liên lạc.

Các vấn đề trên cho thấy cần một hệ thống quản lý sinh viên tập trung, phân quyền chặt theo khoa, có truy vết và có cổng tự phục vụ cho sinh viên.

### 1.1.3. Cơ sở pháp lý

- **Thông tư 08/2021/TT-BGDĐT** ban hành Quy chế đào tạo trình độ đại học: quy định các tình huống nghỉ học tạm thời (bảo lưu), thôi học, công nhận tốt nghiệp – là căn cứ cho các trạng thái học tập Đang học, Bảo lưu, Thôi học, Đã tốt nghiệp và quy tắc chuyển trạng thái (FR-16).
- **Thông tư 10/2016/TT-BGDĐT** ban hành Quy chế công tác sinh viên đối với chương trình đào tạo đại học hệ chính quy: quy định trách nhiệm quản lý hồ sơ, thông tin sinh viên của nhà trường và các đơn vị (phòng công tác sinh viên, khoa).
- **Nghị định 13/2023/NĐ-CP** về bảo vệ dữ liệu cá nhân: yêu cầu xử lý dữ liệu cá nhân đúng mục đích, giới hạn người được truy cập, có biện pháp bảo vệ và lưu vết khi xử lý – là căn cứ cho phân quyền theo khoa (FR-09), nhật ký thao tác (NFR-07) và bảo vệ dữ liệu khi xuất file (NFR-08).
- **Quyết định 131/QĐ-TTg (2022)** về chuyển đổi số trong giáo dục và đào tạo: định hướng số hóa hồ sơ, quản trị cơ sở giáo dục trên môi trường số.

## 1.2. Đánh giá các giải pháp liên quan

Hệ thống quản lý sinh viên của các trường phần lớn là hệ thống nội bộ, vì vậy nhóm đánh giá theo bốn nhóm giải pháp đang được sử dụng phổ biến.

### 1.2.1. Quản lý bằng bảng tính Excel và hồ sơ giấy

**Ưu điểm:** chi phí bằng 0, quen thuộc, linh hoạt. **Nhược điểm:** dữ liệu phân tán theo file; không kiểm soát trùng mã; không phân quyền theo khoa; không lưu vết sửa/xóa; tìm kiếm phụ thuộc dấu tiếng Việt; thống kê phải làm tay.

### 1.2.2. Phần mềm quản lý đào tạo tổng thể (thương mại)

**Ưu điểm:** tích hợp nhiều phân hệ (tuyển sinh, học phần, điểm, học phí, hồ sơ). **Nhược điểm:** chi phí bản quyền và triển khai cao; giao diện phức tạp với nghiệp vụ hồ sơ đơn giản; tùy biến quy tắc riêng (phạm vi theo khoa, quy tắc xóa mềm) phụ thuộc nhà cung cấp.

### 1.2.3. Cổng thông tin sinh viên của các trường

**Ưu điểm:** sinh viên tự tra cứu thông tin, điểm, lịch học. **Nhược điểm:** nhiều cổng chỉ cho xem, không cho sinh viên tự cập nhật thông tin liên hệ; giao diện trên điện thoại chưa tối ưu; phần quản trị hồ sơ phía giáo vụ tách rời.

### 1.2.4. Các ứng dụng quản lý sinh viên dạng đồ án, mã nguồn mở

**Ưu điểm:** đơn giản, dễ học tập. **Nhược điểm:** thường chỉ dừng ở CRUD; xóa cứng dữ liệu; phân quyền một cấp (chỉ Admin); không có phạm vi dữ liệu theo khoa, không có nhật ký, không có quy tắc chuyển trạng thái học tập; tìm kiếm phải đúng dấu.

Từ đánh giá trên, Student Portal kế thừa ưu điểm tập trung dữ liệu và cổng sinh viên, đồng thời bổ sung những điểm các giải pháp đơn giản còn thiếu: phân quyền ba vai trò kèm phạm vi theo khoa, xóa mềm, quy tắc chuyển trạng thái, tìm kiếm không dấu, nhật ký thao tác và xuất/nhập Excel.

## 1.3. Mục tiêu của đề tài

| **Vấn đề** | **Mục tiêu cụ thể** |
|----|----|
| Vấn đề 1: Dữ liệu phân tán, thiếu nhất quán | MT1: Xây dựng CSDL tập trung tổ chức theo Khoa – Khóa – Lớp; mã sinh viên duy nhất và không dùng lại; hồ sơ và tài khoản sinh viên được tạo trong cùng một giao dịch; xóa mềm thay cho xóa cứng; quy tắc chuyển trạng thái học tập và chuyển lớp được kiểm soát ở tầng nghiệp vụ. |
| Vấn đề 2: Tra cứu, thống kê, báo cáo thủ công | MT2: Cung cấp danh sách, tìm kiếm không phân biệt hoa thường và dấu tiếng Việt bằng Ajax (không tải lại trang); thống kê theo Khoa/Khóa/Lớp/Trạng thái; xuất danh sách và báo cáo ra Excel/PDF; nhập danh sách sinh viên từ Excel có báo cáo lỗi từng dòng. |
| Vấn đề 3: Phân quyền lỏng lẻo, rủi ro lộ dữ liệu cá nhân | MT3: Xác thực bằng ASP.NET Core Identity, phân quyền ba vai trò Admin – Giáo vụ – Sinh viên; giới hạn phạm vi dữ liệu theo khoa phụ trách và quyền sở hữu hồ sơ ở mọi endpoint (kể cả Ajax, xuất, nhập file); ghi nhật ký mọi thao tác nhạy cảm; chống dò mật khẩu, CSRF, XSS, SQL Injection. |
| Vấn đề 4: Sinh viên khó tiếp cận thông tin của mình | MT4: Cổng sinh viên cho phép xem hồ sơ, tự cập nhật SĐT/Email/Địa chỉ và đổi mật khẩu; giao diện responsive, dùng tốt trên điện thoại; menu và chức năng hiển thị theo vai trò. |

Bên cạnh bốn mục tiêu nghiệp vụ, sản phẩm phải đáp ứng các yêu cầu kỹ thuật của bài tập lớn:

| **Yêu cầu của BTL** | **Cách đáp ứng trong Student Portal** |
|----|----|
| Công nghệ ASP.NET Core MVC, HTML, CSS | ASP.NET Core MVC (.NET 8 LTS), Razor View, Bootstrap 5, CSS dùng chung (mục 3.2.1). |
| Sử dụng các điều khiển Validator | Data Annotations + Custom Validation Attribute (MaSo, HoTen, NgaySinh, SoDienThoai, MatKhau) ở server; jQuery Validation Unobtrusive ở client (NFR-05, mục 3.2.2.4). |
| Khai thác CSDL và kết gán dữ liệu | EF Core + SQL Server; Tag Helper asp-for, asp-items cho form và dropdown phụ thuộc Khoa → Khóa → Lớp; bảng danh sách render từ JSON. |
| Xử lý bằng kỹ thuật AJAX | Tìm kiếm/lọc/phân trang danh sách, thêm – sửa – đổi trạng thái – chuyển lớp – xóa mềm qua modal, danh mục, thống kê, nhật ký đều dùng Ajax/Fetch trả JSON theo quy ước chung (mục 3.2.1.5). |
| Xác thực và phân quyền | Identity + \[Authorize(Roles)\] + bộ lọc phạm vi dữ liệu theo khoa (IScopeService, FR-08, FR-09). |
| An toàn thông tin | HTTPS, cookie HttpOnly/Secure, hash mật khẩu có salt, khóa tạm 15 phút sau 5 lần sai, anti-forgery token, mã hóa đầu ra Razor chống XSS, truy vấn tham số hóa qua EF Core, nhật ký thao tác (NFR-01, NFR-07, NFR-08). |
| Responsive, SEO | Bootstrap grid, menu thu gọn trên mobile, kiểm thử ≥ 2 kích thước màn hình. SEO cơ bản cho trang công khai (trang đăng nhập/giới thiệu): title, meta description, HTML5 ngữ nghĩa, URL thân thiện; các trang nội bộ chứa dữ liệu cá nhân đặt noindex để không bị lập chỉ mục. |

## 1.4. Phân công nhiệm vụ

Mỗi thành viên phụ trách trọn gói một phân hệ (giao diện, Controller, Service) theo tài liệu Phân công công việc và ma trận RACI của nhóm. Ký hiệu: **P** – phụ trách chính (Responsible); **S** – hỗ trợ, tích hợp hoặc kiểm thử chéo. Cặp kiểm thử chéo: Khánh ↔ Dũng, Đức Anh ↔ Duy.

<table>
<thead>
<tr>
<th><strong>STT</strong></th>
<th><strong>Luồng / Công việc</strong></th>
<th><strong>Khánh</strong></th>
<th><strong>Đức Anh</strong></th>
<th><strong>Duy</strong></th>
<th><strong>Dũng</strong></th>
</tr>
</thead>
<tbody>
<tr>
<td colspan="6"><strong>I. Tài khoản, xác thực và phân quyền (FR-01 – FR-12, NFR-01, NFR-08)</strong></td>
</tr>
<tr>
<td>1</td>
<td>Cấu hình ASP.NET Core Identity: ApplicationUser mở rộng, vai trò Admin/CanBo/SinhVien, chính sách mật khẩu, khóa tạm, cookie</td>
<td>P</td>
<td>S</td>
<td></td>
<td>S</td>
</tr>
<tr>
<td>2</td>
<td>IAccountService – dịch vụ tài khoản dùng chung (tạo TK sinh viên trong giao dịch của bên gọi, cấp lại MK, khóa/mở khóa)</td>
<td>P</td>
<td>S</td>
<td></td>
<td>S</td>
</tr>
<tr>
<td>3</td>
<td>IScopeService – bộ lọc phạm vi dữ liệu theo khoa và quyền sở hữu hồ sơ (FR-09)</td>
<td>P</td>
<td>S</td>
<td>S</td>
<td>S</td>
</tr>
<tr>
<td>4</td>
<td>AccountController: đăng nhập, đăng xuất, đổi mật khẩu, hồ sơ cá nhân, cập nhật liên hệ (FR-06, 07, 10, 11, 12)</td>
<td>P</td>
<td></td>
<td></td>
<td>S</td>
</tr>
<tr>
<td>5</td>
<td>UsersController + View: danh sách, tạo, cập nhật, cấp lại MK, khóa/mở khóa tài khoản Admin/Giáo vụ (FR-01 – FR-05)</td>
<td>P</td>
<td></td>
<td></td>
<td>S</td>
</tr>
<tr>
<td colspan="6"><strong>II. Hồ sơ sinh viên và dữ liệu (FR-13 – FR-20, NFR-02, NFR-04)</strong></td>
</tr>
<tr>
<td>6</td>
<td>Thiết kế ERD, entity, AppDbContext, cấu hình Fluent API, migration, dữ liệu mẫu ~1.000 sinh viên</td>
<td>S</td>
<td>P</td>
<td>S</td>
<td>S</td>
</tr>
<tr>
<td>7</td>
<td>IStudentService: thêm hồ sơ kèm tài khoản, cập nhật, đổi trạng thái, chuyển lớp, xóa mềm (quy tắc đặt ở Service để FR-30 dùng lại)</td>
<td>S</td>
<td>P</td>
<td>S</td>
<td></td>
</tr>
<tr>
<td>8</td>
<td>StudentsController + View chi tiết/modal: FR-13 – FR-20, cấp lại MK và khóa/mở khóa TK sinh viên</td>
<td>S</td>
<td>P</td>
<td>S</td>
<td></td>
</tr>
<tr>
<td>9</td>
<td>Chỉ mục CSDL, tối ưu truy vấn, đo thời gian phản hồi (NFR-02)</td>
<td></td>
<td>P</td>
<td>S</td>
<td></td>
</tr>
<tr>
<td colspan="6"><strong>III. Tra cứu, danh mục, nhật ký và giao diện chung (FR-21 – FR-26, FR-31, NFR-03, NFR-06)</strong></td>
</tr>
<tr>
<td>10</td>
<td>Layout chung, menu theo vai trò, CSS và thành phần dùng chung (modal, toast, bảng phân trang), quy ước Ajax/JSON (site.js)</td>
<td>S</td>
<td>S</td>
<td>P</td>
<td></td>
</tr>
<tr>
<td>11</td>
<td>Danh sách sinh viên, tìm kiếm không dấu, lọc Khoa/Khóa/Lớp/Trạng thái (FR-21, FR-22)</td>
<td></td>
<td>S</td>
<td>P</td>
<td>S</td>
</tr>
<tr>
<td>12</td>
<td>Faculties/Courses/ClassesController + View: danh mục Khoa, Khóa, Lớp (FR-23 – FR-26)</td>
<td></td>
<td>S</td>
<td>P</td>
<td></td>
</tr>
<tr>
<td>13</td>
<td>AuditLogsController + View: tra cứu nhật ký thao tác (FR-31)</td>
<td></td>
<td>S</td>
<td>P</td>
<td>S</td>
</tr>
<tr>
<td>14</td>
<td>Wireframe/mockup, kiểm thử responsive và tương thích trình duyệt</td>
<td>S</td>
<td>S</td>
<td>P</td>
<td>S</td>
</tr>
<tr>
<td colspan="6"><strong>IV. Thống kê, xuất/nhập dữ liệu (FR-27 – FR-30, NFR-05, NFR-07)</strong></td>
</tr>
<tr>
<td>15</td>
<td>IAuditLogService – dịch vụ ghi nhật ký dùng chung; ValidationRules + Custom Validation Attribute dùng chung</td>
<td>S</td>
<td>S</td>
<td>S</td>
<td>P</td>
</tr>
<tr>
<td>16</td>
<td>StatisticsController + View: thống kê sinh viên dạng bảng (FR-27)</td>
<td>S</td>
<td></td>
<td></td>
<td>P</td>
</tr>
<tr>
<td>17</td>
<td>IExportService: xuất danh sách và báo cáo thống kê ra Excel/PDF (FR-28, FR-29)</td>
<td>S</td>
<td></td>
<td>S</td>
<td>P</td>
</tr>
<tr>
<td>18</td>
<td>ImportController + IImportService: nhập danh sách sinh viên từ Excel, gọi IStudentService và IAccountService (FR-30)</td>
<td>S</td>
<td>S</td>
<td></td>
<td>P</td>
</tr>
<tr>
<td colspan="6"><strong>V. Công việc chung</strong></td>
</tr>
<tr>
<td>19</td>
<td>Kiểm thử chéo theo cặp; kiểm thử tích hợp (Dũng điều phối)</td>
<td>S</td>
<td>S</td>
<td>S</td>
<td>P</td>
</tr>
<tr>
<td>20</td>
<td>Quản lý Git, review Pull Request, tổng hợp báo cáo, slide, chủ trì demo</td>
<td>P</td>
<td>S</td>
<td>S</td>
<td>S</td>
</tr>
<tr>
<td>21</td>
<td>Hướng dẫn cài đặt + script CSDL (Đức Anh); hướng dẫn sử dụng + video demo (Duy)</td>
<td></td>
<td>P</td>
<td>P</td>
<td>S</td>
</tr>
</tbody>
</table>
