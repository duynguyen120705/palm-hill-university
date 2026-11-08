using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly AppDbContext _context;
        private readonly IScopeService _scope;

        public CatalogService(AppDbContext context, IScopeService scope)
        {
            _context = context;
            _scope = scope;
        }

        public async Task<List<LuaChonDto>> LookupAsync(string type, string? maKhoa, string? maKhoaHoc)
        {
            // Global query filter đã loại danh mục xóa mềm
            switch (type?.ToLowerInvariant())
            {
                case "khoa":
                {
                    // Giáo vụ chỉ thấy khoa phụ trách
                    var scope = _scope.GetCurrent();
                    var khoaPhuTrach = scope.MaKhoaPhuTrach;
                    return await _context.Khoas.AsNoTracking()
                        .Where(k => scope.IsAdmin || (khoaPhuTrach != null && k.MaKhoa == khoaPhuTrach))
                        .OrderBy(k => k.MaKhoa)
                        .Select(k => new LuaChonDto { Ma = k.MaKhoa, Ten = k.TenKhoa })
                        .ToListAsync();
                }
                case "khoahoc":
                    return await _context.KhoaHocs.AsNoTracking()
                        .OrderBy(kh => kh.NamNhapHoc)
                        .Select(kh => new LuaChonDto { Ma = kh.MaKhoaHoc, Ten = kh.MaKhoaHoc + " (" + kh.NamNhapHoc + ")" })
                        .ToListAsync();
                case "lop":
                {
                    var query = _scope.ApplyClassScope(_context.Lops.AsNoTracking());
                    if (!string.IsNullOrWhiteSpace(maKhoa)) query = query.Where(l => l.MaKhoa == maKhoa);
                    if (!string.IsNullOrWhiteSpace(maKhoaHoc)) query = query.Where(l => l.MaKhoaHoc == maKhoaHoc);
                    return await query
                        .OrderBy(l => l.MaLop)
                        .Select(l => new LuaChonDto { Ma = l.MaLop, Ten = l.TenLop, MaKhoa = l.MaKhoa, MaKhoaHoc = l.MaKhoaHoc })
                        .ToListAsync();
                }
                default:
                    throw new NghiepVuException("Loại danh mục không hợp lệ.");
            }
        }
    }
}
