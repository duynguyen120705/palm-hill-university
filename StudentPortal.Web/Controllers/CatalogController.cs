using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // Dữ liệu cho dropdown Khoa/Khóa/Lớp, đã giới hạn theo phạm vi dữ liệu
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        // GET: /Catalog/Lookup?type=khoa|khoahoc|lop&maKhoa=&maKhoaHoc=
        [HttpGet]
        public async Task<IActionResult> Lookup(string type, string? maKhoa, string? maKhoaHoc)
        {
            try
            {
                var data = await _catalogService.LookupAsync(type, maKhoa, maKhoaHoc);
                return Json(new { success = true, data });
            }
            catch (NghiepVuException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
        }
    }
}
