using Microsoft.EntityFrameworkCore;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.Entities;
using StudentPortal.Repositories;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class ScopeService : IScopeService
    {
        private readonly IRepository<SinhVien> _sinhViens;
        private readonly IRepository<Lop> _lops;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScopeService(IRepository<SinhVien> sinhViens, IRepository<Lop> lops, IHttpContextAccessor httpContextAccessor)
        {
            _sinhViens = sinhViens;
            _lops = lops;
            _httpContextAccessor = httpContextAccessor;
        }

        // Đọc từ principal của request hiện tại; principal được tạo lại từ CSDL ở mỗi request
        // nên đổi vai trò/khoa phụ trách (FR-03) áp dụng từ yêu cầu kế tiếp
        public UserScope GetCurrent()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return new UserScope("", null, null, null);

            var vaiTro = user.IsInRole(Roles.Admin) ? Roles.Admin
                : user.IsInRole(Roles.CanBo) ? Roles.CanBo
                : user.IsInRole(Roles.SinhVien) ? Roles.SinhVien
                : null;

            return new UserScope(
                user.Identity.Name ?? "",
                vaiTro,
                user.FindFirst(AppClaimTypes.MaKhoaPhuTrach)?.Value,
                user.FindFirst(AppClaimTypes.MaSV)?.Value);
        }

        public IQueryable<SinhVien> ApplyStudentScope(IQueryable<SinhVien> query)
        {
            var scope = GetCurrent();
            if (scope.IsAdmin) return query;

            if (scope.IsCanBo)
            {
                var maKhoa = scope.MaKhoaPhuTrach;
                return query.Where(s => maKhoa != null && s.Lop!.MaKhoa == maKhoa);
            }
            if (scope.IsSinhVien)
            {
                var maSV = scope.MaSV;
                return query.Where(s => maSV != null && s.MaSV == maSV);
            }
            return query.Where(s => false);
        }

        public IQueryable<Lop> ApplyClassScope(IQueryable<Lop> query)
        {
            var scope = GetCurrent();
            if (scope.IsAdmin) return query;

            if (scope.IsCanBo)
            {
                var maKhoa = scope.MaKhoaPhuTrach;
                return query.Where(l => maKhoa != null && l.MaKhoa == maKhoa);
            }
            return query.Where(l => false);
        }

        public async Task EnsureStudentInScopeAsync(string maSV)
        {
            if (!await ApplyStudentScope(_sinhViens.QueryNoTracking()).AnyAsync(s => s.MaSV == maSV))
                throw new ScopeException();
        }

        public async Task EnsureClassInScopeAsync(string maLop)
        {
            if (!await ApplyClassScope(_lops.QueryNoTracking()).AnyAsync(l => l.MaLop == maLop))
                throw new ScopeException();
        }
    }
}
