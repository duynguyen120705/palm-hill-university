using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-01 – FR-05 Quản lý tài khoản Admin/Giáo vụ - chỉ Admin
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ICatalogService _catalogService;

        public UsersController(IAccountService accountService, ICatalogService catalogService)
        {
            _accountService = accountService;
            _catalogService = catalogService;
        }

        // GET: /Users
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Users/List?keyword=&vaiTro=&maKhoa=&trangThai=&page=1&pageSize=20 (FR-01)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] UserFilterVm filter) =>
            Ok(ApiResponse.Ok(await _accountService.ListStaffAsync(filter)));

        // GET: /Users/Create (FR-02)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachKhoaAsync();
            return View(new StaffCreateDto());
        }

        // POST: /Users/Create - lỗi báo tại trường, giữ nguyên biểu mẫu (UC06 6a)
        [HttpPost]
        public async Task<IActionResult> Create(StaffCreateDto dto)
        {
            if (ModelState.IsValid)
            {
                var ketQua = await _accountService.CreateStaffAccountAsync(dto);
                if (ketQua.Succeeded)
                {
                    TempData["ThongBao"] = $"Đã tạo tài khoản '{dto.UserName.Trim()}'.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(ketQua.Truong ?? string.Empty, ketQua.Error!);
            }

            await NapDanhSachKhoaAsync();
            return View(dto);
        }

        // GET: /Users/Edit/{id} (FR-03)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var taiKhoan = await _accountService.GetStaffAsync(id);
                ViewData["UserName"] = taiKhoan.UserName;
                await NapDanhSachKhoaAsync();
                return View(new StaffEditVm
                {
                    Id = taiKhoan.Id,
                    HoTen = taiKhoan.HoTen ?? string.Empty,
                    VaiTro = taiKhoan.VaiTro,
                    MaKhoaPhuTrach = taiKhoan.MaKhoaPhuTrach
                });
            }
            catch (NghiepVuException ex) when (ex.StatusCode == StatusCodes.Status404NotFound)
            {
                return KhongTimThay(ex.Message);
            }
        }

        // POST: /Users/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(StaffEditVm dto)
        {
            try
            {
                var taiKhoan = await _accountService.GetStaffAsync(dto.Id);
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _accountService.UpdateStaffAsync(dto);
                        TempData["ThongBao"] = $"Đã cập nhật tài khoản '{taiKhoan.UserName}'.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (NghiepVuException ex) when (ex.StatusCode != StatusCodes.Status404NotFound)
                    {
                        ModelState.AddModelError(ex.Truong ?? string.Empty, ex.Message);
                    }
                }

                ViewData["UserName"] = taiKhoan.UserName;
                await NapDanhSachKhoaAsync();
                return View(dto);
            }
            catch (NghiepVuException ex) when (ex.StatusCode == StatusCodes.Status404NotFound)
            {
                return KhongTimThay(ex.Message);
            }
        }

        // POST: /Users/ResetPassword (Ajax, JSON) - FR-04
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVm dto)
        {
            var taiKhoan = await _accountService.GetStaffAsync(dto.Id);
            (await _accountService.ResetPasswordAsync(taiKhoan.UserName, dto.MatKhauMoi)).ThrowIfFailed();
            return Ok(ApiResponse.Ok(message: $"Đã cấp lại mật khẩu cho '{taiKhoan.UserName}'."));
        }

        // POST: /Users/SetLock (Ajax, JSON) - FR-05: khóa thay cho xóa vì tài khoản không bị xóa
        [HttpPost]
        public async Task<IActionResult> SetLock([FromBody] SetLockVm dto)
        {
            var taiKhoan = await _accountService.GetStaffAsync(dto.Id);
            (await _accountService.SetLockAsync(taiKhoan.UserName, dto.Khoa, dto.LyDo)).ThrowIfFailed();
            var data = await _accountService.GetStaffAsync(dto.Id);
            var hanhDong = dto.Khoa ? "khóa" : "mở khóa";
            return Ok(ApiResponse.Ok(data, $"Đã {hanhDong} tài khoản '{data.UserName}'."));
        }

        private async Task NapDanhSachKhoaAsync() =>
            ViewData["Khoas"] = new SelectList(await _catalogService.LookupAsync("khoa", null, null),
                nameof(LookupItemDto.Ma), nameof(LookupItemDto.Ten));

        private ViewResult KhongTimThay(string thongBao)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            ViewData["ThongBao"] = thongBao;
            return View("NotFound");
        }
    }
}
