using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public UsersController(IAccountService accountService)
        {
            _accountService = accountService;
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

        // POST: /Users/Create (Ajax, JSON) - FR-02
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
        {
            (await _accountService.CreateStaffAccountAsync(dto)).ThrowIfFailed();
            return Ok(ApiResponse.Ok(message: $"Đã tạo tài khoản '{dto.UserName.Trim()}'."));
        }

        // POST: /Users/Edit (Ajax, JSON) - FR-03
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StaffEditVm dto)
        {
            await _accountService.UpdateStaffAsync(dto);
            var data = await _accountService.GetStaffAsync(dto.Id);
            return Ok(ApiResponse.Ok(data, $"Đã cập nhật tài khoản '{data.UserName}'."));
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
    }
}
