using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Models.DTOs;
using StudentPortal.Services;

namespace StudentPortal.Controllers
{
    // FR-30 Nhập danh sách sinh viên từ Excel (FR-28, FR-29 xuất file bổ sung ở Phase 4)
    [Authorize(Roles = "Admin,GiaoVu")]
    public class XuatNhapController : Controller
    {
        private const long KichThuocToiDa = 5 * 1024 * 1024;

        private readonly IXuatNhapService _xuatNhapService;
        private readonly ILogger<XuatNhapController> _logger;

        public XuatNhapController(IXuatNhapService xuatNhapService, ILogger<XuatNhapController> logger)
        {
            _xuatNhapService = xuatNhapService;
            _logger = logger;
        }

        // GET: /XuatNhap
        public IActionResult Index()
        {
            return View();
        }

        // GET: /XuatNhap/MauNhap - tải file Excel mẫu
        [HttpGet]
        public IActionResult MauNhap()
        {
            return File(_xuatNhapService.TaoFileMau(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauNhapSinhVien.xlsx");
        }

        // GET: /XuatNhap/DanhSach - trang xem trước danh sách cần xuất (FR-28)
        [HttpGet]
        public IActionResult DanhSach()
        {
            return View();
        }

        // GET: /XuatNhap/DanhSachData?maKhoa=&maKhoaHoc=&maLop=&trangThai=&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> DanhSachData([FromQuery] DanhSachFilterDto filter)
        {
            try
            {
                return Json(await _xuatNhapService.DanhSachSinhVienAsync(filter, User));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sinh viên cần xuất");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải danh sách." });
            }
        }

        // GET: /XuatNhap/XuatDanhSach?format=excel|pdf&keyword=&maKhoa=&... (FR-28)
        [HttpGet]
        public async Task<IActionResult> XuatDanhSach([FromQuery] DanhSachFilterDto filter, string format = "excel")
        {
            try
            {
                var file = await _xuatNhapService.XuatDanhSachAsync(filter, format == "pdf", User);
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

        // GET: /XuatNhap/DanhSachPhienNhap?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> DanhSachPhienNhap(int page = 1, int pageSize = 10)
        {
            try
            {
                return Json(await _xuatNhapService.DanhSachPhienNhapAsync(page, pageSize, User));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử nhập");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải lịch sử nhập." });
            }
        }

        // POST: /XuatNhap/Import (multipart/form-data, jQuery AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(KichThuocToiDa + 64 * 1024)]
        public async Task<IActionResult> Import(IFormFile? file)
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
                var data = await _xuatNhapService.NhapExcelAsync(stream, Path.GetFileName(file.FileName), User);
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

        // DELETE: /XuatNhap/XoaPhienNhap/5 (jQuery AJAX)
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaPhienNhap(long id)
        {
            try
            {
                var (ok, statusCode, message) = await _xuatNhapService.XoaPhienNhapAsync(id, User);
                return StatusCode(statusCode, new { success = ok, message, id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phiên nhập {Id}", id);
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xóa phiên nhập." });
            }
        }
    }
}
