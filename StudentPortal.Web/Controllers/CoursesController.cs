using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-23 – FR-26 Danh mục Khóa: Admin quản lý, Giáo vụ chỉ xem
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class CoursesController : Controller
    {
        private readonly ICatalogService _catalogService;

        public CoursesController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // GET: /Courses
        public IActionResult Index()
        {
            ViewData["Tab"] = "khoahoc";
            return View("~/Views/Catalog/Index.cshtml");
        }

        // GET: /Courses/List?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CatalogFilterVm filter) =>
            Ok(ApiResponse.Ok(await _catalogService.ListCoursesAsync(filter)));

        // POST: /Courses/Create (Ajax, JSON)
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] CourseCreateVm dto)
        {
            await _catalogService.CreateCourseAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã thêm khóa '{dto.MaKhoaHoc.Trim()}'."));
        }

        // POST: /Courses/Edit (Ajax, JSON)
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Edit([FromBody] CourseEditVm dto)
        {
            await _catalogService.UpdateCourseAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã cập nhật khóa '{dto.MaKhoaHoc}'."));
        }

        // POST: /Courses/Delete (Ajax, JSON) - xóa mềm
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete([FromBody] CatalogDeleteVm dto)
        {
            await _catalogService.DeleteCourseAsync(dto.Ma);
            return Ok(ApiResponse.Ok(message: $"Đã xóa khóa '{dto.Ma}'."));
        }
    }
}
