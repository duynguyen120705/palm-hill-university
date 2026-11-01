using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Models.DTOs;
using StudentPortal.Services;

namespace StudentPortal.Controllers
{
    // FR-27 Thống kê sinh viên - Admin xem toàn trường, Giáo vụ xem khoa phụ trách
    [Authorize(Roles = "Admin,GiaoVu")]
    public class ThongKeController : Controller
    {
        private readonly IThongKeService _thongKeService;
        private readonly ILogger<ThongKeController> _logger;

        public ThongKeController(IThongKeService thongKeService, ILogger<ThongKeController> logger)
        {
            _thongKeService = thongKeService;
            _logger = logger;
        }

        // GET: /ThongKe
        public IActionResult Index()
        {
            return View();
        }

        // GET: /ThongKe/BoLoc - lựa chọn Khoa/Khóa/Lớp trong phạm vi được phép
        [HttpGet]
        public async Task<IActionResult> BoLoc()
        {
            try
            {
                var data = await _thongKeService.LayBoLocAsync(User);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải bộ lọc thống kê");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải bộ lọc." });
            }
        }

        // GET: /ThongKe/Xuat?format=excel|pdf&maKhoa=&maKhoaHoc=&maLop=&trangThai= (FR-29)
        [HttpGet]
        public async Task<IActionResult> Xuat([FromQuery] ThongKeFilterDto filter, string format = "excel")
        {
            try
            {
                var file = await _thongKeService.XuatAsync(filter, format == "pdf", User);
                if (file == null)
                    return BadRequest(new { success = false, message = "Không có dữ liệu thống kê để xuất." });
                return File(file.NoiDung, file.ContentType, file.TenFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất báo cáo thống kê");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xuất báo cáo." });
            }
        }

        // GET: /ThongKe/Data?maKhoa=&maKhoaHoc=&maLop=&trangThai=
        [HttpGet]
        public async Task<IActionResult> Data([FromQuery] ThongKeFilterDto filter)
        {
            try
            {
                var data = await _thongKeService.ThongKeAsync(filter, User);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi thống kê." });
            }
        }
    }
}
