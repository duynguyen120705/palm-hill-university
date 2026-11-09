using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // Hồ sơ và tài khoản sinh viên (FR-13 – FR-20), danh sách, tìm kiếm (FR-21, FR-22), xuất danh sách (FR-28)
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IStudentQueryService _studentQueryService;
        private readonly IExportService _exportService;

        public StudentsController(IStudentService studentService, IStudentQueryService studentQueryService,
            IExportService exportService)
        {
            _studentService = studentService;
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

        // POST: /Students/Create (Ajax, JSON) - FR-13
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentCreateVm dto)
        {
            await _studentService.CreateAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã thêm sinh viên '{dto.MaSV.Trim().ToUpperInvariant()}'."));
        }

        // GET: /Students/Details/{maSV} - FR-14
        [HttpGet("Students/Details/{maSV}")]
        public async Task<IActionResult> Details(string maSV)
        {
            try
            {
                return View(await _studentService.GetDetailAsync(maSV));
            }
            catch (NghiepVuException ex) when (ex.StatusCode == StatusCodes.Status404NotFound)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                ViewData["ThongBao"] = ex.Message;
                return View("NotFound");
            }
        }

        // POST: /Students/Edit (Ajax, JSON) - FR-15
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StudentEditVm dto) =>
            Ok(ApiResponse.Ok(await _studentService.UpdateAsync(dto), "Đã cập nhật hồ sơ."));

        // GET: /Students/AllowedStatuses/{maSV} - FR-16
        [HttpGet("Students/AllowedStatuses/{maSV}")]
        public async Task<IActionResult> AllowedStatuses(string maSV) =>
            Ok(ApiResponse.Ok(await _studentService.GetAllowedStatusesAsync(maSV)));

        // POST: /Students/ChangeStatus (Ajax, JSON) - FR-16
        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusVm dto) =>
            Ok(ApiResponse.Ok(await _studentService.ChangeStatusAsync(dto), "Đã đổi trạng thái học tập."));

        // POST: /Students/Transfer (Ajax, JSON) - FR-17
        [HttpPost]
        public async Task<IActionResult> Transfer([FromBody] TransferVm dto) =>
            Ok(ApiResponse.Ok(await _studentService.TransferAsync(dto), "Đã chuyển lớp."));

        // POST: /Students/Delete (Ajax, JSON) - FR-18: xóa mềm
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] StudentDeleteVm dto)
        {
            await _studentService.SoftDeleteAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã xóa hồ sơ '{dto.MaSV}'."));
        }

        // POST: /Students/ResetPassword (Ajax, JSON) - FR-19
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] StudentResetPasswordVm dto)
        {
            await _studentService.ResetPasswordAsync(dto);
            return Ok(ApiResponse.Ok(message: "Đã cấp lại mật khẩu."));
        }

        // POST: /Students/SetLock (Ajax, JSON) - FR-20
        [HttpPost]
        public async Task<IActionResult> SetLock([FromBody] StudentSetLockVm dto)
        {
            var data = await _studentService.SetLockAsync(dto);
            return Ok(ApiResponse.Ok(data, dto.Khoa ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản."));
        }

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
