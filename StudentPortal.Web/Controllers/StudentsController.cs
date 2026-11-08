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

        public StudentsController(IStudentQueryService studentQueryService, IExportService exportService)
        {
            _studentQueryService = studentQueryService;
            _exportService = exportService;
        }

        // GET: /Students
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Students/Search?keyword=&maKhoa=&maKhoaHoc=&maLop=&trangThai=&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] StudentFilter filter) =>
            Ok(ApiResponse.Ok(await _studentQueryService.SearchAsync(filter)));

        // GET: /Students/Export?format=excel|pdf&keyword=&maKhoa=&... (FR-28)
        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] StudentFilter filter, string format = "excel")
        {
            var file = await _exportService.ExportStudentsAsync(filter, format == "pdf");
            if (file == null) return BadRequest(ApiResponse.Fail("Không có sinh viên nào để xuất."));
            return File(file.NoiDung, file.ContentType, file.TenFile);
        }
    }
}
