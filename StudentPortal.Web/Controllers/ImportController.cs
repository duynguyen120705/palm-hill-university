using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
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

        // POST: /Import/Upload (multipart/form-data, jQuery AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(KichThuocToiDa + 64 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Vui lòng chọn file Excel." });
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Chỉ chấp nhận file .xlsx." });
            if (file.Length > KichThuocToiDa)
                return BadRequest(new { success = false, message = "File vượt quá 5 MB." });

            try
            {
                await using var stream = file.OpenReadStream();
                var data = await _importService.ImportAsync(stream, Path.GetFileName(file.FileName));
                return Ok(new
                {
                    success = true,
                    message = $"Nhập xong: {data.SoThemMoi} thêm mới, {data.SoCapNhat} cập nhật, {data.SoLoi} lỗi.",
                    data
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nhập file Excel");
                return StatusCode(500, new { success = false, message = "Không đọc được file. Hãy dùng đúng file mẫu .xlsx." });
            }
        }

        // GET: /Import/Sessions?page=1&pageSize=10 - lịch sử nhập
        [HttpGet]
        public async Task<IActionResult> Sessions(int page = 1, int pageSize = 10)
        {
            try
            {
                return Json(await _importService.ListSessionsAsync(page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử nhập");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải lịch sử nhập." });
            }
        }

        // DELETE: /Import/DeleteSession/5 (jQuery AJAX)
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(long id)
        {
            try
            {
                await _importService.DeleteSessionAsync(id);
                return Ok(new { success = true, message = $"Đã xóa phiên nhập #{id}.", id });
            }
            catch (NghiepVuException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message, id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phiên nhập {Id}", id);
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xóa phiên nhập." });
            }
        }
    }
}
