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
        private readonly ILogger<ImportController> _logger;

        public ImportController(IImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
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

        // POST: /Import/Upload (multipart/form-data, Ajax)
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

            try
            {
                await using var stream = file.OpenReadStream();
                var data = await _importService.ImportAsync(stream, Path.GetFileName(file.FileName));
                return Ok(ApiResponse.Ok(data,
                    $"Nhập xong: {data.SoThemMoi} thêm mới, {data.SoCapNhat} cập nhật, {data.SoLoi} lỗi."));
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nhập file Excel");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse.Fail("Không đọc được file. Hãy dùng đúng file mẫu .xlsx."));
            }
        }
    }
}
