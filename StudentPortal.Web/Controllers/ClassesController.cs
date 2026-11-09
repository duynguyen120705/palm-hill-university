using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-23 – FR-26 Danh mục Lớp: Admin toàn trường, Giáo vụ trong khoa phụ trách
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class ClassesController : Controller
    {
        private readonly ICatalogService _catalogService;

        public ClassesController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // GET: /Classes
        public IActionResult Index()
        {
            ViewData["Tab"] = "lop";
            return View("~/Views/Catalog/Index.cshtml");
        }

        // GET: /Classes/List?maKhoa=&maKhoaHoc=&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CatalogFilterVm filter) =>
            Ok(ApiResponse.Ok(await _catalogService.ListClassesAsync(filter)));

        // POST: /Classes/Create (Ajax, JSON)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClassCreateVm dto)
        {
            await _catalogService.CreateClassAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã thêm lớp '{dto.MaLop.Trim()}'."));
        }

        // POST: /Classes/Edit (Ajax, JSON)
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ClassEditVm dto)
        {
            await _catalogService.UpdateClassAsync(dto);
            return Ok(ApiResponse.Ok(message: $"Đã cập nhật lớp '{dto.MaLop}'."));
        }

        // POST: /Classes/Delete (Ajax, JSON) - xóa mềm
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] CatalogDeleteVm dto)
        {
            await _catalogService.DeleteClassAsync(dto.Ma);
            return Ok(ApiResponse.Ok(message: $"Đã xóa lớp '{dto.Ma}'."));
        }
    }
}
