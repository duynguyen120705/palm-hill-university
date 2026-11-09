using Microsoft.EntityFrameworkCore;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Repositories;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class StudentQueryService : IStudentQueryService
    {
        private readonly IRepository<SinhVien> _sinhViens;
        private readonly IScopeService _scope;

        public StudentQueryService(IRepository<SinhVien> sinhViens, IScopeService scope)
        {
            _sinhViens = sinhViens;
            _scope = scope;
        }

        public IQueryable<SinhVien> BuildQuery(StudentFilter filter)
        {
            // Phạm vi (FR-09) trước; global query filter đã loại hồ sơ xóa mềm
            var query = _scope.ApplyStudentScope(_sinhViens.QueryNoTracking());

            // Không phân biệt hoa thường và dấu: chuẩn hóa từ khóa rồi tìm theo HoTenKhongDau (FR-22)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kwKhongDau = TextHelper.BoDau(filter.Keyword);
                query = query.Where(s => s.HoTenKhongDau.Contains(kwKhongDau) || s.MaSV.Contains(kwKhongDau));
            }

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
                query = query.Where(s => s.Lop!.MaKhoa == filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc))
                query = query.Where(s => s.Lop!.MaKhoaHoc == filter.MaKhoaHoc);
            if (!string.IsNullOrWhiteSpace(filter.MaLop))
                query = query.Where(s => s.MaLop == filter.MaLop);
            if (filter.TrangThai.HasValue)
                query = query.Where(s => s.TrangThai == filter.TrangThai.Value);

            return query;
        }

        public async Task<PagedResult<StudentListItemDto>> SearchAsync(StudentFilter filter)
        {
            return await BuildQuery(filter)
                .OrderBy(s => s.MaSV)
                .ToPagedResultAsync(filter.Page, filter.PageSize, s => new StudentListItemDto
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    MaLop = s.MaLop,
                    TrangThai = s.TrangThai,
                    TrangThaiText = s.TrangThai.TenHienThi(),
                    TrangThaiTaiKhoanText = s.TaiKhoan != null ? s.TaiKhoan.TrangThaiTaiKhoan.TenHienThi() : "Chưa có tài khoản"
                });
        }
    }
}
