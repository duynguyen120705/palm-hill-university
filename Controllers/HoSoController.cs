using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;
using StudentPortal.Services;

namespace StudentPortal.Controllers
{
    // FR-10 Đổi mật khẩu, FR-11 Xem hồ sơ cá nhân, FR-12 Cập nhật thông tin liên hệ
    // Không action nào nhận định danh từ client - luôn dùng tài khoản đang đăng nhập (FR-09)
    [Authorize]
    public class HoSoController : Controller
    {
        private readonly IHoSoCaNhanService _hoSoService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HoSoController> _logger;

        public HoSoController(IHoSoCaNhanService hoSoService, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, ILogger<HoSoController> logger)
        {
            _hoSoService = hoSoService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /HoSo
        public async Task<IActionResult> Index()
        {
            return View(await _hoSoService.LayHoSoAsync(User));
        }

        // GET: /HoSo/DoiMatKhau
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View();
        }

        // POST: /HoSo/CapNhatLienHe (jQuery AJAX, JSON) - chỉ sinh viên
        [HttpPost]
        [Authorize(Roles = "SinhVien")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CapNhatLienHe([FromBody] CapNhatLienHeDto dto) =>
            XuLyAsync(async () =>
            {
                var data = await _hoSoService.CapNhatLienHeAsync(dto, User);
                return Ok(new { success = true, message = "Đã cập nhật thông tin liên hệ.", data });
            });

        // POST: /HoSo/DoiMatKhau (jQuery AJAX, JSON) - cả ba vai trò
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DoiMatKhau")]
        public Task<IActionResult> DoiMatKhauPost([FromBody] DoiMatKhauDto dto) =>
            XuLyAsync(async () =>
            {
                await _hoSoService.DoiMatKhauAsync(dto, User);
                // SecurityStamp đã đổi: cấp lại cookie để phiên hiện tại tiếp tục hợp lệ (FR-10)
                var user = await _userManager.GetUserAsync(User);
                if (user != null) await _signInManager.RefreshSignInAsync(user);
                return Ok(new { success = true, message = "Đã đổi mật khẩu." });
            });

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
                _logger.LogError(ex, "Lỗi khi xử lý hồ sơ cá nhân");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ." });
            }
        }
    }
}
