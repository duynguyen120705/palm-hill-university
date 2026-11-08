using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAccountService accountService, ILogger<UsersController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // GET: /Users
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Users/List?keyword=&vaiTro=&maKhoa=&trangThai=&page=1&pageSize=10 (FR-01)
        [HttpGet]
        public Task<IActionResult> List([FromQuery] TaiKhoanFilterDto filter) =>
            XuLyAsync(async () => Json(await _accountService.ListStaffAsync(filter)));

        // POST: /Users/Create (jQuery AJAX, JSON) - FR-02
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Create([FromBody] StaffCreateDto dto) =>
            XuLyAsync(async () =>
            {
                (await _accountService.CreateStaffAccountAsync(dto)).ThrowIfFailed();
                return Ok(new { success = true, message = $"Đã tạo tài khoản '{dto.UserName.Trim()}'." });
            });

        // POST: /Users/Edit (jQuery AJAX, JSON) - FR-03
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Edit([FromBody] TaiKhoanUpdateDto dto) =>
            XuLyAsync(async () =>
            {
                await _accountService.UpdateStaffAsync(dto);
                var data = await _accountService.GetStaffAsync(dto.Id);
                return Ok(new { success = true, message = $"Đã cập nhật tài khoản '{data.UserName}'.", data });
            });

        // POST: /Users/ResetPassword (jQuery AJAX, JSON) - FR-04
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ResetPassword([FromBody] DatLaiMatKhauDto dto) =>
            XuLyAsync(async () =>
            {
                var taiKhoan = await _accountService.GetStaffAsync(dto.Id);
                (await _accountService.ResetPasswordAsync(taiKhoan.UserName, dto.MatKhauMoi)).ThrowIfFailed();
                return Ok(new { success = true, message = "Đã cấp lại mật khẩu." });
            });

        // POST: /Users/SetLock (jQuery AJAX, JSON) - FR-05: khóa thay cho xóa vì tài khoản không bị xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> SetLock([FromBody] DoiTrangThaiTaiKhoanDto dto) =>
            XuLyAsync(async () =>
            {
                var taiKhoan = await _accountService.GetStaffAsync(dto.Id);
                (await _accountService.SetLockAsync(taiKhoan.UserName, dto.Khoa, dto.LyDo)).ThrowIfFailed();
                var data = await _accountService.GetStaffAsync(dto.Id);
                var hanhDong = dto.Khoa ? "khóa" : "mở khóa";
                return Ok(new { success = true, message = $"Đã {hanhDong} tài khoản '{data.UserName}'.", data });
            });

        // Gom xử lý lỗi validate / nghiệp vụ / hệ thống cho các endpoint Ajax
        private async Task<IActionResult> XuLyAsync(Func<Task<IActionResult>> action)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", errors });
            }

            try
            {
                return await action();
            }
            catch (NghiepVuException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý tài khoản");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xử lý tài khoản." });
            }
        }
    }
}
