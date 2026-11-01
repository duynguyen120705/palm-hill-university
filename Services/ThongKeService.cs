using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    public class ThongKeService : IThongKeService
    {
        // Mã trạng thái theo dữ liệu seed và SinhVienAjaxController
        // (Models/Enums.cs đang lệch 2/3 - chờ nhóm chốt)
        private const int DangHoc = 0, BaoLuu = 1, ThoiHoc = 2, DaTotNghiep = 3;

        private readonly ApplicationDbContext _context;
        private readonly IPhamViDuLieuService _phamVi;
        private readonly INhatKyService _nhatKy;

        public ThongKeService(ApplicationDbContext context, IPhamViDuLieuService phamVi, INhatKyService nhatKy)
        {
            _context = context;
            _phamVi = phamVi;
            _nhatKy = nhatKy;
        }

        // FR-29: tổng hợp lại số liệu theo tiêu chí trong phạm vi được phép rồi tạo file
        public async Task<FileXuat?> XuatAsync(ThongKeFilterDto filter, bool pdf, ClaimsPrincipal user)
        {
            var kq = await ThongKeAsync(filter, user);
            if (kq.Rows.Count == 0) return null;

            string[] cot = { "Khoa", "Khóa", "Lớp", "Đang học", "Bảo lưu", "Đã tốt nghiệp", "Thôi học", "Tổng" };
            var dong = kq.Rows
                .Select(r => new object?[] { r.TenKhoa, r.MaKhoaHoc, r.TenLop + " (" + r.MaLop + ")", r.DangHoc, r.BaoLuu, r.DaTotNghiep, r.ThoiHoc, r.Tong })
                .ToList();
            var t = kq.TongCong;
            dong.Add(new object?[] { "TỔNG CỘNG", "", kq.Rows.Count + " lớp", t.DangHoc, t.BaoLuu, t.DaTotNghiep, t.ThoiHoc, t.Tong });

            var phamVi = XuatNhapService.MoTaPhamVi(filter, user);
            var tieuDe = "BÁO CÁO THỐNG KÊ SINH VIÊN";
            var moTa = $"{phamVi} | Xuất lúc {DateTime.Now:dd/MM/yyyy HH:mm} bởi {user.Identity?.Name}";
            var tenFile = $"ThongKeSinhVien_{DateTime.Now:yyyyMMdd_HHmm}";

            var file = pdf
                ? new FileXuat(BaoCaoFileBuilder.TaoPdf(tieuDe, moTa, cot, dong), BaoCaoFileBuilder.PdfContentType, tenFile + ".pdf")
                : new FileXuat(BaoCaoFileBuilder.TaoExcel(tieuDe, moTa, cot, dong), BaoCaoFileBuilder.ExcelContentType, tenFile + ".xlsx");

            await _nhatKy.GhiAsync(user.Identity?.Name ?? "unknown", pdf ? "XuatThongKePdf" : "XuatThongKeExcel",
                "ThongKe", XuatNhapService.CatChuoi(phamVi, 100));
            return file;
        }

        public async Task<ThongKeResultDto> ThongKeAsync(ThongKeFilterDto filter, ClaimsPrincipal user)
        {
            // Áp phạm vi (FR-09) trước khi lọc; global query filter đã loại bản ghi xóa mềm
            var query = await _phamVi.LocSinhVienAsync(_context.SinhViens.AsNoTracking(), user);

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
                query = query.Where(s => s.Lop!.MaKhoa == filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc))
                query = query.Where(s => s.Lop!.MaKhoaHoc == filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop))
                query = query.Where(s => s.MaLop == filter.MaLop);
            if (filter.TrangThai.HasValue)
                query = query.Where(s => s.TrangThai == filter.TrangThai.Value);

            // GROUP BY tại CSDL, đếm theo từng trạng thái
            var rows = await query
                .GroupBy(s => new { s.Lop!.MaKhoa, s.Lop.Khoa!.TenKhoa, s.Lop.MaKhoaHoc, s.MaLop, s.Lop.TenLop })
                .Select(g => new ThongKeRowDto
                {
                    MaKhoa = g.Key.MaKhoa,
                    TenKhoa = g.Key.TenKhoa,
                    MaKhoaHoc = g.Key.MaKhoaHoc,
                    MaLop = g.Key.MaLop,
                    TenLop = g.Key.TenLop,
                    DangHoc = g.Count(s => s.TrangThai == DangHoc),
                    BaoLuu = g.Count(s => s.TrangThai == BaoLuu),
                    ThoiHoc = g.Count(s => s.TrangThai == ThoiHoc),
                    DaTotNghiep = g.Count(s => s.TrangThai == DaTotNghiep),
                    Tong = g.Count()
                })
                .OrderBy(r => r.MaKhoa).ThenBy(r => r.MaKhoaHoc).ThenBy(r => r.MaLop)
                .ToListAsync();

            return new ThongKeResultDto
            {
                Rows = rows,
                TongCong = new ThongKeRowDto
                {
                    DangHoc = rows.Sum(r => r.DangHoc),
                    BaoLuu = rows.Sum(r => r.BaoLuu),
                    ThoiHoc = rows.Sum(r => r.ThoiHoc),
                    DaTotNghiep = rows.Sum(r => r.DaTotNghiep),
                    Tong = rows.Sum(r => r.Tong)
                }
            };
        }

        public async Task<BoLocThongKeDto> LayBoLocAsync(ClaimsPrincipal user)
        {
            var lopQuery = await _phamVi.LocLopAsync(_context.Lops.AsNoTracking(), user);
            var lops = await lopQuery
                .OrderBy(l => l.MaLop)
                .Select(l => new LuaChonDto { Ma = l.MaLop, Ten = l.TenLop, MaKhoa = l.MaKhoa, MaKhoaHoc = l.MaKhoaHoc })
                .ToListAsync();

            var isAdmin = user.IsInRole("Admin");
            var maKhoas = lops.Select(l => l.MaKhoa).Distinct().ToList();
            var khoas = await _context.Khoas.AsNoTracking()
                .Where(k => isAdmin || maKhoas.Contains(k.MaKhoa))
                .OrderBy(k => k.MaKhoa)
                .Select(k => new LuaChonDto { Ma = k.MaKhoa, Ten = k.TenKhoa })
                .ToListAsync();

            var khoaHocs = await _context.KhoaHocs.AsNoTracking()
                .OrderBy(kh => kh.NamNhapHoc)
                .Select(kh => new LuaChonDto { Ma = kh.MaKhoaHoc, Ten = kh.MaKhoaHoc + " (" + kh.NamNhapHoc + ")" })
                .ToListAsync();

            return new BoLocThongKeDto { Khoas = khoas, KhoaHocs = khoaHocs, Lops = lops };
        }
    }
}
