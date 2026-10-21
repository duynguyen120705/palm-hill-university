# BÁO CÁO THỰC HÀNH 7 – TÍCH HỢP AJAX

**Module:** Tài khoản, xác thực và phân quyền (FR-01 – FR-12)
**Thành viên thực hiện:** Lê Nam Khánh

---

## I. ÁP DỤNG VÀO MODULE ĐƯỢC PHÂN CÔNG

| Yêu cầu | Chức năng áp dụng | Công cụ phía client | Endpoint |
| --- | --- | --- | --- |
| 1. UI Update | Bảng tài khoản Admin/Giáo vụ (FR-01) | `XMLHttpRequest` | `GET /TaiKhoan/Data`, `GET /TaiKhoan/Khoas` |
| 2. CRUD AJAX | **Create:** tạo tài khoản (FR-02)<br>**Delete:** khóa tài khoản (FR-05)<br>Kèm: cập nhật (FR-03), cấp lại mật khẩu (FR-04), đổi mật khẩu (FR-10), cập nhật liên hệ (FR-12) | jQuery `$.ajax` | `POST /TaiKhoan/Create`<br>`POST /TaiKhoan/DoiTrangThai`<br>`POST /TaiKhoan/Update`, `/TaiKhoan/DatLaiMatKhau`<br>`POST /HoSo/DoiMatKhau`, `/HoSo/CapNhatLienHe` |
| 3. Pagination | Danh sách tài khoản (lọc vai trò, khoa, trạng thái) | `XMLHttpRequest` + EF Core `Skip/Take` | `GET /TaiKhoan/Data?page=&pageSize=` |
| 4. AJAX Search | Tìm tài khoản theo tên đăng nhập / họ tên, không dấu | `fetch()` | `GET /TaiKhoan/Data?keyword=` |

**Vì sao "Delete" là khóa tài khoản:** FR-05 quy định *"Tài khoản chỉ được khóa hoặc mở khóa, không bị xóa khỏi hệ thống. Hệ thống luôn duy trì ít nhất một Admin hoạt động."* Xóa vật lý sẽ làm mất dấu vết trong nhật ký và phá liên kết hồ sơ – tài khoản. Vì vậy thao tác Delete của TH7 được hiện thực bằng khóa tài khoản: bản ghi được giữ, trạng thái chuyển `BiKhoa`, dòng trên bảng đổi trạng thái ngay mà không tải lại trang.

### Nền tảng xác thực (FR-06 – FR-09)

| Thành phần | Nội dung |
| --- | --- |
| `AccountController` | Đăng nhập/đăng xuất; thông báo lỗi chung (không lộ tên đăng nhập tồn tại hay bị khóa); sai 5 lần khóa tạm 15 phút; mở giao diện theo vai trò; ghi `LanDangNhapDauTien` |
| `AppSignInManager` | Tài khoản `BiKhoa` không đăng nhập được; mỗi request xác thực lại `SecurityStamp` + trạng thái (`ValidationInterval = 0`) → tài khoản bị khóa / đổi quyền / đổi khoa bị từ chối từ yêu cầu kế tiếp |
| Cookie (`Program.cs`) | HttpOnly, Secure, SameSite=Lax, hết hạn 60 phút trượt; request Ajax nhận **401/403** thay vì bị chuyển hướng sang trang HTML |
| `PhamViDuLieuService` | Bộ lọc phạm vi dùng chung FR-09 (Admin toàn trường, Giáo vụ theo khoa, Sinh viên hồ sơ của mình), dùng bởi module Thống kê/Xuất nhập và các module khác |
| `TaiKhoanService` | Dịch vụ tài khoản dùng chung: tạo tài khoản sinh viên, cấp lại mật khẩu, khóa/mở khóa – chạy trong giao dịch của bên gọi (FR-13, FR-19, FR-20, FR-30) |

### Cấu trúc mã nguồn

| Tầng | Tệp |
| --- | --- |
| View | `Views/Account/Login.cshtml`, `AccessDenied.cshtml`; `Views/TaiKhoan/Index.cshtml`; `Views/HoSo/Index.cshtml`, `DoiMatKhau.cshtml` |
| JavaScript | `wwwroot/js/taikhoan.js` (XHR + jQuery + fetch), `hoso.js` (jQuery), `phantrang.js` (dùng chung) |
| Controller | `AccountController`, `TaiKhoanController` (`[Authorize(Roles = "Admin")]`), `HoSoController` (`[Authorize]`) |
| Service | `QuanLyTaiKhoanService`, `HoSoCaNhanService`, `TaiKhoanService`, `PhamViDuLieuService`, `AppSignInManager`, `NhatKyService`, `PhanTrangExtensions`, `TimKiemKhongDau` |
| DTO | `Models/DTOs/TaiKhoanDtos.cs`, `HoSoCaNhanDtos.cs`, `DangNhapDto.cs` |

### Nguyên tắc chung

- **Phân quyền tại server:** vai trò kiểm tra bằng `[Authorize(Roles)]`; quy tắc nghiệp vụ (Admin cuối cùng, khoa hợp lệ, quyền sở hữu hồ sơ) kiểm tra ở tầng Service.
- **Định danh từ phiên đăng nhập:** `/HoSo/*` không nhận mã người dùng hay mã SV từ client (FR-09, FR-11).
- **Chặn trường ngoài quyền sửa (FR-12):** `[JsonUnmappedMemberHandling(Disallow)]` – JSON có trường lạ (VD: `hoTen`, `maSV`) bị từ chối 400.
- **Chống CSRF / XSS:** `[ValidateAntiForgeryToken]` + header `RequestVerificationToken`; DOM tạo bằng `textContent` / `.text()`.
- **Giao dịch:** tạo/cập nhật/khóa tài khoản và ghi nhật ký trong cùng giao dịch; lỗi Identity → rollback.
- **Nhật ký (NFR-07):** `TaoTaiKhoan`, `CapNhatTaiKhoan`, `DoiQuyen`, `DoiKhoaPhuTrach`, `CapLaiMatKhau`, `KhoaTaiKhoan`, `MoKhoaTaiKhoan`, `DoiMatKhau`, `CapNhatLienHe` – chỉ lưu định danh, không lưu mật khẩu hay giá trị liên hệ.

---

## II. GIẢI THÍCH VÀ PHÂN TÍCH LUỒNG

```
UI ──sự kiện──► JS ──HTTP──► [Cookie auth + SecurityStamp] ──► Controller [Authorize] ──► Service ──LINQ──► EF Core ──SQL──► DB
                                                                                                                         │
DOM ◄──render── JS ◄──────────────── JSON ◄─────────────────── Controller ◄──── DTO ──── Service ◄─── entity ── EF Core ◄─┘
```

Khác với module dữ liệu thông thường, mọi request của module này đi qua **bước xác thực cookie và kiểm tra SecurityStamp** trước khi tới Controller: tài khoản bị khóa giữa phiên sẽ bị chặn ngay tại đây (401 với Ajax).

### 1. XMLHttpRequest – Bảng tài khoản (FR-01)

| Bước | Thành phần | Diễn giải |
| --- | --- | --- |
| UI | `Views/TaiKhoan/Index.cshtml` | Mở trang hoặc đổi bộ lọc Vai trò / Khoa / Trạng thái. |
| JS | `taikhoan.js` → `loadDanhSach()` → `xhrGetJson()` | `buildUrl()` gom bộ lọc + `page`, `pageSize`; `new XMLHttpRequest()`, `open('GET', url, true)`, header `Accept: application/json`, `send()`. |
| Xác thực | Cookie + `AppSignInManager` | Cookie hợp lệ, SecurityStamp khớp, tài khoản `HoatDong`; không phải Admin → **403**, chưa đăng nhập → **401**. |
| Controller | `TaiKhoanController.Data(TaiKhoanFilterDto)` | Bind query string vào DTO, gọi `QuanLyTaiKhoanService.DanhSachAsync`. |
| EF Core | `QueryTaiKhoan()` | `Users JOIN UserRoles JOIN Roles WHERE Name IN ('Admin','GiaoVu')` – loại tài khoản sinh viên (FR-01); áp bộ lọc; `LEFT JOIN KHOA` lấy tên khoa phụ trách. |
| DB → JSON | SQL Server → `Json(PagedResultDto)` | `{ items: [{ id, userName, hoTen, vaiTro, tenKhoa, trangThai, trangThaiText }], pageIndex, totalItems, totalPages }`. |
| JS | `onreadystatechange` | `readyState === DONE` → kiểm tra `status` → `JSON.parse(responseText)` → callback. |
| DOM | `renderTable()` / `taoDong()` | Tạo `<tr>`, badge trạng thái, nút Sửa / Cấp lại mật khẩu / Khóa hoặc Mở khóa bằng `createElement` + `textContent`. |

### 2. jQuery AJAX – Create và "Delete"

**Create – `POST /TaiKhoan/Create` (FR-02)**

| Bước | Diễn giải |
| --- | --- |
| UI → JS | Mở modal *Tạo tài khoản*, chọn vai trò (ô Khoa chỉ bật với Giáo vụ) → `$.ajax({ type: 'POST', contentType: 'application/json', data: JSON.stringify(dto), headers: { RequestVerificationToken } })`. |
| Controller | `[ValidateAntiForgeryToken]`; `ModelState` kiểm tra DataAnnotations (họ tên không chứa số, tên đăng nhập 3–50 ký tự, mật khẩu ≥ 8 gồm chữ và số, xác nhận khớp). |
| Service | Vai trò ∈ {Admin, GiaoVu}; Giáo vụ bắt buộc một khoa đang sử dụng; tên đăng nhập duy nhất → `BeginTransaction` → `UserManager.CreateAsync` (hash mật khẩu có salt) → `AddToRoleAsync` → ghi nhật ký → `Commit`. |
| DB | `INSERT AspNetUsers`, `INSERT AspNetUserRoles`, `INSERT NHAT_KY_THAO_TAC` trong một giao dịch. |
| JSON → DOM | 200 `{ success, message, data }` → đóng modal, hiện thông báo, tải lại trang hiện tại. 400 → `.fail()` hiện lỗi **trong modal, giữ nguyên dữ liệu đã nhập** (FR-02). |

**"Delete" = Khóa – `POST /TaiKhoan/DoiTrangThai` (FR-05)**

| Bước | Diễn giải |
| --- | --- |
| UI → JS | Bấm nút khóa (event delegation trên `tbody` vì dòng render động) → modal nhập **lý do** → `$.ajax` gửi `{ id, khoa: true, lyDo }`. |
| Service | Thiếu lý do → 400; đã khóa → 400; **là Admin hoạt động cuối cùng → từ chối**; `TaiKhoanService.KhoaAsync`: `TrangThaiTaiKhoan = 0`, `LockoutEnd = MaxValue`, `UpdateSecurityStampAsync`. |
| DB | `UPDATE AspNetUsers SET TrangThaiTaiKhoan, LockoutEnd, SecurityStamp`; ghi `KhoaTaiKhoan`. |
| Hiệu ứng | Request kế tiếp của người bị khóa: SecurityStamp không khớp → cookie bị từ chối → **401** (đã kiểm thử). |
| JSON → DOM | `capNhatDong(res.data)` thay đúng một dòng (badge "Bị khóa", nút đổi thành Mở khóa); nếu đang lọc theo trạng thái thì tải lại trang. |

### 3. Phân trang Skip/Take

| Bước | Diễn giải |
| --- | --- |
| UI → JS | Bấm số trang (`phantrang.js`) hoặc đổi số dòng/trang → `loadDanhSach(page)` qua XHR; đổi bộ lọc quay về trang 1. |
| EF Core | `PhanTrangExtensions.ToPagedResultAsync`: `pageSize ≤ 100` → `CountAsync()` → kẹp `page` vào `[1, totalPages]` → `OrderBy(UserName)` → `Skip((page-1)*pageSize).Take(pageSize)` → `Select(ItemSelector)`. |
| DB | `SELECT COUNT(*) ...` và `... ORDER BY [UserName] OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY` (đã xác nhận trong log EF Core). |
| JSON → DOM | `hienThiTrang(res)`: render bảng, vẽ thanh phân trang, "Tổng: N tài khoản — trang x/y". |

### 4. fetch() – Tìm kiếm

| Bước | Diễn giải |
| --- | --- |
| UI | Gõ vào ô tìm kiếm (VD: `hung`). |
| JS | Sự kiện `input` + **debounce 300 ms** → `timKiem()`: `fetchController.abort()` hủy request cũ, `new AbortController()`, `await fetch(url, { signal })`. |
| Kiểm tra lỗi | `if (!res.ok)` – fetch **không reject** với 401/403/500; `AbortError` được bỏ qua. |
| Thứ tự phản hồi | Mỗi request (XHR hoặc fetch) mang `requestSeq`; chỉ phản hồi của request mới nhất được render, tránh kết quả cũ về muộn ghi đè kết quả mới. |
| EF Core → DB | `EF.Functions.Collate(UserName, "Latin1_General_100_CI_AI").Contains(kw) \|\| Collate(HoTen, ...).Contains(kw)` → `WHERE [UserName] COLLATE Latin1_General_100_CI_AI LIKE @kw ESCAPE N'\'` – kết hợp bộ lọc và phân trang. |
| JSON → DOM | Dùng chung `hienThiTrang()` với phân trang. |

**Chọn collation:** đã so sánh trực tiếp trên SQL Server:

| Từ khóa → tên | `Vietnamese_CI_AI` | `Latin1_General_100_CI_AI` |
| --- | --- | --- |
| hung → Hưng, do → Đỗ, van → Văn, duc → Đức, hông → Hồng | không khớp | khớp |
| nguyen → Nguyễn, quoc → Quốc, hong → Hồng | khớp | khớp |

`Vietnamese_CI_AI` coi ư, ơ, ă, đ… là chữ cái riêng nên không đáp ứng yêu cầu tìm không dấu; nhóm dùng `Latin1_General_100_CI_AI` (hằng số `TimKiemKhongDau.Collation`).

### 5. So sánh ba công cụ trong module

| Tiêu chí | XMLHttpRequest | jQuery `$.ajax` | `fetch()` |
| --- | --- | --- | --- |
| Dùng cho | Bảng + phân trang tài khoản | Tạo, sửa, khóa/mở khóa, cấp lại & đổi mật khẩu, cập nhật liên hệ | Tìm kiếm |
| Gửi JSON + token | `setRequestHeader` thủ công | `contentType`, `data: JSON.stringify`, `headers` | `headers`, `body` |
| Phản hồi lỗi | Tự đọc `xhr.status` | `.fail(xhr)` với `xhr.responseJSON` | Tự kiểm tra `res.ok` |
| Hủy request | `xhr.abort()` | `jqXHR.abort()` | `AbortController` (đã dùng) |
| Nhận 401 khi phiên hết hạn / bị khóa | `status === 401` | `.fail`, `xhr.status === 401` | `res.status === 401` |

Phía server giống nhau với cả ba công cụ: cùng cookie xác thực, cùng `[Authorize]`, cùng Service và EF Core; chỉ cách JS gửi/nhận khác nhau. Server phân biệt request Ajax qua header `X-Requested-With`, `Accept: application/json` hoặc `Content-Type: application/json` để trả 401/403 thay vì chuyển hướng HTML.

---

## III. KIỂM THỬ

Chạy trên SQL Server LocalDB với dữ liệu seed, gọi API thật bằng `curl` (cookie + token anti-forgery). Tài khoản: `admin` / `Admin@123`, `gv.cntt` / `GiaoVu@123`, sinh viên `22010004` / `Student@123`. Dữ liệu và mật khẩu thay đổi trong lúc kiểm thử đã được khôi phục.

| Mã | Chức năng | Kịch bản | Kết quả |
| --- | --- | --- | --- |
| TC-01 | FR-06 | Admin / Giáo vụ / Sinh viên đăng nhập đúng | Chuyển tới `/TaiKhoan`, `/ThongKe`, `/HoSo` – **Đạt** |
| TC-02 | FR-06 | Sai mật khẩu; tên đăng nhập không tồn tại | Cùng một thông báo chung – **Đạt** |
| TC-03 | FR-06 | Sai 5 lần liên tiếp, lần 6 đúng mật khẩu | Vẫn bị từ chối (khóa tạm 15 phút), thông báo chung – **Đạt** |
| TC-04 | FR-07 | Đăng xuất rồi gọi `/TaiKhoan/Data` | 302 về Login; Ajax sau đó nhận 401 – **Đạt** |
| TC-05 | FR-08 | Chưa đăng nhập: gọi Ajax / mở trang | 401 / chuyển về Login có `ReturnUrl` – **Đạt** |
| TC-06 | FR-08 | Giáo vụ, Sinh viên gọi `/TaiKhoan/Data` | 403 – **Đạt** |
| TC-07 | FR-09 | Giáo vụ CNTT mở bộ lọc thống kê | Chỉ thấy khoa CNTT – **Đạt** |
| TC-08 | FR-01 | Danh sách | Chỉ tài khoản Admin/Giáo vụ (7), không có sinh viên – **Đạt** |
| TC-09 | FR-02 | Giáo vụ thiếu khoa; mật khẩu không có số; trùng tên đăng nhập | 400 với thông báo tương ứng – **Đạt** |
| TC-10 | FR-02 | Tạo `gv.test` hợp lệ rồi đăng nhập | 200; đăng nhập thành công – **Đạt** |
| TC-11 | FR-05 | Khóa thiếu lý do | 400 "Vui lòng nhập lý do" – **Đạt** |
| TC-12 | FR-05, FR-08 | Khóa `gv.test` **khi đang đăng nhập** | Request kế tiếp của `gv.test` nhận 401; đăng nhập lại bị từ chối – **Đạt** |
| TC-13 | FR-05 | Khóa Admin hoạt động cuối cùng | 400 "Không thể khóa Admin hoạt động cuối cùng…" – **Đạt** |
| TC-14 | FR-03 | Đổi vai trò Admin cuối cùng thành Giáo vụ | 400 – **Đạt** |
| TC-15 | FR-03, FR-09 | Đổi khoa `gv.test` khi đang đăng nhập | Phiên cũ nhận 401 (áp dụng từ yêu cầu kế tiếp) – **Đạt** |
| TC-16 | FR-04 | Cấp lại mật khẩu, đăng nhập bằng mật khẩu mới | Thành công – **Đạt** |
| TC-17 | CSRF | POST không có token | 400 – **Đạt** |
| TC-18 | Phân trang | 7 tài khoản, 3 dòng/trang, trang 1/2/3 | `admin, gv.cntt, gv.kt` / `gv.luat, gv.nna, gv.qtkd` / `gv.test`; không trùng, không sót – **Đạt** |
| TC-19 | Phân trang | `page=99`; `pageSize=1000` | Kẹp về trang 3/3; `pageSize` về 10 – **Đạt** |
| TC-20 | Tìm kiếm | `hung`, `do`, `van`, `hông`, `GV.CN` | Hưng (và Nhung), Đỗ Thùy Linh, Lê Văn Phúc, Hồng Nhung, gv.cntt – **Đạt** |
| TC-21 | Tìm kiếm | Từ khóa > 100 ký tự; ký tự `%_[` | 400; không lỗi, 0 kết quả (EF Core escape) – **Đạt** |
| TC-22 | FR-11 | Sinh viên mở `/HoSo`; thêm `?maSV=22010005` | Chỉ hiện hồ sơ của mình, tham số bị bỏ qua – **Đạt** |
| TC-23 | FR-11 | Giáo vụ mở `/HoSo` | Chỉ thông tin tài khoản (vai trò, khoa phụ trách) – **Đạt** |
| TC-24 | FR-12 | Cập nhật SĐT/Email/Địa chỉ hợp lệ | 200, ghi nhật ký `CapNhatLienHe` – **Đạt** |
| TC-25 | FR-12 | SĐT `12345`; Email trùng sinh viên khác | 400 với thông báo tương ứng – **Đạt** |
| TC-26 | FR-12 | Gửi kèm `hoTen`, `maSV` | 400 "…chứa trường không được phép sửa" – **Đạt** |
| TC-27 | FR-12 | Giáo vụ gọi `/HoSo/CapNhatLienHe` | 403 – **Đạt** |
| TC-28 | FR-10 | Sai mật khẩu hiện tại; mới trùng cũ; mới không có số; xác nhận không khớp | 400 với thông báo tương ứng – **Đạt** |
| TC-29 | FR-10 | Đổi mật khẩu hợp lệ | Phiên hiện tại vẫn dùng được; **phiên khác** của cùng tài khoản bị đăng xuất; mật khẩu cũ không đăng nhập được – **Đạt** |
| TC-30 | FR-10 | Admin và Giáo vụ đổi mật khẩu | 200 – **Đạt** |
| TC-31 | Giao diện | Modal, nút, debounce trên trình duyệt; responsive | **Chưa kiểm thử** trên trình duyệt |

---

## IV. HẠN CHẾ VÀ PHỤ THUỘC

- **Lý do khóa/mở khóa** bắt buộc nhập nhưng chưa được lưu vì `NHAT_KY_THAO_TAC` không có cột lý do; cần bổ sung cột hoặc bảng lịch sử tài khoản (migration).
- Tên đăng nhập Admin/Giáo vụ theo quy tắc 3–50 ký tự (chữ, số, `.`, `_`) cho khớp dữ liệu seed (`admin`, `gv.cntt`); quy tắc 6–15 ký tự của SRS áp dụng cho mã sinh viên.
- Tên vai trò dùng `GiaoVu` (theo seed) thay cho `CanBo` trong SRS; mã trạng thái tài khoản theo seed (1 = Hoạt động, 0 = Khóa), ngược với `Models/Enums.cs`.
- Kiểm tra SecurityStamp ở mọi request thêm một truy vấn tài khoản mỗi request – chấp nhận được với quy mô bài tập.
- `SinhVienAjaxController` (module khác) chưa có `[Authorize]`.
