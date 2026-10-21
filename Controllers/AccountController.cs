using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Controllers
{
    // FR-06 Đăng nhập, FR-07 Đăng xuất
    public class AccountController : Controller
    {
        // Thông báo chung, không tiết lộ tên đăng nhập có tồn tại hay tài khoản bị khóa (FR-06)
        private const string LoiDangNhap = "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản không được phép đăng nhập.";

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
        // vẫn chuyển về trang đăng nhập nên bỏ qua kiểm tra token - đăng xuất không thay đổi dữ liệu
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
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

        // Mở giao diện theo vai trò (FR-06 bước 4)
        private async Task<IActionResult> RedirectTheoVaiTroAsync(ApplicationUser? user)
        {
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin")) return RedirectToAction("Index", "TaiKhoan");
                if (await _userManager.IsInRoleAsync(user, "GiaoVu")) return RedirectToAction("Index", "ThongKe");
                if (await _userManager.IsInRoleAsync(user, "SinhVien")) return RedirectToAction("Index", "HoSo");
            }
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Home");
    }
}
