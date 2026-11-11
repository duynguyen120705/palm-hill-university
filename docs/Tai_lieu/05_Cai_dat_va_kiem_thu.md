# 05. MỘT SỐ CHỨC NĂNG CHÍNH VÀ KIỂM THỬ

Ứng dụng quản lý sinh viên Student Portal

Môi trường cài đặt, bốn chức năng tiêu biểu (giao diện, mã HTML, CSS, script client, script server, đánh giá), kết quả kiểm thử chức năng và đối chiếu yêu cầu BTL.

Chương này trình bày bốn chức năng tiêu biểu, mỗi thành viên một chức năng theo phân công ở mục 1.4. Mỗi chức năng gồm giao diện, mã HTML (Razor View), mã CSS, mã script phía client, mã script phía server và đánh giá. Các đoạn mã được trích từ kho mã nguồn của nhóm (nhánh main, bản gộp ngày 09/11/2026); phần lược bỏ được thay bằng dấu "// ...". Kết quả kiểm thử của từng chức năng được tổng hợp ở mục 4.5.

## 4.0. Môi trường cài đặt

| **Thành phần** | **Phiên bản / cấu hình** |
|----|----|
| Nền tảng | .NET 8, ASP.NET Core MVC (TargetFramework net8.0); SDK được cố định trong global.json (8.0.426, rollForward latestFeature) |
| CSDL | SQL Server LocalDB (MSSQLLocalDB), Entity Framework Core 8.0.10, một migration InitialCreate |
| Xác thực | ASP.NET Core Identity 8.0.10, cookie .StudentPortal.Auth (HttpOnly, Secure, hết hạn sau 30 phút không hoạt động) |
| Thư viện | ClosedXML 0.104.2 (Excel), QuestPDF 2024.12.3 – giấy phép Community (PDF), Bogus 35.6.1 (dữ liệu mẫu) |
| Giao diện | Bootstrap 5, Bootstrap Icons, jQuery, jQuery Validation Unobtrusive |
| Dữ liệu mẫu | 1 Admin, 3 Giáo vụ, 4 khoa, 4 khóa (K2022 – K2025), 32 lớp, 1.000 hồ sơ sinh viên (10 hồ sơ xóa mềm); mật khẩu mẫu đọc từ cấu hình Seed:AdminPassword, Seed:DemoPassword, không lưu trong mã nguồn |
| Quản lý mã nguồn | GitHub, nhánh theo chức năng, gộp qua Pull Request; phân tích chất lượng bằng SonarCloud Quality Gate |

## 4.1. Đăng nhập, phân quyền và phạm vi dữ liệu theo khoa – Lê Nam Khánh

### 4.1.1. Giao diện

Trang đăng nhập là trang công khai duy nhất của hệ thống, dùng form Razor gửi về AccountController. Khi đăng nhập sai, hệ thống hiển thị một thông báo chung, không cho biết tên đăng nhập có tồn tại hay tài khoản đang bị khóa. Sau khi đăng nhập, menu bên trái hiển thị theo vai trò. Giáo vụ không thấy mục Quản lý tài khoản và Nhật ký thao tác; tên vai trò và khoa phụ trách hiển thị ở góc phải thanh tiêu đề.

<img src="../images/05_Cai_dat_va_kiem_thu/media/image27.png" width="560" />

Hình 4.1. Trang đăng nhập khi nhập sai mật khẩu

<img src="../images/05_Cai_dat_va_kiem_thu/media/image28.png" width="600" />

Hình 4.2. Trang chủ của Admin với menu đầy đủ chức năng

### 4.1.2. Mã HTML

Form dùng Tag Helper asp-for để kết gán LoginVm, asp-validation-for để hiển thị lỗi từng trường và asp-validation-summary cho lỗi chung. Token anti-forgery được Tag Helper form tự chèn.

**Views/Account/Login.cshtml**

```cshtml
<form asp-action="Login" method="post" novalidate>
    <input type="hidden" asp-for="ReturnUrl" />
    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2" role="alert"></div>
    <div class="mb-3">
        <label asp-for="UserName" class="form-label"></label>
        <input asp-for="UserName" class="form-control" autocomplete="username" />
        <span asp-validation-for="UserName" class="text-danger small"></span>
    </div>
    <div class="mb-3">
        <label asp-for="MatKhau" class="form-label"></label>
        <input asp-for="MatKhau" class="form-control" autocomplete="current-password" />
        <span asp-validation-for="MatKhau" class="text-danger small"></span>
    </div>
    <div class="d-grid">
        <button type="submit" class="btn btn-primary">Đăng nhập</button>
    </div>
</form>
```

### 4.1.3. Mã CSS

Bố cục chung (header, menu trái, vùng nội dung) dùng lưới Bootstrap và một số lớp riêng trong site.css. Trên màn hình từ 768 px, menu có độ rộng cố định 15rem; trên điện thoại, menu thu vào nút ☰ ở thanh tiêu đề.

**wwwroot/css/site.css**

```css
/* Layout chung: menu bên trái, vùng nội dung (mục 3.2.3) */
.app-noi-dung {
  min-width: 0;
  padding-bottom: 6rem !important;
}
@media (min-width: 768px) {
  .app-menu {
    flex: 0 0 15rem;
    min-height: calc(100vh - 57px);
  }
}
```

### 4.1.4. Mã ClientSide Script

Form đăng nhập dùng jQuery Validation Unobtrusive để kiểm tra trường bắt buộc ngay trên trình duyệt. Mọi lời gọi Ajax trong hệ thống đi qua hàm dùng chung trong site.js. Hàm này gửi kèm token anti-forgery qua header RequestVerificationToken; khi máy chủ trả 401 (hết phiên), hàm chuyển người dùng về trang đăng nhập và giữ lại địa chỉ đang xem.

**wwwroot/js/site.js**

```javascript
function veTrangDangNhap() {
    const returnUrl = encodeURIComponent(location.pathname + location.search);
    location.href = '/Account/Login?returnUrl=' + returnUrl;
}

function goi(options) {
    return new Promise(function (resolve, reject) {
        $.ajax(Object.assign({
            dataType: 'json',
            headers: {
                'RequestVerificationToken': antiForgeryToken(),
                'X-Requested-With': 'XMLHttpRequest'
            }
        }, options)).done(function (res) {
            resolve(res);
        }).fail(function (xhr) {
            if (xhr.status === 401) veTrangDangNhap();
            reject(taoLoi(xhr));
        });
    });
}
```

### 4.1.5. Mã ServerSide Script

Chính sách mật khẩu, khóa tạm và cookie được cấu hình trong Program.cs. ValidationInterval = 0 buộc Identity kiểm tra lại SecurityStamp ở mọi yêu cầu, nên tài khoản bị khóa hoặc bị đổi vai trò, khoa phụ trách sẽ chịu tác dụng ngay từ yêu cầu kế tiếp.

**Program.cs**

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;          // + LetterPasswordValidator: bắt buộc có chữ
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager<AppSignInManager>()
.AddPasswordValidator<LetterPasswordValidator>()
.AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    o.SlidingExpiration = true;
});
```

Action Login bật lockoutOnFailure và trả về cùng một thông báo cho mọi trường hợp sai. Tài khoản ở trạng thái BiKhoa bị AppSignInManager từ chối trước bước kiểm tra mật khẩu, nên action tự tăng số lần sai để hành vi giống tài khoản thường.

**Controllers/AccountController.cs**

```csharp
[HttpPost, AllowAnonymous]
public async Task<IActionResult> Login(LoginVm dto)
{
    if (!ModelState.IsValid) return View(dto);
    var result = await _signInManager.PasswordSignInAsync(dto.UserName.Trim(), dto.MatKhau,
        isPersistent: false, lockoutOnFailure: true);
    if (!result.Succeeded)
    {
        if (result.IsNotAllowed)                      // tài khoản BiKhoa
        {
            var biKhoa = await _userManager.FindByNameAsync(dto.UserName.Trim());
            if (biKhoa != null) await _userManager.AccessFailedAsync(biKhoa);
        }
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? LoiKhoaTam : LoiDangNhap);
        dto.MatKhau = string.Empty;
        return View(dto);
    }
    // ... ghi LanDangNhapCuoi, chuyển về returnUrl nội bộ hoặc trang chủ
}
```

Phạm vi dữ liệu (FR-09) được áp dụng tập trung trong ScopeService. Mọi truy vấn sinh viên, kể cả xuất file và nhập Excel, đều đi qua ApplyStudentScope trước khi lọc và phân trang. Admin thấy toàn bộ dữ liệu, Giáo vụ chỉ thấy khoa phụ trách, Sinh viên chỉ thấy hồ sơ của mình.

**Services/ScopeService.cs**

```csharp
public IQueryable<SinhVien> ApplyStudentScope(IQueryable<SinhVien> query)
{
    var scope = GetCurrent();
    if (scope.IsAdmin) return query;
    if (scope.IsCanBo)
    {
        var maKhoa = scope.MaKhoaPhuTrach;
        return query.Where(s => maKhoa != null && s.Lop!.MaKhoa == maKhoa);
    }
    if (scope.IsSinhVien)
    {
        var maSV = scope.MaSV;
        return query.Where(s => maSV != null && s.MaSV == maSV);
    }
    return query.Where(s => false);
}
```

### 4.1.6. Đánh giá

Các ca TC01 – TC12, TC16, TC17, TC33 ở mục 4.5 đều đạt. Cụ thể:

- Sai mật khẩu 5 lần liên tiếp thì tài khoản bị khóa tạm.
- Giáo vụ mở trang Quản lý tài khoản bị chuyển đến trang 403.
- Giáo vụ khoa CNTT gọi trực tiếp API xem hồ sơ sinh viên khoa QTKD nhận HTTP 403, và tìm theo khoa QTKD trả về 0 kết quả.
- Admin không thể khóa tài khoản Admin hoạt động cuối cùng.
- Admin đổi khoa phụ trách của một Giáo vụ từ CNTT sang QTKD; ngay ở yêu cầu kế tiếp, Giáo vụ đó chỉ còn thấy sinh viên khoa QTKD.

Kiểm tra phân quyền đặt ở cả thuộc tính \[Authorize(Roles)\] và tầng dịch vụ, nên gọi API trực tiếp, bỏ qua giao diện, vẫn bị chặn.

## 4.2. Thêm hồ sơ sinh viên kèm tài khoản – Từ Đức Anh

### 4.2.1. Giao diện

Chức năng thêm hồ sơ mở bằng nút "Thêm SV" trên trang danh sách và hiển thị trong modal theo phiếu BM01. Dropdown Lớp phụ thuộc vào Khoa và Khóa đang chọn, và chỉ chứa các lớp trong phạm vi của người dùng. Lỗi nhập liệu hiển thị ngay dưới từng trường trước khi gửi. Khi lưu thành công, modal đóng, danh sách tải lại bằng Ajax và một thông báo hiện ở góc phải.

<img src="../images/05_Cai_dat_va_kiem_thu/media/image29.png" width="430" />

Hình 4.3. Modal thêm hồ sơ hiển thị lỗi kiểm tra phía client

<img src="../images/05_Cai_dat_va_kiem_thu/media/image30.png" width="430" />

Hình 4.4. Modal thêm hồ sơ với dữ liệu hợp lệ

<img src="../images/05_Cai_dat_va_kiem_thu/media/image31.png" width="600" />

Hình 4.5. Trang chi tiết hồ sơ vừa tạo, các nút thao tác hiển thị theo vai trò và trạng thái

### 4.2.2. Mã HTML

Modal là partial view \_StudentCreateModal nhận StudentCreateVm. Tag Helper asp-for sinh các thuộc tính data-val-\* từ Data Annotations, nhờ đó quy tắc kiểm tra phía client và phía server dùng chung một nguồn.

**Views/Students/_StudentCreateModal.cshtml**

```cshtml
<form id="formThemSV" class="modal-content" novalidate>
  <div class="modal-body">
    <div class="alert alert-danger d-none loi-modal" role="alert"></div>
    <div class="row g-3">
      <div class="col-12 col-md-6">
        <label asp-for="MaSV" class="form-label"></label>
        <input asp-for="MaSV" class="form-control" maxlength="15" />
        <span asp-validation-for="MaSV" class="text-danger small"></span>
      </div>
      <!-- ... Họ tên, Ngày sinh, Giới tính -->
      <div class="col-12 col-md-4">
        <label for="csKhoa" class="form-label">Khoa</label>
        <select id="csKhoa" class="form-select"><option value="">Tất cả</option></select>
      </div>
      <!-- ... Khóa -->
      <div class="col-12 col-md-4">
        <label asp-for="MaLop" class="form-label"></label>
        <select asp-for="MaLop" class="form-select"><option value="">-- Chọn lớp --</option></select>
        <span asp-validation-for="MaLop" class="text-danger small"></span>
      </div>
      <!-- ... Trạng thái, SĐT, Email, Địa chỉ, Mật khẩu ban đầu -->
    </div>
  </div>
</form>
```

### 4.2.3. Mã CSS

Modal dùng lưới Bootstrap: hai cột từ 768 px trở lên và một cột trên điện thoại. Trên điện thoại, nút thao tác chính (Thêm SV) được cố định ở góc dưới màn hình để luôn trong tầm tay (Hình 4.8 ở mục 4.3).

**wwwroot/css/site.css**

```css
/* Mobile: nút thao tác chính cố định dưới màn hình (Hình 3.7) */
@media (max-width: 767.98px) {
  .app-nut-chinh {
    position: fixed;
    right: 1rem;
    bottom: 1rem;
    z-index: 1020;
  }
}
```

### 4.2.4. Mã ClientSide Script

Validator tùy biến NgaySinhAttribute được đăng ký phía client bằng một adapter trong site.js. Khi gửi form, script kiểm tra hợp lệ, khóa nút Lưu để tránh gửi lặp và gửi JSON qua App.post. Lỗi trùng mã (HTTP 409) được gắn vào đúng trường Mã sinh viên; lỗi 400 được gắn theo từng trường mà server trả về.

**wwwroot/js/site.js**

```javascript
$.validator.addMethod('ngaysinh', function (value, element, minAge) {
    if (this.optional(element)) return true;
    const ngay = new Date(value);
    const moc = new Date();
    moc.setFullYear(moc.getFullYear() - parseInt(minAge, 10));
    return !isNaN(ngay) && ngay <= moc;
});
$.validator.unobtrusive.adapters.addSingleVal('ngaysinh', 'minage');
```

**wwwroot/js/pages/students.js**

```javascript
$formThem.on('submit', function (e) {
    e.preventDefault();
    if (!$formThem.valid()) return;
    const $btn = $formThem.find('button[type="submit"]').prop('disabled', true);
    App.post('/Students/Create', {
        maSV: giaTri('MaSV'), hoTen: giaTri('HoTen'), ngaySinh: giaTri('NgaySinh'),
        gioiTinh: parseInt($('#GioiTinh').val(), 10) || null, maLop: csLop.value,
        trangThai: parseInt($('#TrangThai').val(), 10),
        sdt: giaTri('SDT'), email: giaTri('Email'), diaChi: giaTri('DiaChi'),
        matKhau: $('#MatKhau').val()
    }).then(function (res) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalThemSV')).hide();
        App.toast(res.message);
        loadPage(1);
    }).catch(function (err) {
        const errors = err.status === 409 ? { maSV: [err.message] } : err.errors;
        if (!errors || !App.showFieldErrors($formThem, errors)) {
            $formThem.find('.loi-modal').removeClass('d-none').text(App.errorText(err));
        }
    }).finally(function () { $btn.prop('disabled', false); });
});
```

### 4.2.5. Mã ServerSide Script

Controller chỉ nhận dữ liệu và gọi dịch vụ; ModelState sai được bộ lọc AjaxModelStateFilter chuyển thành JSON 400. Quy tắc nghiệp vụ nằm trong StudentService để chức năng nhập Excel (FR-30) dùng lại:

- Mã sinh viên duy nhất, kể cả so với hồ sơ đã xóa mềm.
- Lớp phải nằm trong phạm vi của người thêm.
- Hồ sơ, tài khoản và nhật ký được ghi trong cùng một giao dịch: tạo tài khoản lỗi thì hồ sơ cũng không được lưu.

**Controllers/StudentsController.cs**

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] StudentCreateVm dto)
{
    await _studentService.CreateAsync(dto);
    return Ok(ApiResponse.Ok(message: $"Đã thêm sinh viên '{dto.MaSV.Trim().ToUpperInvariant()}'."));
}
```

**Services/StudentService.cs**

```csharp
public async Task CreateAsync(StudentCreateVm dto)
{
    var maSV = dto.MaSV.Trim().ToUpperInvariant();
    await KiemTraLopAsync(dto.MaLop);                     // lớp đang dùng, trong phạm vi
    if (await _students.QueryIncludingDeleted().AnyAsync(s => s.MaSV == maSV))
        throw new NghiepVuException("Mã sinh viên đã tồn tại", StatusCodes.Status409Conflict);

    await using var tx = await _context.Database.BeginTransactionAsync();
    _students.Add(new SinhVien { MaSV = maSV, HoTen = dto.HoTen.Trim(),
        HoTenKhongDau = TextHelper.BoDau(dto.HoTen), /* ... */ TrangThai = dto.TrangThai });
    await _auditLog.LogAsync("TaoHoSo", LoaiSinhVien, maSV);
    await _students.SaveChangesAsync();

    var taoTaiKhoan = await _account.CreateStudentAccountAsync(maSV, dto.MatKhau);
    if (!taoTaiKhoan.Succeeded)                           // chưa commit: hồ sơ bị hủy theo
        throw new NghiepVuException("Không tạo được tài khoản sinh viên, hồ sơ chưa được lưu.");
    if (dto.TrangThai == TrangThaiHocTap.ThoiHoc)          // thống nhất với FR-16
        (await _account.SetLockAsync(maSV, true, "Hồ sơ tạo ở trạng thái Thôi học")).ThrowIfFailed();
    await tx.CommitAsync();
}
```

### 4.2.6. Đánh giá

Các ca TC13 – TC15 và TC18 – TC22 đều đạt: thêm hồ sơ hợp lệ, chặn trùng mã (HTTP 409), chặn dữ liệu sai gửi thẳng lên server (HTTP 400 kèm lỗi từng trường), cập nhật, đổi trạng thái theo bảng chuyển trạng thái, chuyển lớp, cấp lại mật khẩu, khóa tài khoản và xóa mềm. Sinh viên vừa tạo đăng nhập được ngay bằng mã sinh viên và mật khẩu ban đầu (TC16).

Ca TC15b chưa đạt: khi hai yêu cầu thêm cùng một mã mới đến máy chủ gần như đồng thời, cả hai cùng qua bước kiểm tra trùng, yêu cầu đến sau vi phạm khóa chính và nhận HTTP 500 thay vì 409. Dữ liệu vẫn đúng (chỉ một hồ sơ được lưu), và giao diện đã khóa nút Lưu trong lúc gửi nên người dùng thao tác bình thường khó gặp lỗi này. Cách khắc phục được nêu ở mục 5.3.

## 4.3. Danh sách và tìm kiếm sinh viên không dấu bằng Ajax – Nguyễn Hoàng Quốc Duy

### 4.3.1. Giao diện

Trang danh sách gồm ô tìm kiếm, bốn bộ lọc (Khoa, Khóa, Lớp, Trạng thái), bảng kết quả và phân trang. Kết quả cập nhật sau khi người dùng ngừng gõ 300 ms mà không tải lại trang. Từ khóa không cần dấu: gõ "nguyen" sẽ tìm ra cả "Nguyễn" và "Nguyên". Tiêu chí lọc được ghi lên URL nên khi quay lại trang, bộ lọc vẫn được giữ.

<img src="../images/05_Cai_dat_va_kiem_thu/media/image32.png" width="600" />

Hình 4.6. Tìm kiếm không dấu với từ khóa "nguyen"

<img src="../images/05_Cai_dat_va_kiem_thu/media/image33.png" width="600" />

Hình 4.7. Danh sách lọc theo Khóa K2023 và lớp CNTT23A

<img src="../images/05_Cai_dat_va_kiem_thu/media/image34.png" width="250" />

Hình 4.8. Danh sách sinh viên trên màn hình điện thoại (390 px)

### 4.3.2. Mã HTML

Bảng chỉ có phần đầu cố định; phần thân (tbody) do script dựng từ JSON. Thanh phân trang là partial dùng chung \_Pagination.

**Views/Students/Index.cshtml**

```cshtml
<form id="formDanhSach" class="row g-2 align-items-end">
    <div class="col-12">
        <label for="txtKeyword" class="form-label">Tìm kiếm</label>
        <input type="search" id="txtKeyword" name="keyword" class="form-control" maxlength="100"
               placeholder="Nhập tên hoặc mã sinh viên (không cần dấu, VD: nguyen)" autocomplete="off" />
    </div>
    <div class="col-12 col-md-3">
        <label for="selKhoa" class="form-label">Khoa</label>
        <select id="selKhoa" name="maKhoa" class="form-select"><option value="">Tất cả</option></select>
    </div>
    <!-- ... Khóa, Lớp, Trạng thái -->
</form>
<div class="table-responsive">
    <table class="table table-hover align-middle mb-0">
        <thead class="table-light"><tr><th>Mã SV</th><th>Họ tên</th> ... </tr></thead>
        <tbody id="dsBody"></tbody>
    </table>
</div>
<nav class="card-footer ..." aria-label="Phân trang danh sách">
    <span id="dsTotal" class="fw-semibold"></span>
    <partial name="_Pagination" model='"dsPaging"' />
</nav>
```

### 4.3.3. Mã CSS

Bảng nằm trong khung .table-responsive của Bootstrap. Trên màn hình hẹp, chỉ riêng bảng cuộn ngang, cả trang không bị cuộn ngang (TC32). Các bộ lọc dùng col-12 col-md-3 để xếp dọc trên điện thoại và nằm trên một hàng trên máy tính. Cỡ chữ gốc được giảm trên màn hình nhỏ:

**wwwroot/css/site.css**

```css
html { font-size: 14px; }
@media (min-width: 768px) {
  html { font-size: 16px; }
}
```

### 4.3.4. Mã ClientSide Script

Mỗi lần gõ, script đặt lại bộ đếm 300 ms (debounce). Biến requestSeq bảo đảm chỉ phản hồi của yêu cầu mới nhất được hiển thị, tránh trường hợp phản hồi cũ về sau ghi đè kết quả mới. Dữ liệu được đưa vào bảng bằng textContent nên nội dung từ CSDL không thể chèn mã HTML (chống XSS).

**wwwroot/js/pages/students.js**

```javascript
function loadPage(page) {
    const seq = ++requestSeq;
    const params = filterParams();
    const query = params.toString();
    history.replaceState(null, '', location.pathname + (query ? '?' + query : ''));
    params.set('page', page || 1);
    status.textContent = 'Đang tải...';
    App.get('/Students/Search?' + params.toString()).then(function (res) {
        if (seq !== requestSeq) return;                 // bỏ phản hồi cũ
        const d = res.data;
        renderRows(d.items);
        App.renderPaging(document.getElementById('dsPaging'), d.page, d.totalPages, loadPage);
        total.textContent = 'Tổng: ' + d.totalItems + ' sinh viên';
    });
}
txtKeyword.addEventListener('input', function () {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () { loadPage(1); }, 300);
});
```

### 4.3.5. Mã ServerSide Script

Cột HoTenKhongDau được tính sẵn khi lưu hồ sơ (bỏ dấu, chữ thường, đ → d). Từ khóa được chuẩn hóa cùng cách rồi so khớp, nên tìm kiếm không phụ thuộc collation của CSDL. Lọc, sắp xếp và phân trang đều thực hiện tại CSDL bằng Skip/Take; pageSize mặc định 20, tối đa 100.

**Services/StudentQueryService.cs**

```csharp
public IQueryable<SinhVien> BuildQuery(StudentFilter filter)
{
    var query = _scope.ApplyStudentScope(_sinhViens.QueryNoTracking());  // FR-09
    if (!string.IsNullOrWhiteSpace(filter.Keyword))
    {
        var kwKhongDau = TextHelper.BoDau(filter.Keyword);
        query = query.Where(s => s.HoTenKhongDau.Contains(kwKhongDau) || s.MaSV.Contains(kwKhongDau));
    }
    if (!string.IsNullOrWhiteSpace(filter.MaKhoa)) query = query.Where(s => s.Lop!.MaKhoa == filter.MaKhoa);
    if (!string.IsNullOrWhiteSpace(filter.MaLop))  query = query.Where(s => s.MaLop == filter.MaLop);
    if (filter.TrangThai.HasValue)                 query = query.Where(s => s.TrangThai == filter.TrangThai.Value);
    return query;
}
```

**Infrastructure/TextHelper.cs**

```csharp
public static string BoDau(string? s)
{
    if (string.IsNullOrWhiteSpace(s)) return "";
    var chuan = s.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();
    foreach (var c in chuan)
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
    return Regex.Replace(sb.ToString(), @"\s+", " ", RegexOptions.None, RegexTimeout);
}
```

### 4.3.6. Đánh giá

Các ca TC23, TC24 và TC32 đều đạt. Với dữ liệu mẫu khoảng 1.000 sinh viên trên máy phát triển, thời gian phản hồi API /Students/Search đo từ trình duyệt (trung vị 5 lần gọi) là 7 ms cho trang đầu, 6 ms khi tìm "nguyen" và 4 ms cho trang 40, thấp hơn nhiều so với ngưỡng 2 giây của NFR-02.

## 4.4. Nhập danh sách từ Excel và xuất báo cáo – Trần Văn Dũng

### 4.4.1. Giao diện

Trang Nhập Excel cho tải file mẫu BM02, chọn file .xlsx (tối đa 5 MB) và hiển thị thanh tiến trình trong lúc xử lý. Kết quả hiển thị ngay trên trang theo mẫu BM05: bốn ô tổng hợp và bảng các dòng lỗi (dòng, mã sinh viên, cột, nội dung lỗi). Các dòng hợp lệ vẫn được lưu; danh sách lỗi tải được dưới dạng file Excel để sửa và nhập lại. Trang Thống kê nhóm số liệu theo Khoa, Khóa hoặc Lớp, có dòng tổng cộng, và xuất được ra Excel hoặc PDF theo mẫu BM04.

<img src="../images/05_Cai_dat_va_kiem_thu/media/image35.png" width="600" />

Hình 4.9. Kết quả nhập file Excel có 3 dòng lỗi

<img src="../images/05_Cai_dat_va_kiem_thu/media/image36.png" width="600" />

Hình 4.10. Thống kê sinh viên theo lớp của khóa K2023

### 4.4.2. Mã HTML

**Views/Import/Index.cshtml**

```cshtml
<form id="formImport" class="row g-2 align-items-end" enctype="multipart/form-data">
    <div class="col-12 col-md-7">
        <label for="fileExcel" class="form-label">File Excel (.xlsx, tối đa 5 MB)</label>
        <input type="file" id="fileExcel" name="file" class="form-control" accept=".xlsx" required />
    </div>
    <div class="col-6 col-md-2 d-grid">
        <button type="submit" id="btnImport" class="btn btn-primary">Tải lên</button>
    </div>
    <div class="col-6 col-md-3 d-grid">
        <a class="btn btn-outline-secondary" asp-action="Template">Tải file mẫu</a>
    </div>
</form>
<div id="importResult" class="card shadow-sm d-none mb-3">
    <!-- tiêu đề BM05, 4 ô #soTong #soThem #soSua #soLoi, bảng tbody#importErrors -->
</div>
```

### 4.4.3. Mã CSS

Bảng lỗi đặt trong khung có chiều cao tối đa 320 px và cuộn dọc, để file nhiều dòng lỗi không đẩy các nút thao tác ra khỏi màn hình. Ô "Lỗi" có viền đỏ dày để nổi bật (lớp Bootstrap border-2 border-danger).

**Views/Import/Index.cshtml**

```cshtml
<div class="table-responsive" style="max-height: 320px;">
    <table class="table table-sm table-bordered mb-3"> ... </table>
</div>
<div class="border border-2 border-danger rounded p-2">Lỗi<div class="fs-3 fw-bold" id="soLoi"></div></div>
```

### 4.4.4. Mã ClientSide Script

Script kiểm tra kích thước file trước khi gửi, gửi file bằng FormData qua App.upload, rồi dựng bảng kết quả bằng .text() để tránh chèn mã HTML từ nội dung file.

**wwwroot/js/pages/import.js**

```javascript
$('#formImport').on('submit', function (e) {
    e.preventDefault();
    const file = $('#fileExcel')[0].files[0];
    if (!file) return showError('Vui lòng chọn file Excel.');
    if (file.size > KICH_THUOC_TOI_DA) return showError('File vượt quá 5 MB.');
    const formData = new FormData();
    formData.append('file', file);
    $('#btnImport').prop('disabled', true);
    $('#importProgress').removeClass('d-none');
    App.upload('/Import/Upload', formData).then(function (res) {
        showResult(res.data);                       // BM05
    }).catch(function (err) {
        showError(err.message);
    }).finally(function () {
        $('#btnImport').prop('disabled', false);
        $('#importProgress').addClass('d-none');
    });
});
```

### 4.4.5. Mã ServerSide Script

Controller kiểm tra lại đuôi file và kích thước ở phía server. File chỉ được đọc trong bộ nhớ bằng ClosedXML, không ghi xuống đĩa (NFR-08). Mỗi dòng được xử lý độc lập qua cùng các quy tắc của StudentService: dòng lỗi được ghi vào báo cáo và bộ theo dõi thay đổi được xóa, để lỗi của dòng này không ảnh hưởng dòng sau.

**Controllers/ImportController.cs**

```csharp
[HttpPost]
[RequestSizeLimit(KichThuocToiDa + 64 * 1024)]
public async Task<IActionResult> Upload(IFormFile? file)
{
    if (file == null || file.Length == 0) return BadRequest(ApiResponse.Fail("Vui lòng chọn file Excel."));
    if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        return BadRequest(ApiResponse.Fail("Chỉ chấp nhận file .xlsx."));
    if (file.Length > KichThuocToiDa) return BadRequest(ApiResponse.Fail("File vượt quá 5 MB."));
    await using var stream = file.OpenReadStream();
    var data = await _importService.ImportAsync(stream, Path.GetFileName(file.FileName));
    return Ok(ApiResponse.Ok(data));
}
```

**Services/ImportService.cs**

```csharp
foreach (var r in cacDong)
{
    var maSV = DocMaSV(ws, r);
    try
    {
        if (dongLap.TryGetValue(maSV, out var cungMa))
            throw new DongLoiException(CotMaSV, $"Mã SV lặp trong file (trùng dòng {...})");
        if (await XuLyDongAsync(ws, r, maSV, lopDangDung, lopDuocPhep)) ketQua.SoThemMoi++;
        else ketQua.SoCapNhat++;
    }
    catch (DongLoiException ex)  { GhiLoi(ketQua, r, maSV, ex.Loi); }
    catch (NghiepVuException ex) { GhiLoi(ketQua, r, maSV, new[] { new LoiCot(ex.Truong ?? CotMaSV, ex.Message) }); }
    finally { _context.ChangeTracker.Clear(); }      // dòng lỗi không để lại thay đổi
}
await _auditLog.LogAsync("Import", "File", tenFile,
    $"Thêm mới: {ketQua.SoThemMoi}, cập nhật: {ketQua.SoCapNhat}, lỗi: {ketQua.SoLoi}");
```

Xuất danh sách dùng lại đúng truy vấn của trang danh sách (BuildQuery), nên file xuất chứa đúng những sinh viên người dùng được phép thấy với cùng tiêu chí lọc, không phân trang.

**Services/ExportService.cs**

```csharp
public async Task<FileXuat?> ExportStudentsAsync(StudentFilter filter, bool pdf)
{
    var ds = await _studentQuery.BuildQuery(filter).OrderBy(s => s.MaSV)
        .Select(s => new { s.MaSV, s.HoTen, s.NgaySinh, /* ... */ s.TrangThai }).ToListAsync();
    if (ds.Count == 0) return null;
    var thoiDiem = DateTime.Now;
    return TaoFile(pdf, new MauBaoCao
    {
        MaMau = "Mẫu BM03", TieuDe = "DANH SÁCH SINH VIÊN", /* ... tiêu chí, cột, dòng */
        TongSo = $"Tổng số: {ds.Count} sinh viên",
        NguoiXuat = _scope.GetCurrent().UserName, ThoiDiem = thoiDiem,
        TenFile = $"DanhSachSinhVien_{thoiDiem:yyyyMMdd_HHmm}" + DuoiFile(pdf)
    });   // ghi nhật ký Xuat bên trong TaoFile
}
```

### 4.4.6. Đánh giá

Các ca TC25 – TC28 đều đạt. File thử 6 dòng (3 hợp lệ, 3 lỗi cố ý) cho kết quả tổng 6, thêm mới 3, lỗi 3. Ba lỗi được báo đúng dòng, cột và nội dung: lớp ngoài khoa phụ trách, ngày 31/02 không hợp lệ, dòng thêm mới thiếu mật khẩu. Thống kê theo lớp có tổng các dòng bằng dòng tổng cộng. File Excel và PDF tải về được đặt tên theo mẫu BM03, BM04 (ví dụ DanhSachSinhVien_yyyyMMdd_HHmm.xlsx).

## 4.5. Kiểm thử chức năng

Nhóm chạy bộ 35 ca kiểm thử chức năng ngày 11/11/2026 trên bản build từ nhánh main, với một CSDL thử riêng được seed lại từ đầu (1.000 sinh viên). Các ca được thực hiện tự động trên trình duyệt Microsoft Edge (chế độ headless) qua giao thức DevTools: thao tác trên giao diện như người dùng, gọi trực tiếp các API Ajax để thử đường vượt quyền và dữ liệu sai, rồi đối chiếu kết quả với kết quả mong đợi lấy từ đặc tả Use Case. Kết quả: 34/35 ca đạt.

| **Mã** | **FR / NFR** | **Nội dung kiểm thử** | **Kết quả mong đợi** | **Kết quả thực tế** | **Đạt** |
|:--:|----|----|----|----|:--:|
| TC01 | FR-06, FR-08 | Giáo vụ đăng nhập đúng tên đăng nhập, mật khẩu | Vào trang chủ; menu không có mục Quản lý tài khoản | Trang /; menu theo vai trò: đúng | Đạt |
| TC02 | FR-06 | Đăng nhập sai mật khẩu | Thông báo chung, không lộ tài khoản tồn tại | Tên đăng nhập hoặc mật khẩu không đúng | Đạt |
| TC03 | FR-06, NFR-01 | Sai mật khẩu 5 lần liên tiếp rồi nhập đúng | Tài khoản bị khóa tạm 15 phút | Tài khoản tạm khóa, vui lòng thử lại sau | Đạt |
| TC04 | FR-08 | Giáo vụ mở trang Quản lý tài khoản (/Users) | Bị từ chối (403) | Trang /Account/AccessDenied: từ chối truy cập | Đạt |
| TC05 | FR-09 | Giáo vụ CNTT xem hồ sơ sinh viên khoa QTKD | Từ chối; tìm kiếm không trả SV ngoài khoa | Chi tiết: HTTP 403; tìm theo QTKD: 0 kết quả | Đạt |
| TC06 | FR-07 | Đăng xuất rồi mở lại /Students | Chuyển về trang đăng nhập | Trang /Account/Login | Đạt |
| TC07 | FR-01 | Admin xem danh sách tài khoản, lọc vai trò Giáo vụ | Danh sách Giáo vụ có phân trang | 3 tài khoản Giáo vụ | Đạt |
| TC08 | FR-02 | Admin tạo tài khoản Giáo vụ gvtest01, khoa CNTT | Tạo thành công, quay về danh sách | Thông báo: Đã tạo tài khoản 'gvtest01' | Đạt |
| TC09 | FR-02 | Tạo tài khoản trùng tên đăng nhập gvtest01 | Báo lỗi trùng, không tạo | Tên đăng nhập 'gvtest01' đã tồn tại. | Đạt |
| TC10 | FR-05 | Admin khóa tài khoản gvtest01 kèm lý do | Khóa thành công | HTTP 200: Đã khóa tài khoản 'gvtest01'. | Đạt |
| TC11 | FR-05 | Admin tự khóa tài khoản Admin duy nhất | Bị từ chối | HTTP 400: Không thể khóa Admin hoạt động cuối cùng của hệ thống. | Đạt |
| TC12 | FR-04 | Mở khóa và cấp lại mật khẩu gvtest01, đăng nhập bằng mật khẩu mới | Đăng nhập được | Cấp lại: HTTP 200; đăng nhập vào trang / | Đạt |
| TC13 | FR-13 | Thêm hồ sơ hợp lệ TC2026001 qua modal Ajax | Lưu hồ sơ và tạo tài khoản, danh sách tải lại | Đã thêm sinh viên 'TC2026001'. | Đạt |
| TC14 | FR-13 | Thêm hồ sơ trùng mã TC2026001 | HTTP 409, báo mã đã tồn tại | HTTP 409: Mã sinh viên đã tồn tại | Đạt |
| TC15 | FR-13, NFR-05 | Gửi thẳng dữ liệu sai lên server (bỏ qua kiểm tra phía client) | HTTP 400 kèm lỗi theo từng trường | HTTP 400; trường lỗi: sdt, email, matKhau, ngaySinh | Đạt |
| TC15b | FR-13 | Gửi đồng thời 2 yêu cầu thêm cùng mã TCRACE01 | Một yêu cầu thành công, yêu cầu còn lại HTTP 409 | HTTP 500 và 200 | Không đạt |
| TC16 | FR-11, FR-12 | Sinh viên mới đăng nhập, cập nhật liên hệ, thử xem hồ sơ người khác | Đăng nhập được; cập nhật thành công; xem hồ sơ khác bị 403 | Đăng nhập: /; cập nhật: HTTP 200; hồ sơ khác: HTTP 403 | Đạt |
| TC17 | FR-10 | Sinh viên đổi mật khẩu | Thông báo đổi mật khẩu thành công | Thông báo: Đã đổi mật khẩu. | Đạt |
| TC18 | FR-14, FR-15 | Xem chi tiết và cập nhật họ tên hồ sơ | Hiển thị chi tiết; cập nhật thành công | HTTP 200: Đã cập nhật hồ sơ. | Đạt |
| TC19 | FR-16 | Đổi trạng thái Đang học → Bảo lưu, rồi Bảo lưu → Đã tốt nghiệp | Lần 1 thành công; lần 2 bị từ chối theo bảng chuyển trạng thái | Lần 1: HTTP 200; lần 2: HTTP 400 (Không được chuyển từ Bảo lưu sang Đã tốt nghiệp.) | Đạt |
| TC20 | FR-17, FR-09 | Chuyển lớp sang CNTT25B, rồi thử chuyển sang lớp khoa QTKD | Lần 1 thành công; lần 2 bị từ chối | Lần 1: HTTP 200; lần 2: HTTP 403 | Đạt |
| TC21 | FR-19, FR-20 | Cấp lại mật khẩu, khóa tài khoản sinh viên rồi đăng nhập | Đăng nhập bị từ chối với thông báo chung | Cấp lại: HTTP 200; khóa: HTTP 200; đăng nhập: Tên đăng nhập hoặc mật khẩu không đúng | Đạt |
| TC22 | FR-18 | Giáo vụ xóa hồ sơ SV đã từng đăng nhập; Admin xóa mềm | Giáo vụ bị từ chối; Admin xóa được, hồ sơ không còn trong tìm kiếm | Giáo vụ: HTTP 400; Admin: HTTP 200; tìm lại: 0 kết quả | Đạt |
| TC23 | FR-21 | Lập danh sách theo lớp CNTT23A | Mọi dòng thuộc lớp CNTT23A, có phân trang | 32 sinh viên, tất cả thuộc CNTT23A: có | Đạt |
| TC24 | FR-22 | Tìm "nguyen" (không dấu) trên danh sách, debounce 300 ms | Mọi kết quả có họ tên chứa "nguyen" khi bỏ dấu (Nguyễn, Nguyên) | 12 dòng trên trang, ví dụ: Nguyễn Yến Loan; Nguyễn Bảo Huy | Đạt |
| TC25 | FR-27 | Thống kê theo lớp trong phạm vi khoa CNTT | Số liệu theo trạng thái; tổng các dòng bằng dòng tổng cộng | 8 lớp; tổng các dòng 253; dòng tổng cộng 253 | Đạt |
| TC26 | FR-28 | Xuất danh sách lớp CNTT23A ra Excel và PDF | Tải về file .xlsx và .pdf đặt tên theo BM03 | Excel: 200, 10041 byte, DanhSachSinhVien_20261111_2017.xlsx; PDF: 200, 102041 byte | Đạt |
| TC27 | FR-29 | Xuất báo cáo thống kê ra PDF và Excel | Tải về file theo BM04 | PDF: 200, 84393 byte; Excel: 200, 7522 byte | Đạt |
| TC28 | FR-30 | Nhập file Excel 6 dòng (3 hợp lệ, 3 lỗi cố ý) | Tổng 6, thêm mới 3, lỗi 3; báo cáo BM05 nêu dòng, cột, nội dung lỗi | Tổng 6, thêm 3, cập nhật 0, lỗi 3; 5 \| TCIMP004 \| MaLop \| Lớp QTKD25A không thuộc khoa phụ trách ; 6 \| TCIMP005 \| NgaySinh \| Ngày sinh "31/02/2006" không hợp lệ ; 7 \| TCIMP006 \| MatKhauBanDau \| Dòng thêm mới thiếu mật khẩu ban đầu | Đạt |
| TC29 | FR-23 – FR-26 | Giáo vụ thêm Khoa; xóa lớp còn sinh viên | Thêm Khoa bị từ chối (chỉ Admin); xóa lớp bị chặn | Thêm Khoa: HTTP 403; xóa lớp: HTTP 400 (Không thể xóa: lớp còn hồ sơ sinh viên.) | Đạt |
| TC30 | FR-24, FR-26 | Admin thêm Khoa TEST rồi xóa mềm | Cả hai thao tác thành công | Thêm: HTTP 200; xóa: HTTP 200 | Đạt |
| TC31 | FR-31, NFR-07 | Admin tra cứu nhật ký theo đối tượng TC2026001 | Có đủ các hành động đã thực hiện trong kiểm thử | XoaMemHoSo, KhoaTaiKhoan, CapLaiMatKhau, ChuyenLop, DoiTrangThai, SuaHoSo, DoiMatKhau, CapNhatLienHe, TaoTaiKhoan, TaoHoSo | Đạt |
| TC32 | NFR-03 | Mở danh sách sinh viên ở màn hình 390 px | Bố cục co giãn, không cuộn ngang toàn trang | Chiều rộng trang 390 px | Đạt |
| TC33 | FR-03 | Admin đổi khoa phụ trách của gvtest01 từ CNTT sang QTKD | Cập nhật thành công; gvtest01 chỉ còn thấy sinh viên khoa QTKD | Thông báo cập nhật: có; gvtest01 thấy QTKD 242, CNTT 0 sinh viên | Đạt |
| TC34 | FR-23, FR-25 | Admin xem danh mục lớp và sửa tên lớp CNTT25B | Danh sách hiển thị lớp; tên lớp được cập nhật | Trước: "Công nghệ thông tin 25B (sửa)"; sửa: HTTP 200; sau: "Công nghệ thông tin K25 - lớp B" | Đạt |

Phạm vi kiểm thử còn thiếu:

- Mới kiểm thử trên Edge (nhân Chromium); chưa chạy trên Firefox như NFR-06 yêu cầu.
- Thời gian phản hồi đo trên máy phát triển với CSDL LocalDB, chưa đo trên máy demo hay dưới tải nhiều người dùng đồng thời.
- Mã nguồn chưa có project kiểm thử đơn vị hoặc kiểm thử tích hợp chạy tự động trong quy trình build.

## 4.6. Đáp ứng các yêu cầu

Bảng sau đối chiếu các yêu cầu kỹ thuật của bài tập lớn (mục 1.3) với sản phẩm đã xây dựng.

| **Yêu cầu của BTL** | **Minh chứng trong sản phẩm** | **Kết quả** |
|----|----|----|
| Công nghệ ASP.NET Core MVC, HTML, CSS | ASP.NET Core MVC trên .NET 8, Razor View, Bootstrap 5, site.css (mục 4.0 – 4.4). | Đạt |
| Sử dụng các điều khiển Validator | Data Annotations và validator tùy biến MaSo, HoTen, NgaySinh, SoDienThoai, MatKhauManh có adapter phía client (mục 4.2.4); server từ chối dữ liệu sai gửi thẳng (TC15). | Đạt |
| Khai thác CSDL và kết gán dữ liệu | EF Core + SQL Server, Tag Helper asp-for; dropdown Khoa → Khóa → Lớp phụ thuộc nhau; lọc và phân trang tại CSDL (mục 4.3.5). | Đạt |
| Xử lý bằng kỹ thuật AJAX | Danh sách, tìm kiếm, thêm/sửa/đổi trạng thái/chuyển lớp/xóa mềm, danh mục, thống kê, nhập Excel, nhật ký đều gọi Ajax trả JSON qua App trong site.js (mục 4.1.4). | Đạt |
| Xác thực và phân quyền | Identity, \[Authorize(Roles)\], ScopeService giới hạn theo khoa và chủ hồ sơ; TC04, TC05, TC16, TC20, TC29. | Đạt |
| An toàn thông tin | HTTPS, cookie HttpOnly/Secure, khóa tạm 15 phút sau 5 lần sai (TC03), anti-forgery toàn cục, thông báo đăng nhập chung (TC02), dữ liệu hiển thị bằng textContent/.text(), nhật ký thao tác (TC31), mật khẩu mẫu không nằm trong mã nguồn. | Đạt |
| Responsive, SEO | Bố cục co giãn, menu thu gọn trên điện thoại, không cuộn ngang ở 390 px (TC32). Trang đăng nhập có title và meta description; các trang nội bộ đặt noindex, nofollow; HTML5 ngữ nghĩa header/nav/main/footer. | Đạt |
