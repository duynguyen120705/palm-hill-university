using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // Hồ sơ sinh viên: danh sách, tìm kiếm (FR-21, FR-22), xuất danh sách (FR-28)
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class StudentsController : Controller
    {
        private readonly IStudentQueryService _studentQueryService;
        private readonly IExportService _exportService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentQueryService studentQueryService, IExportService exportService,
            ILogger<StudentsController> logger)
        {
            _studentQueryService = studentQueryService;
            _exportService = exportService;
            _logger = logger;
        }

        // GET: /Students
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Students/Search?keyword=&maKhoa=&maKhoaHoc=&maLop=&trangThai=&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] StudentFilter filter)
        {
            try
            {
                return Json(await _studentQueryService.SearchAsync(filter));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải danh sách." });
            }
        }

        // GET: /Students/Export?format=excel|pdf&keyword=&maKhoa=&... (FR-28)
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] StudentFilter filter, string format = "excel")
        {
            try
            {
                var file = await _exportService.ExportStudentsAsync(filter, format == "pdf");
                if (file == null)
                    return BadRequest(new { success = false, message = "Không có sinh viên nào để xuất." });
                return File(file.NoiDung, file.ContentType, file.TenFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất danh sách sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xuất danh sách." });
            }
        }
    }
}
