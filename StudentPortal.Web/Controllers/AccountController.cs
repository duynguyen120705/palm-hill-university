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
        // Thông báo chung, không tiết lộ tên đăng nhập có tồn tại hay tài khoản bị khóa (UC01 4a)
        private const string LoiDangNhap = "Tên đăng nhập hoặc mật khẩu không đúng";
        private const string LoiKhoaTam = "Tài khoản tạm khóa, vui lòng thử lại sau";

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
            return View(new LoginVm { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVm dto)
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

                // Tài khoản BiKhoa bị từ chối trước bước kiểm tra mật khẩu nên phải tự tăng số lần sai (UC01 4a)
                if (result.IsNotAllowed)
                {
                    var biKhoa = await _userManager.FindByNameAsync(dto.UserName.Trim());
                    if (biKhoa != null) await _userManager.AccessFailedAsync(biKhoa);
                }

                ModelState.AddModelError(string.Empty, result.IsLockedOut ? LoiKhoaTam : LoiDangNhap);
                dto.MatKhau = string.Empty;
                return View(dto);
            }

            var user = await _userManager.FindByNameAsync(dto.UserName.Trim());
            if (user != null)
            {
                // Ghi thời điểm đăng nhập cuối; FR-18 dựa vào mốc này để xét Giáo vụ được xóa mềm hồ sơ hay không
                user.LanDangNhapCuoi = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToLocal(dto.ReturnUrl);
        }

        // POST: /Account/Logout
        // Phiên đã hết hạn thì token anti-forgery (gắn với người dùng cũ) không còn khớp; FR-07 yêu cầu
        // vẫn chuyển về trang đăng nhập: token sai thì về Login (không chạy action) thay vì trả lỗi 400
        [HttpPost]
        [AllowAnonymous]
        [VeLoginKhiTokenSai]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [Authorize]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // GET: /Account/Profile (FR-11): định danh luôn lấy từ phiên đăng nhập
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile(string? id, string? maSV)
        {
            // Yêu cầu chứa định danh hồ sơ khác: từ chối (UC04 2a)
            var dinhDanh = id ?? maSV;
            if (dinhDanh != null && !string.Equals(dinhDanh, User.Identity!.Name, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return View(await _accountService.GetProfileAsync());
        }

        // POST: /Account/UpdateContact (Ajax, JSON) - chỉ sinh viên (FR-12)
        [HttpPost]
        [Authorize(Roles = Roles.SinhVien)]
        public async Task<IActionResult> UpdateContact([FromBody] ContactEditVm dto)
        {
            var data = await _accountService.UpdateContactAsync(dto);
            return Ok(ApiResponse.Ok(data, "Đã cập nhật thông tin liên hệ."));
        }

        // GET: /Account/ChangePassword (FR-10)
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVm());
        }

        // POST: /Account/ChangePassword - cả ba vai trò; lỗi báo tại trường tương ứng (UC03 4a, 4b)
        [HttpPost]
        [Authorize]
        [GiaoDich]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = _userManager.GetUserId(User)
                ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);
            var ketQua = await _accountService.ChangePasswordAsync(userId, dto.MatKhauHienTai, dto.MatKhauMoi);
            if (!ketQua.Succeeded)
            {
                ModelState.AddModelError(ketQua.Truong ?? nameof(dto.MatKhauMoi), ketQua.Error!);
                return View(dto);
            }

            // SecurityStamp đã đổi: cấp lại cookie để phiên hiện tại tiếp tục hợp lệ (FR-10)
            var user = await _userManager.GetUserAsync(User);
            if (user != null) await _signInManager.RefreshSignInAsync(user);
            TempData["ThongBao"] = "Đã đổi mật khẩu.";
            return RedirectToAction(nameof(ChangePassword));
        }

        // Về trang chủ theo vai trò; returnUrl không phải URL nội bộ thì bỏ qua (UC01 6a)
        private IActionResult RedirectToLocal(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Home");

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
