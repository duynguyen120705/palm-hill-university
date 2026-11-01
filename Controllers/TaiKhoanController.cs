using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Models.DTOs;
using StudentPortal.Services;

namespace StudentPortal.Controllers
{
    // FR-01 – FR-05 Quản lý tài khoản Admin/Giáo vụ - chỉ Admin
    [Authorize(Roles = "Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly IQuanLyTaiKhoanService _taiKhoanService;
        private readonly ILogger<TaiKhoanController> _logger;

        public TaiKhoanController(IQuanLyTaiKhoanService taiKhoanService, ILogger<TaiKhoanController> logger)
        {
            _taiKhoanService = taiKhoanService;
            _logger = logger;
        }

        // GET: /TaiKhoan
        public IActionResult Index()
        {
            return View();
        }

        // GET: /TaiKhoan/Data?vaiTro=&maKhoa=&trangThai=&page=1&pageSize=10 (XMLHttpRequest)
        [HttpGet]
        public Task<IActionResult> Data([FromQuery] TaiKhoanFilterDto filter) =>
            XuLyAsync(async () => Json(await _taiKhoanService.DanhSachAsync(filter)));

        // GET: /TaiKhoan/Khoas - lựa chọn khoa phụ trách
        [HttpGet]
        public Task<IActionResult> Khoas() =>
            XuLyAsync(async () => Json(new { success = true, data = await _taiKhoanService.DanhSachKhoaAsync() }));

        // POST: /TaiKhoan/Create (jQuery AJAX, JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Create([FromBody] TaiKhoanCreateDto dto) =>
            XuLyAsync(async () =>
            {
                var data = await _taiKhoanService.TaoAsync(dto, User);
                return Ok(new { success = true, message = $"Đã tạo tài khoản '{data.UserName}'.", data });
            });

        // POST: /TaiKhoan/Update (jQuery AJAX, JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Update([FromBody] TaiKhoanUpdateDto dto) =>
            XuLyAsync(async () =>
            {
                var data = await _taiKhoanService.CapNhatAsync(dto, User);
                return Ok(new { success = true, message = $"Đã cập nhật tài khoản '{data.UserName}'.", data });
            });

        // POST: /TaiKhoan/DatLaiMatKhau (jQuery AJAX, JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DatLaiMatKhau([FromBody] DatLaiMatKhauDto dto) =>
            XuLyAsync(async () =>
            {
                await _taiKhoanService.DatLaiMatKhauAsync(dto, User);
                return Ok(new { success = true, message = "Đã cấp lại mật khẩu." });
            });

        // POST: /TaiKhoan/DoiTrangThai (jQuery AJAX, JSON) - khóa thay cho xóa vì tài khoản không bị xóa (FR-05)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DoiTrangThai([FromBody] DoiTrangThaiTaiKhoanDto dto) =>
            XuLyAsync(async () =>
            {
                var data = await _taiKhoanService.DoiTrangThaiAsync(dto, User);
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
