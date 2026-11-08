using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-27 Thống kê sinh viên, FR-29 Xuất báo cáo thống kê - Admin xem toàn trường, Giáo vụ xem khoa phụ trách
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IExportService _exportService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticsService statisticsService, IExportService exportService,
            ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _exportService = exportService;
            _logger = logger;
        }

        // GET: /Statistics
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Statistics/Data?maKhoa=&maKhoaHoc=&maLop=&trangThai=
        [HttpGet]
        public async Task<IActionResult> Data([FromQuery] ThongKeFilterDto filter)
        {
            try
            {
                var data = await _statisticsService.GetAsync(filter);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thống kê sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi thống kê." });
            }
        }

        // GET: /Statistics/Export?format=excel|pdf&maKhoa=&maKhoaHoc=&maLop=&trangThai= (FR-29)
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] ThongKeFilterDto filter, string format = "excel")
        {
            try
            {
                var file = await _exportService.ExportStatisticsAsync(filter, format == "pdf");
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
    }
}
