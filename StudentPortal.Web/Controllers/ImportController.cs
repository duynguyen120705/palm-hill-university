using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers
{
    // FR-30 Nhập danh sách sinh viên từ Excel
    [Authorize(Roles = Roles.AdminOrCanBo)]
    public class ImportController : Controller
    {
        private const long KichThuocToiDa = 5 * 1024 * 1024;

        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        // GET: /Import
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Import/Template - tải file Excel mẫu BM02
        [HttpGet]
        public IActionResult Template()
        {
            return File(_importService.CreateTemplate(), ReportFileBuilder.ExcelContentType, "MauNhapSinhVien.xlsx");
        }

        // POST: /Import/Upload (multipart/form-data, Ajax); trả báo cáo BM05
        [HttpPost]
        [RequestSizeLimit(KichThuocToiDa + 64 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("Vui lòng chọn file Excel."));
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.Fail("Chỉ chấp nhận file .xlsx."));
            if (file.Length > KichThuocToiDa)
                return BadRequest(ApiResponse.Fail("File vượt quá 5 MB."));

            await using var stream = file.OpenReadStream();
            var data = await _importService.ImportAsync(stream, Path.GetFileName(file.FileName));
            return Ok(ApiResponse.Ok(data));
        }
    }
}
