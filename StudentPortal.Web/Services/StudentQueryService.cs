using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class StudentQueryService : IStudentQueryService
    {
        private readonly AppDbContext _context;
        private readonly IScopeService _scope;

        public StudentQueryService(AppDbContext context, IScopeService scope)
        {
            _context = context;
            _scope = scope;
        }

        public IQueryable<SinhVien> BuildQuery(StudentFilter filter)
        {
            // Phạm vi (FR-09) trước; global query filter đã loại hồ sơ xóa mềm
            var query = _scope.ApplyStudentScope(_context.SinhViens.AsNoTracking());

            // Không phân biệt hoa thường và dấu ("nguyen" khớp "Nguyễn", "do" khớp "Đỗ")
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(s =>
                    EF.Functions.Collate(s.HoTen, TextHelper.CollationKhongDau).Contains(kw) ||
                    s.MaSV.Contains(kw));
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

        public async Task<PagedResultDto<SinhVienItemDto>> SearchAsync(StudentFilter filter)
        {
            return await BuildQuery(filter)
                .OrderBy(s => s.MaSV)
                .ToPagedResultAsync(filter.Page, filter.PageSize, s => new SinhVienItemDto
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    GioiTinh = s.GioiTinh,
                    GioiTinhText = MaTrangThai.TenGioiTinh(s.GioiTinh),
                    MaLop = s.MaLop,
                    TenLop = s.Lop!.TenLop,
                    SDT = s.SDT,
                    Email = s.Email,
                    TrangThai = s.TrangThai,
                    TrangThaiText = MaTrangThai.TenTrangThai(s.TrangThai)
                });
        }
    }
}
