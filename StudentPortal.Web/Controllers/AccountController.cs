using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-06 Đăng nhập, FR-07 Đăng xuất, FR-10 Đổi mật khẩu, FR-11 Hồ sơ cá nhân, FR-12 Cập nhật liên hệ.
    // Không action nào nhận định danh từ client - luôn dùng tài khoản đang đăng nhập (FR-09).
    public class AccountController : Controller
    {
        // Thông báo chung, không tiết lộ tên đăng nhập có tồn tại hay tài khoản bị khóa (FR-06)
        private const string LoiDangNhap = "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản không được phép đăng nhập.";

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            IAccountService accountService, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _accountService = accountService;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToLocal(returnUrl);
            return View(new DangNhapDto { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DangNhapDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            // lockoutOnFailure: sai 5 lần liên tiếp khóa tạm 15 phút, tự mở lại (cấu hình trong Program.cs)
            var result = await _signInManager.PasswordSignInAsync(dto.UserName.Trim(), dto.MatKhau,
                isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // Không ghi mật khẩu hay tên đăng nhập vào log ứng dụng (NFR-08)
                _logger.LogInformation("Đăng nhập thất bại (LockedOut={LockedOut}, NotAllowed={NotAllowed})",
                    result.IsLockedOut, result.IsNotAllowed);
                ModelState.AddModelError(string.Empty, LoiDangNhap);
                dto.MatKhau = string.Empty;
                return View(dto);
            }

            var user = await _userManager.FindByNameAsync(dto.UserName.Trim());
            if (user != null && user.LanDangNhapDauTien == null)
            {
                // FR-18 dựa vào mốc này để xét Giáo vụ được xóa mềm hồ sơ hay không
                user.LanDangNhapDauTien = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrEmpty(dto.ReturnUrl) && Url.IsLocalUrl(dto.ReturnUrl))
                return LocalRedirect(dto.ReturnUrl);
            return await RedirectTheoVaiTroAsync(user);
        }

        // POST: /Account/Logout
        // Phiên đã hết hạn thì token anti-forgery (gắn với người dùng cũ) không còn khớp; FR-07 yêu cầu
        // vẫn chuyển về trang đăng nhập: token sai thì về Login (không chạy action) thay vì trả lỗi 400
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [VeLoginKhiTokenSai]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // GET: /Account/Profile (FR-11)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            return View(await _accountService.GetProfileAsync());
        }

        // POST: /Account/UpdateContact (jQuery AJAX, JSON) - chỉ sinh viên (FR-12)
        [HttpPost]
        [Authorize(Roles = Roles.SinhVien)]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateContact([FromBody] CapNhatLienHeDto dto) =>
            XuLyAsync(async () =>
            {
                var data = await _accountService.UpdateContactAsync(dto);
                return Ok(new { success = true, message = "Đã cập nhật thông tin liên hệ.", data });
            });

        // GET: /Account/ChangePassword (FR-10)
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword (jQuery AJAX, JSON) - cả ba vai trò
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(ChangePassword))]
        public Task<IActionResult> ChangePasswordPost([FromBody] DoiMatKhauDto dto) =>
            XuLyAsync(async () =>
            {
                var userId = _userManager.GetUserId(User) ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", 401);
                (await _accountService.ChangePasswordAsync(userId, dto.MatKhauHienTai, dto.MatKhauMoi)).ThrowIfFailed();

                // SecurityStamp đã đổi: cấp lại cookie để phiên hiện tại tiếp tục hợp lệ (FR-10)
                var user = await _userManager.GetUserAsync(User);
                if (user != null) await _signInManager.RefreshSignInAsync(user);
                return Ok(new { success = true, message = "Đã đổi mật khẩu." });
            });

        // Mở giao diện theo vai trò (FR-06 bước 4)
        private async Task<IActionResult> RedirectTheoVaiTroAsync(ApplicationUser? user)
        {
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, Roles.Admin)) return RedirectToAction("Index", "Users");
                if (await _userManager.IsInRoleAsync(user, Roles.CanBo)) return RedirectToAction("Index", "Statistics");
                if (await _userManager.IsInRoleAsync(user, Roles.SinhVien)) return RedirectToAction(nameof(Profile));
            }
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Home");

        private async Task<IActionResult> XuLyAsync(Func<Task<IActionResult>> action)
        {
            if (!ModelState.IsValid)
            {
                // Lỗi đọc JSON (gồm gửi trường ngoài quyền sửa - JsonUnmappedMemberHandling.Disallow) nằm ở khóa "$..."
                // hoặc làm thiếu "dto"; không trả chi tiết kỹ thuật cho client
                if (ModelState.Keys.Any(k => k.StartsWith('$') || k == "dto"))
                    return BadRequest(new { success = false, message = "Dữ liệu gửi lên không hợp lệ hoặc chứa trường không được phép sửa." });

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Distinct().ToList();
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
                _logger.LogError(ex, "Lỗi khi xử lý tài khoản cá nhân");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ." });
            }
        }

        // Thay kết quả lỗi anti-forgery (400) bằng chuyển hướng về trang đăng nhập
        private sealed class VeLoginKhiTokenSaiAttribute : Attribute, IAlwaysRunResultFilter
        {
            public void OnResultExecuting(ResultExecutingContext context)
            {
                if (context.Result is IAntiforgeryValidationFailedResult)
                    context.Result = new RedirectToActionResult(nameof(Login), "Account", null);
            }

            public void OnResultExecuted(ResultExecutedContext context)
            {
                // Không cần xử lý sau khi trả kết quả
            }
        }
    }
}
