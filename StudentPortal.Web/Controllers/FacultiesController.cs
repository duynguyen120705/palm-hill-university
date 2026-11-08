using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-23 – FR-26 Danh mục Khoa: Admin quản lý, Giáo vụ chỉ xem khoa phụ trách
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class FacultiesController : Controller
    {
        private readonly ICatalogService _catalogService;

        public FacultiesController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // GET: /Faculties
        public IActionResult Index()
        {
            ViewData["Tab"] = "khoa";
            return View("~/Views/Catalog/Index.cshtml");
        }

        // GET: /Faculties/List?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CatalogFilterVm filter) =>
            Ok(ApiResponse.Ok(await _catalogService.ListFacultiesAsync(filter)));

        // POST: /Faculties/Create (Ajax, JSON)
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] FacultyCreateVm dto)
        {
            await _catalogService.CreateFacultyAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã thêm khoa '{dto.MaKhoa.Trim()}'."));
        }

        // POST: /Faculties/Edit (Ajax, JSON)
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Edit([FromBody] FacultyEditVm dto)
        {
            await _catalogService.UpdateFacultyAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã cập nhật khoa '{dto.MaKhoa}'."));
        }

        // POST: /Faculties/Delete (Ajax, JSON) - xóa mềm
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete([FromBody] CatalogDeleteVm dto)
        {
            await _catalogService.DeleteFacultyAsync(dto.Ma);
            return Ok(ApiResponse.Ok(message: $"Đã xóa khoa '{dto.Ma}'."));
        }
    }
}
