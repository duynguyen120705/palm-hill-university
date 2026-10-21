using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Controllers
{
    public class SinhVienAjaxController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SinhVienAjaxController> _logger;

        public SinhVienAjaxController(ApplicationDbContext context, ILogger<SinhVienAjaxController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /SinhVienAjax
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================================
        // 1. API: Lấy danh sách phân trang (Hỗ trợ Skip/Take EF Core & XMLHttpRequest / jQuery)
        // GET: /SinhVienAjax/GetPagedData?page=1&pageSize=10&keyword=...
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetPagedData(int page = 1, int pageSize = 10, string? keyword = null, string? maLop = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.SinhViens
                    .AsNoTracking()
                    .Include(s => s.Lop)
                    .AsQueryable();

                // Lọc theo từ khóa (MaSV, HoTen, Email, SDT)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(s => s.MaSV.ToLower().Contains(kw) ||
                                             s.HoTen.ToLower().Contains(kw) ||
                                             (s.Email != null && s.Email.ToLower().Contains(kw)) ||
                                             (s.SDT != null && s.SDT.Contains(kw)));
                }

                // Lọc theo lớp học
                if (!string.IsNullOrWhiteSpace(maLop))
                {
                    query = query.Where(s => s.MaLop == maLop);
                }

                // Tính tổng số lượng bản ghi sau khi lọc
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // EF Core Skip / Take: Kỹ thuật phân trang chuẩn tại Database (Mục 3)
                var items = await query
                    .OrderByDescending(s => s.NgayTao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SinhVienItemDto
                    {
                        MaSV = s.MaSV,
                        HoTen = s.HoTen,
                        NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                        GioiTinh = s.GioiTinh,
                        GioiTinhText = s.GioiTinh == 1 ? "Nam" : (s.GioiTinh == 0 ? "Nữ" : "Khác"),
                        MaLop = s.MaLop,
                        TenLop = s.Lop != null ? s.Lop.TenLop : s.MaLop,
                        SDT = s.SDT ?? "Chưa có",
                        Email = s.Email ?? "Chưa có",
                        DiaChi = s.DiaChi ?? "Chưa có",
                        TrangThai = s.TrangThai,
                        TrangThaiText = s.TrangThai == 0 ? "Đang học" :
                                       (s.TrangThai == 1 ? "Bảo lưu" :
                                       (s.TrangThai == 2 ? "Thôi học" : "Đã tốt nghiệp")),
                        NgayTaoFormatted = s.NgayTao.ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToListAsync();

                return Json(new PagedResultDto<SinhVienItemDto>
                {
                    Success = true,
                    Items = items,
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Message = "Tải dữ liệu thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn GetPagedData");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi tải dữ liệu: " + ex.Message });
            }
        }

        // =========================================================================
        // 2. API: Tìm kiếm tức thì phục vụ fetch() API (Mục 4)
        // GET: /SinhVienAjax/Search?keyword=...
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Search(string? keyword)
        {
            try
            {
                var query = _context.SinhViens
                    .AsNoTracking()
                    .Include(s => s.Lop)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(s => s.MaSV.ToLower().Contains(kw) ||
                                             s.HoTen.ToLower().Contains(kw) ||
                                             (s.Email != null && s.Email.ToLower().Contains(kw)));
                }

                var results = await query
                    .OrderByDescending(s => s.NgayTao)
                    .Take(15) // Giới hạn 15 kết quả gợi ý nhanh
                    .Select(s => new SinhVienItemDto
                    {
                        MaSV = s.MaSV,
                        HoTen = s.HoTen,
                        NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                        GioiTinhText = s.GioiTinh == 1 ? "Nam" : (s.GioiTinh == 0 ? "Nữ" : "Khác"),
                        MaLop = s.MaLop,
                        TenLop = s.Lop != null ? s.Lop.TenLop : s.MaLop,
                        Email = s.Email ?? "Chưa có",
                        SDT = s.SDT ?? "Chưa có",
                        TrangThaiText = s.TrangThai == 0 ? "Đang học" :
                                       (s.TrangThai == 1 ? "Bảo lưu" :
                                       (s.TrangThai == 2 ? "Thôi học" : "Đã tốt nghiệp"))
                    })
                    .ToListAsync();

                return Json(new { success = true, count = results.Count, data = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm Search");
                return StatusCode(500, new { success = false, message = "Lỗi khi tìm kiếm: " + ex.Message });
            }
        }

        // =========================================================================
        // 3. API: Tạo mới Sinh viên phục vụ jQuery AJAX POST (Mục 2)
        // POST: /SinhVienAjax/Create
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SinhVienCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ!", errors });
                }

                // Kiểm tra trùng mã sinh viên
                var isDuplicate = await _context.SinhViens
                    .IgnoreQueryFilters()
                    .AnyAsync(s => s.MaSV == dto.MaSV);

                if (isDuplicate)
                {
                    return BadRequest(new { success = false, message = $"Mã sinh viên '{dto.MaSV}' đã tồn tại trong hệ thống!" });
                }

                // Kiểm tra lớp tồn tại
                var lopExists = await _context.Lops.AnyAsync(l => l.MaLop == dto.MaLop);
                if (!lopExists)
                {
                    return BadRequest(new { success = false, message = $"Mã lớp '{dto.MaLop}' không tồn tại!" });
                }

                var sinhVien = new SinhVien
                {
                    MaSV = dto.MaSV.Trim().ToUpper(),
                    HoTen = dto.HoTen.Trim(),
                    NgaySinh = dto.NgaySinh,
                    GioiTinh = dto.GioiTinh,
                    MaLop = dto.MaLop,
                    SDT = dto.SDT?.Trim(),
                    Email = dto.Email?.Trim().ToLower(),
                    DiaChi = dto.DiaChi?.Trim(),
                    TrangThai = dto.TrangThai,
                    DaXoa = false,
                    NgayTao = DateTime.UtcNow,
                    NgayCapNhat = DateTime.UtcNow
                };

                _context.SinhViens.Add(sinhVien);
                await _context.SaveChangesAsync();

                // Lấy thông tin lớp để trả về
                var tenLop = (await _context.Lops.FindAsync(sinhVien.MaLop))?.TenLop ?? sinhVien.MaLop;

                return Ok(new
                {
                    success = true,
                    message = $"Thêm mới sinh viên [{sinhVien.MaSV}] - {sinhVien.HoTen} thành công!",
                    data = new SinhVienItemDto
                    {
                        MaSV = sinhVien.MaSV,
                        HoTen = sinhVien.HoTen,
                        NgaySinhFormatted = sinhVien.NgaySinh.ToString("dd/MM/yyyy"),
                        GioiTinh = sinhVien.GioiTinh,
                        GioiTinhText = sinhVien.GioiTinh == 1 ? "Nam" : (sinhVien.GioiTinh == 0 ? "Nữ" : "Khác"),
                        MaLop = sinhVien.MaLop,
                        TenLop = tenLop,
                        SDT = sinhVien.SDT ?? "Chưa có",
                        Email = sinhVien.Email ?? "Chưa có",
                        DiaChi = sinhVien.DiaChi ?? "Chưa có",
                        TrangThai = sinhVien.TrangThai,
                        TrangThaiText = sinhVien.TrangThai == 0 ? "Đang học" :
                                       (sinhVien.TrangThai == 1 ? "Bảo lưu" :
                                       (sinhVien.TrangThai == 2 ? "Thôi học" : "Đã tốt nghiệp")),
                        NgayTaoFormatted = sinhVien.NgayTao.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo mới sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi thêm mới: " + ex.Message });
            }
        }

        // =========================================================================
        // 4. API: Xóa Sinh viên (Soft Delete) phục vụ jQuery AJAX DELETE/POST (Mục 2)
        // POST hoặc DELETE: /SinhVienAjax/Delete
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] string maSV)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maSV))
                {
                    return BadRequest(new { success = false, message = "Mã sinh viên không hợp lệ!" });
                }

                var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(s => s.MaSV == maSV);
                if (sinhVien == null)
                {
                    return NotFound(new { success = false, message = $"Không tìm thấy sinh viên có mã '{maSV}'!" });
                }

                // Thực hiện Soft Delete an toàn
                sinhVien.DaXoa = true;
                sinhVien.NgayXoa = DateTime.UtcNow;
                sinhVien.NguoiXoa = "Admin_AjaxDemo";
                sinhVien.NgayCapNhat = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã xóa sinh viên [{maSV}] - {sinhVien.HoTen} thành công!",
                    maSV = maSV
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sinh viên");
                return StatusCode(500, new { success = false, message = "Lỗi máy chủ khi xóa: " + ex.Message });
            }
        }

        // =========================================================================
        // 5. API: Lấy danh mục lớp học nạp vào Dropdown Modal
        // GET: /SinhVienAjax/GetLopOptions
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetLopOptions()
        {
            try
            {
                var lops = await _context.Lops
                    .AsNoTracking()
                    .OrderBy(l => l.TenLop)
                    .Select(l => new LopOptionDto
                    {
                        MaLop = l.MaLop,
                        TenLop = $"{l.TenLop} ({l.MaLop})"
                    })
                    .ToListAsync();

                return Json(lops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

