using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;
        private readonly IScopeService _scope;

        public StatisticsService(AppDbContext context, IScopeService scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<StatisticsResultDto> GetAsync(StatisticsFilterVm filter)
        {
            // Áp phạm vi (FR-09) trước khi lọc; global query filter đã loại bản ghi xóa mềm
            var query = _scope.ApplyStudentScope(_context.SinhViens.AsNoTracking());

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
                .Select(g => new StatisticsRowDto
                {
                    MaKhoa = g.Key.MaKhoa,
                    TenKhoa = g.Key.TenKhoa,
                    MaKhoaHoc = g.Key.MaKhoaHoc,
                    MaLop = g.Key.MaLop,
                    TenLop = g.Key.TenLop,
                    DangHoc = g.Count(s => s.TrangThai == TrangThaiHocTap.DangHoc),
                    BaoLuu = g.Count(s => s.TrangThai == TrangThaiHocTap.BaoLuu),
                    ThoiHoc = g.Count(s => s.TrangThai == TrangThaiHocTap.ThoiHoc),
                    DaTotNghiep = g.Count(s => s.TrangThai == TrangThaiHocTap.DaTotNghiep),
                    Tong = g.Count()
                })
                .OrderBy(r => r.MaKhoa).ThenBy(r => r.MaKhoaHoc).ThenBy(r => r.MaLop)
                .ToListAsync();

            return new StatisticsResultDto
            {
                Rows = rows,
                TongCong = new StatisticsRowDto
                {
                    DangHoc = rows.Sum(r => r.DangHoc),
                    BaoLuu = rows.Sum(r => r.BaoLuu),
                    ThoiHoc = rows.Sum(r => r.ThoiHoc),
                    DaTotNghiep = rows.Sum(r => r.DaTotNghiep),
                    Tong = rows.Sum(r => r.Tong)
                }
            };
        }
    }
}
