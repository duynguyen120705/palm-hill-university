# BÁO CÁO THỰC HÀNH 7 – TÍCH HỢP AJAX

**Module:** Thống kê, Xuất/Nhập dữ liệu (FR-27 – FR-30)
**Thành viên thực hiện:** Trần Văn Dũng

---

## I. ÁP DỤNG VÀO MODULE ĐƯỢC PHÂN CÔNG

| Yêu cầu | Chức năng áp dụng | Công cụ phía client | Endpoint |
| --- | --- | --- | --- |
| 1. UI Update | Bảng thống kê sinh viên (FR-27) | `XMLHttpRequest` | `GET /ThongKe/Data`, `GET /ThongKe/BoLoc` |
| 2. CRUD AJAX | Create: nhập Excel tạo phiên nhập (FR-30)<br>Delete: xóa phiên nhập | jQuery `$.ajax` | `POST /XuatNhap/Import`<br>`DELETE /XuatNhap/XoaPhienNhap/{id}` |
| 3. Pagination | Lịch sử nhập; danh sách sinh viên cần xuất (FR-28) | jQuery `$.ajax` / `fetch()` | `GET /XuatNhap/DanhSachPhienNhap`<br>`GET /XuatNhap/DanhSachData` |
| 4. AJAX Search | Tìm sinh viên trước khi xuất (FR-28) và tải file xuất (FR-28, FR-29) | `fetch()` | `GET /XuatNhap/DanhSachData?keyword=`<br>`GET /XuatNhap/XuatDanhSach`, `GET /ThongKe/Xuat` |

### Cấu trúc mã nguồn

| Tầng | Tệp |
| --- | --- |
| View | `Views/ThongKe/Index.cshtml`, `Views/XuatNhap/Index.cshtml`, `Views/XuatNhap/DanhSach.cshtml` |
| JavaScript | `wwwroot/js/thongke.js` (XHR), `xuatnhap.js` (jQuery), `danhsach.js` (fetch), `taifile.js` (fetch tải file), `phantrang.js` (thanh phân trang dùng chung) |
| Controller | `Controllers/ThongKeController.cs`, `Controllers/XuatNhapController.cs` |
| Service | `ThongKeService`, `XuatNhapService`, `NhatKyService`, `PhamViDuLieuServiceTam` (bản tạm FR-09), `PhanTrangExtensions`, `BaoCaoFileBuilder` |
| Model / DTO | `Models/PhienNhap.cs`, `Models/DTOs/ThongKeDtos.cs`, `Models/DTOs/XuatNhapDtos.cs`; migration `ThemPhienNhap` |

### Nguyên tắc chung đã áp dụng

- **Phân quyền tại server:** mọi Controller gắn `[Authorize(Roles = "Admin,GiaoVu")]`; Service luôn áp bộ lọc phạm vi (FR-09) **trước** khi lọc, tìm kiếm, phân trang hoặc xuất – Giáo vụ chỉ thấy khoa phụ trách.
- **Chống CSRF:** thao tác thay đổi dữ liệu (`POST Import`, `DELETE XoaPhienNhap`) có `[ValidateAntiForgeryToken]`; JS gửi token qua header `RequestVerificationToken`.
- **Chống XSS:** dữ liệu từ JSON được đưa vào DOM bằng `textContent` / jQuery `.text()`, không ghép chuỗi vào `innerHTML`.
- **JSON thống nhất:** `{ success, message, data }`; dữ liệu phân trang dùng chung `PagedResultDto<T>` của nhóm (`items, pageIndex, pageSize, totalItems, totalPages`).
- **Không lộ lỗi nội bộ:** `catch` ghi log server, client chỉ nhận thông báo chung.
- **Nhật ký (NFR-07):** ghi import, tạo hồ sơ/tài khoản, chuyển lớp, đổi trạng thái, khóa tài khoản, xóa phiên nhập, mỗi lần xuất file; không ghi mật khẩu, dữ liệu cá nhân hoặc từ khóa tìm kiếm.
- **File tạm (NFR-08):** file import chỉ đọc trong bộ nhớ, file xuất tạo trong bộ nhớ và trả thẳng về trình duyệt – không lưu trên server.

---

## II. GIẢI THÍCH VÀ PHÂN TÍCH LUỒNG

Luồng chung của mọi request AJAX trong module:

```
UI ──sự kiện──► JS ──HTTP request──► Controller ──gọi──► Service ──LINQ──► EF Core ──SQL──► DB
                                                                                            │
DOM ◄──render── JS ◄──JSON response── Controller ◄──DTO── Service ◄──entity── EF Core ◄─────┘
```

Các bước giống nhau ở cả ba công cụ là phía server (Controller → EF Core → DB → JSON). Điểm khác nhau nằm ở **cách JS gửi request, nhận và xử lý phản hồi**.

### 1. XMLHttpRequest – Bảng thống kê (FR-27)

| Bước | Thành phần | Diễn giải |
| --- | --- | --- |
| UI | `Views/ThongKe/Index.cshtml` | Người dùng đổi Khoa/Khóa/Lớp/Trạng thái hoặc bấm *Thống kê*. |
| JS | `thongke.js` → `loadThongKe()` → `xhrGetJson()` | Gom tiêu chí bằng `URLSearchParams(new FormData(form))`; tạo `new XMLHttpRequest()`, `xhr.open('GET', '/ThongKe/Data?...', true)`, đặt header `Accept: application/json`, `xhr.send()`. |
| Controller | `ThongKeController.Data(ThongKeFilterDto)` | Model binding đọc query string vào DTO; `[Authorize]` đã kiểm tra vai trò; gọi `IThongKeService.ThongKeAsync(filter, User)`. |
| EF Core | `ThongKeService.ThongKeAsync` | Áp phạm vi FR-09 → `Where` theo tiêu chí → `GroupBy(lớp)` → `Select` với `g.Count(s => s.TrangThai == ...)` cho từng trạng thái. |
| DB | SQL Server | EF Core dịch thành **một câu** `SELECT ... COUNT(CASE WHEN ...) ... FROM SINH_VIEN JOIN LOP JOIN KHOA WHERE DaXoa = 0 AND ... GROUP BY ...` – tổng hợp tại CSDL, chỉ trả số liệu từng lớp. |
| JSON | `Json(new { success, data })` | `System.Text.Json` serialize DTO, tên thuộc tính chuyển camelCase: `{ success: true, data: { rows: [...], tongCong: {...} } }`. |
| JS | `xhr.onreadystatechange` | Khi `readyState === DONE`: kiểm tra `xhr.status` (200 / 401 / 403 / 5xx), `JSON.parse(xhr.responseText)`, gọi callback. |
| DOM | `renderTable(data)` | `tbody.replaceChildren()` rồi tạo `<tr>/<td>` bằng `createElement` + `textContent`; thêm dòng tổng cộng vào `<tfoot>`. |

**Đặc điểm:** API gốc của trình duyệt, không cần thư viện; nhưng phải tự quản lý `readyState`, tự `JSON.parse`, tự phân loại lỗi và dùng callback – mã dài nhất trong ba cách.

### 2. jQuery AJAX – Nhập Excel (Create) và xóa phiên nhập (Delete)

**Create – `POST /XuatNhap/Import`**

| Bước | Thành phần | Diễn giải |
| --- | --- | --- |
| UI | `Views/XuatNhap/Index.cshtml` | Chọn file `.xlsx`, bấm *Nhập*. Trang có `@Html.AntiForgeryToken()`. |
| JS | `xuatnhap.js` | Tạo `FormData` chứa file; `$.ajax({ type: 'POST', data: formData, processData: false, contentType: false, headers: { RequestVerificationToken } })`. `processData:false` giữ nguyên `FormData`; `contentType:false` để trình duyệt tự đặt `multipart/form-data; boundary=...`. `beforeSend` khóa nút, hiện spinner. |
| Controller | `XuatNhapController.Import(IFormFile)` | `[ValidateAntiForgeryToken]` kiểm tra token; `[RequestSizeLimit]` chặn file lớn; kiểm tra phần mở rộng `.xlsx` và dung lượng ≤ 5 MB. |
| Service | `XuatNhapService.NhapExcelAsync` | ClosedXML đọc file từ stream trong bộ nhớ; kiểm tra mẫu cột, mã SV trùng trong file, quy tắc mục 3.4, phạm vi lớp. Mỗi dòng mới: `BeginTransaction` → thêm `SinhVien` → `UserManager.CreateAsync` → `AddToRoleAsync("SinhVien")` → ghi nhật ký → `Commit`; lỗi thì `Rollback` riêng dòng đó. Dòng cập nhật áp dụng FR-16, FR-17, FR-20. |
| DB | SQL Server | Mỗi dòng là một giao dịch `INSERT SINH_VIEN`, `INSERT AspNetUsers`, `INSERT AspNetUserRoles`, `INSERT NHAT_KY_THAO_TAC`; cuối cùng `INSERT PHIEN_NHAP`. |
| JSON | `Ok(new { success, message, data })` | `data` gồm `soThemMoi, soCapNhat, soLoi, loi: [{ dong, maSV, loi }]`. Sai mẫu trả `400` kèm `message`. |
| JS | `.done()` / `.fail()` / `.always()` | jQuery tự parse JSON; `.fail` đọc `xhr.responseJSON.message`; `.always` mở lại nút. |
| DOM | `showResult()` | Hiện alert tổng kết, bảng dòng lỗi; tải lại lịch sử nhập trang 1. |

**Delete – `DELETE /XuatNhap/XoaPhienNhap/{id}`**

| Bước | Diễn giải |
| --- | --- |
| UI → JS | Bấm nút thùng rác (gắn bằng event delegation `$body.on('click', '.btn-xoa-phien', ...)` vì các dòng được render động) → `confirm()` → `$.ajax({ type: 'DELETE', headers: { RequestVerificationToken } })`. |
| Controller → Service | `[HttpDelete]` + `[ValidateAntiForgeryToken]` → `XoaPhienNhapAsync`: không tồn tại → 404; Giáo vụ xóa phiên của người khác → 403. |
| EF Core → DB | `Remove(phien)` → `DELETE FROM PHIEN_NHAP WHERE Id = @id`; ghi nhật ký `XoaPhienNhap`. Hồ sơ sinh viên đã nhập **không** bị ảnh hưởng. |
| JSON → DOM | `{ success, message, id }` → `$row.fadeOut()` rồi tải lại trang hiện tại để các bản ghi trang sau dồn lên. |

**Đặc điểm:** cú pháp gọn, tự parse JSON, tự gửi header `X-Requested-With`, có sẵn `beforeSend/done/fail/always`, gửi file chỉ cần hai cờ `processData/contentType`. Đổi lại phụ thuộc thư viện jQuery.

### 3. Phân trang Skip/Take – Lịch sử nhập và danh sách cần xuất

| Bước | Diễn giải |
| --- | --- |
| UI | Bấm số trang trên thanh phân trang (`phantrang.js` tạo nút, gọi lại `onChange(page)`); đổi bộ lọc hoặc số dòng/trang thì quay về trang 1. |
| JS | Gửi `page`, `pageSize` cùng tiêu chí lọc (`$.ajax({ data: {...} })` cho lịch sử nhập, `fetch()` cho danh sách sinh viên). |
| Controller | Nhận `page`, `pageSize` qua query string; trả `PagedResultDto<T>`. |
| EF Core | `PhanTrangExtensions.ToPagedResultAsync`: giới hạn `pageSize ≤ 100` → `CountAsync()` trên query đã lọc → kẹp `page` trong `[1, totalPages]` → `OrderBy(...)` → `Skip((page-1)*pageSize).Take(pageSize)` → `Select(selector)`. |
| DB | Hai câu SQL: `SELECT COUNT(*) ...` và `SELECT ... ORDER BY MaSV OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`. Chỉ đúng số dòng của một trang được đọc khỏi CSDL. |
| JSON → DOM | `{ items, pageIndex, pageSize, totalItems, totalPages }` → render bảng, vẽ lại thanh phân trang, hiện tổng số. |

**Lưu ý kỹ thuật:**
- Phải `OrderBy` trước `Skip/Take` để thứ tự giữa các trang ổn định (không trùng/sót bản ghi).
- Phép chiếu sang DTO (có `ToString("dd/MM/yyyy")` không dịch được sang SQL) đặt **sau** `Skip/Take`, nên EF Core chỉ thực hiện phần định dạng ở client trên đúng số dòng của trang.
- `Count` và dữ liệu trang dùng **cùng một query đã lọc phạm vi**, nên tổng số luôn khớp với dữ liệu người dùng được phép xem.

### 4. fetch() – Tìm kiếm và xuất file

**Tìm kiếm – `GET /XuatNhap/DanhSachData?keyword=...`**

| Bước | Thành phần | Diễn giải |
| --- | --- | --- |
| UI | `Views/XuatNhap/DanhSach.cshtml` | Gõ vào ô tìm kiếm (VD: `nguyen`). |
| JS | `danhsach.js` | Sự kiện `input` + **debounce 300 ms** (chỉ gọi khi ngừng gõ); `controller.abort()` hủy request cũ, tạo `AbortController` mới; `await fetch(url, { signal })`. |
| Controller | `XuatNhapController.DanhSachData(DanhSachFilterDto)` | Bind `keyword` cùng bộ lọc và trang. |
| EF Core | `XuatNhapService.LocDanhSachAsync` | Áp phạm vi → `EF.Functions.Collate(s.HoTen, "Vietnamese_CI_AI").Contains(kw) || s.MaSV.Contains(kw)` → phân trang. |
| DB | SQL Server | `WHERE [HoTen] COLLATE Vietnamese_CI_AI LIKE N'%nguyen%'` – CI: không phân biệt hoa thường, AI: không phân biệt dấu, nên khớp "Nguyễn". |
| JSON | `PagedResultDto<SinhVienItemDto>` | |
| JS | `if (!res.ok) ...` rồi `await res.json()` | `fetch` **không reject** với HTTP 4xx/5xx – phải tự kiểm tra `res.ok`; chỉ reject khi lỗi mạng hoặc bị `abort` (bỏ qua `AbortError`). |
| DOM | `renderRows()` + `renderPhanTrang()` | Cập nhật bảng, phân trang, tổng số không tải lại trang. |

**Xuất file – `GET /XuatNhap/XuatDanhSach` (FR-28), `GET /ThongKe/Xuat` (FR-29)**

| Bước | Diễn giải |
| --- | --- |
| JS | `taifile.js` → `fetch(url)` với đúng tiêu chí đang lọc + `format=excel|pdf`. |
| Service | Truy vấn lại **toàn bộ** kết quả lọc (không phân trang) trong phạm vi; rỗng → trả `null`; tạo file trong bộ nhớ bằng ClosedXML (Excel) / QuestPDF (PDF); ghi nhật ký người xuất, phạm vi, thời điểm. |
| Controller | Rỗng → `400 { message }`; có dữ liệu → `File(bytes, contentType, tenFile)` (header `Content-Disposition: attachment`). |
| JS → DOM | `!res.ok` → đọc `message` JSON và hiện cảnh báo (không tải file hỏng). Thành công → `res.blob()` → `URL.createObjectURL` → tạo thẻ `<a download>` ẩn, `click()`, rồi `revokeObjectURL` giải phóng bộ nhớ. |

**Đặc điểm:** API hiện đại dựa trên Promise, viết tuần tự bằng `async/await`; hỗ trợ hủy request (`AbortController`) và đọc nhiều kiểu phản hồi (`json()`, `blob()`). Cần nhớ tự kiểm tra `res.ok`.

### 5. So sánh ba công cụ

| Tiêu chí | XMLHttpRequest | jQuery `$.ajax` | `fetch()` |
| --- | --- | --- | --- |
| Phụ thuộc | Không (API gốc) | Thư viện jQuery | Không (API gốc) |
| Mô hình bất đồng bộ | Callback, `onreadystatechange` | Callback / Deferred (`done/fail/always`) | Promise, `async/await` |
| Parse JSON | Tự `JSON.parse` | Tự động (`dataType: 'json'`) | `await res.json()` |
| HTTP 4xx/5xx | Tự kiểm tra `xhr.status` | Vào `.fail()` | **Không reject** – tự kiểm tra `res.ok` |
| Gửi file | `FormData` + `send()` | `FormData` + `processData:false, contentType:false` | `FormData` làm `body` |
| Hủy request | `xhr.abort()` | `jqXHR.abort()` | `AbortController` |
| Nhận file nhị phân | `responseType = 'blob'` | Cần cấu hình thêm | `res.blob()` |
| Độ dài mã | Dài nhất | Ngắn | Ngắn, dễ đọc |
| Dùng trong module | Bảng thống kê | Nhập Excel, xóa phiên nhập, phân trang lịch sử | Tìm kiếm, phân trang danh sách, tải file xuất |

**Phía server không thay đổi theo công cụ:** cả ba đều gọi cùng một kiểu endpoint, Controller trả `Json(...)` hoặc `File(...)`, EF Core dịch LINQ sang SQL như nhau. Lựa chọn công cụ chỉ ảnh hưởng tới mã JavaScript.

---

## III. KIỂM THỬ

Dữ liệu: seed mẫu (200 sinh viên, 5 khoa). Tài khoản: `admin` / `Admin@123`; Giáo vụ `gv.cntt` / `GiaoVu@123` (khoa CNTT).

| Mã | Chức năng | Bước thực hiện | Kết quả mong đợi |
| --- | --- | --- | --- |
| TC-01 | FR-27 | Admin mở *Thống kê*, không chọn bộ lọc | Bảng gồm lớp của cả 5 khoa, dòng tổng cộng = tổng số sinh viên chưa xóa mềm |
| TC-02 | FR-27, FR-09 | `gv.cntt` mở *Thống kê* | Chỉ có lớp khoa CNTT; dropdown Khoa chỉ có CNTT |
| TC-03 | FR-27 | Chọn trạng thái không có sinh viên nào trong lớp đã chọn | Bảng trống kèm ghi chú "Không có sinh viên phù hợp..." |
| TC-04 | FR-08 | Đăng nhập sinh viên, gọi trực tiếp `/ThongKe/Data` | Bị từ chối (403 / chuyển trang); JS hiện "Bạn không có quyền..." |
| TC-05 | FR-30 | Tải file mẫu, điền 3 dòng mới hợp lệ, nhập | 3 thêm mới; đăng nhập được bằng Mã SV + mật khẩu ban đầu |
| TC-06 | FR-30 | File đổi tên cột `HoTen` | 400 "File sai mẫu: cột 2 phải là 'HoTen'" |
| TC-07 | FR-30 | Hai dòng cùng Mã SV | Cả hai dòng báo lỗi "Mã SV bị lặp trong file" |
| TC-08 | FR-30 | Dòng có SĐT `12345`, dòng có ngày sinh năm nay | Báo lỗi đúng dòng; các dòng hợp lệ khác vẫn được lưu |
| TC-09 | FR-30, FR-09 | `gv.cntt` nhập dòng có lớp `QTKD-...` | Lỗi "Lớp ... ngoài phạm vi được phép" |
| TC-10 | FR-30 | Mật khẩu ban đầu `abcdefgh` (không có số) | Lỗi dòng về chính sách mật khẩu; không tạo hồ sơ |
| TC-11 | FR-30, FR-16 | Dòng cập nhật đổi Đang học → Thôi học | Cập nhật trạng thái; tài khoản sinh viên bị khóa; nhật ký có `DoiTrangThai`, `KhoaTaiKhoan` |
| TC-12 | FR-30, FR-16 | `gv.cntt` cập nhật Thôi học → Đang học | Lỗi "Chỉ Admin được chuyển Thôi học → Đang học" |
| TC-13 | FR-30, FR-17 | Dòng cập nhật đổi lớp trong cùng khoa | Hồ sơ gắn lớp mới; nhật ký `ChuyenLop` |
| TC-14 | FR-30 | Dòng cập nhật để trống SĐT/Email/Địa chỉ | Giữ nguyên giá trị cũ; mật khẩu không đổi |
| TC-15 | FR-30 | Dòng có Mã SV của hồ sơ đã xóa mềm | Lỗi "Mã SV trùng với hồ sơ đã xóa mềm" |
| TC-16 | Delete | Xóa một phiên nhập | Dòng biến mất, danh sách tải lại; hồ sơ sinh viên còn nguyên |
| TC-17 | Delete | `gv.cntt` gọi xóa phiên nhập của Admin | 403 "Bạn chỉ được xóa phiên nhập của mình" |
| TC-18 | CSRF | Gọi `POST /XuatNhap/Import` không kèm token | 400 Bad Request |
| TC-19 | Phân trang | Danh sách 10 dòng/trang, bấm trang 3 | Hiện sinh viên thứ 21–30 theo Mã SV; tổng số không đổi |
| TC-20 | Phân trang | Đang ở trang cuối, đổi bộ lọc | Quay về trang 1 |
| TC-21 | Tìm kiếm | Gõ `nguyen` | Có kết quả "Nguyễn ..."; request chỉ gửi sau khi ngừng gõ 300 ms |
| TC-22 | Tìm kiếm | Gõ nhanh nhiều ký tự | Các request cũ bị hủy (tab Network: *canceled*), bảng hiện kết quả của từ khóa cuối |
| TC-23 | FR-28 | Lọc lớp `CNTT-K24A`, bấm *Xuất Excel* | File .xlsx có đúng toàn bộ sinh viên lớp đó (không chỉ trang hiện tại) |
| TC-24 | FR-28 | Từ khóa không khớp ai, bấm *Xuất PDF* | Cảnh báo "Không có sinh viên nào để xuất", không tải file |
| TC-25 | FR-29 | Thống kê khoa CNTT, *Xuất PDF* | File PDF khổ ngang, tiếng Việt hiển thị đúng, có dòng tổng cộng |
| TC-26 | NFR-07 | Sau TC-23, TC-25 xem bảng `NHAT_KY_THAO_TAC` | Có `XuatDanhSachExcel`, `XuatThongKePdf` kèm người xuất, phạm vi, thời điểm; không có từ khóa |
| TC-27 | XSS | Nhập sinh viên có họ tên `<b>Test</b>`; nhập file tên `<img src=x onerror=alert(1)>.xlsx` | Họ tên và tên file hiển thị nguyên văn dưới dạng chữ trong bảng, không có hộp alert |
| TC-28 | NFR-02 | Danh sách 1.000 sinh viên mẫu, đo tab Network | Thời gian phản hồi `DanhSachData` < 2 giây |

---

## IV. HẠN CHẾ VÀ PHỤ THUỘC

- Bộ lọc phạm vi đang dùng **bản tạm** `PhamViDuLieuServiceTam`; thay bằng bản chính thức của phân hệ Tài khoản khi hoàn thành. Trang đăng nhập (FR-06) chưa có nên chưa kiểm thử được trên trình duyệt.
- Mã trạng thái theo dữ liệu seed (2 = Thôi học, 3 = Đã tốt nghiệp) đang ngược với `Models/Enums.cs` – nhóm cần thống nhất.
- Import không có cột lý do nên không đổi được trạng thái hồ sơ Đã tốt nghiệp (FR-16 yêu cầu lý do) – dùng chức năng Đổi trạng thái.
- Chuyển lớp/đổi trạng thái qua import mới ghi `NHAT_KY_THAO_TAC`, chưa ghi `LICH_SU_HO_SO_SINH_VIEN` (chờ thống nhất mã `Loai`).
- Đã chỉnh cấu hình Identity trong `Program.cs` theo SRS (mật khẩu ≥ 8 ký tự gồm chữ và số; Email không bắt buộc) – cần phân hệ Tài khoản xác nhận.
