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

        public StatisticsController(IStatisticsService statisticsService, IExportService exportService)
        {
            _statisticsService = statisticsService;
            _exportService = exportService;
        }

        // GET: /Statistics
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Statistics/Data?maKhoa=&maKhoaHoc=&maLop=&trangThai=
        [HttpGet]
        public async Task<IActionResult> Data([FromQuery] StatisticsFilterVm filter) =>
            Ok(ApiResponse.Ok(await _statisticsService.GetAsync(filter)));

        // GET: /Statistics/Export?format=excel|pdf&maKhoa=&maKhoaHoc=&maLop=&trangThai= (FR-29)
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] StatisticsFilterVm filter, string format = "excel")
        {
            var file = await _exportService.ExportStatisticsAsync(filter, format == "pdf");
            if (file == null) return BadRequest(ApiResponse.Fail("Không có dữ liệu thống kê để xuất."));
            return File(file.NoiDung, file.ContentType, file.TenFile);
        }
    }
}
