using Microsoft.EntityFrameworkCore;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Repositories;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IRepository<SinhVien> _sinhViens;
        private readonly IScopeService _scope;

        public StatisticsService(IRepository<SinhVien> sinhViens, IScopeService scope)
        {
            _sinhViens = sinhViens;
            _scope = scope;
        }

        private sealed class DongNhom
        {
            public string Ma { get; set; } = string.Empty;
            public string Ten { get; set; } = string.Empty;
            public TrangThaiHocTap TrangThai { get; set; }
        }

        public async Task<StatisticsResultDto> GetAsync(StatisticsFilterVm filter)
        {
            var tenNhom = filter.NhomTheo switch
            {
                NhomThongKe.Khoa => "Khoa",
                NhomThongKe.KhoaHoc => "Khóa",
                NhomThongKe.Lop => "Lớp",
                _ => throw new NghiepVuException("Lựa chọn nhóm theo không hợp lệ.")
            };

            // Áp phạm vi (FR-09) trước khi lọc; global query filter đã loại bản ghi xóa mềm
            var query = _scope.ApplyStudentScope(_sinhViens.QueryNoTracking());

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
                query = query.Where(s => s.Lop!.MaKhoa == filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc))
                query = query.Where(s => s.Lop!.MaKhoaHoc == filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop))
                query = query.Where(s => s.MaLop == filter.MaLop);
            if (filter.TrangThai.HasValue)
                query = query.Where(s => s.TrangThai == filter.TrangThai.Value);

            IQueryable<DongNhom> nguon = filter.NhomTheo switch
            {
                NhomThongKe.Khoa => query.Select(s => new DongNhom { Ma = s.Lop!.MaKhoa, Ten = s.Lop.Khoa!.TenKhoa, TrangThai = s.TrangThai }),
                NhomThongKe.KhoaHoc => query.Select(s => new DongNhom { Ma = s.Lop!.MaKhoaHoc, Ten = s.Lop.MaKhoaHoc, TrangThai = s.TrangThai }),
                _ => query.Select(s => new DongNhom { Ma = s.MaLop, Ten = s.MaLop, TrangThai = s.TrangThai })
            };

            // GROUP BY tại CSDL, đếm theo từng trạng thái
            var rows = await nguon
                .GroupBy(x => new { x.Ma, x.Ten })
                .OrderBy(g => g.Key.Ma)
                .Select(g => new StatisticsRowDto
                {
                    Ten = g.Key.Ten,
                    DangHoc = g.Count(x => x.TrangThai == TrangThaiHocTap.DangHoc),
                    BaoLuu = g.Count(x => x.TrangThai == TrangThaiHocTap.BaoLuu),
                    ThoiHoc = g.Count(x => x.TrangThai == TrangThaiHocTap.ThoiHoc),
                    DaTotNghiep = g.Count(x => x.TrangThai == TrangThaiHocTap.DaTotNghiep),
                    Tong = g.Count()
                })
                .ToListAsync();

            return new StatisticsResultDto
            {
                TenNhom = tenNhom,
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
