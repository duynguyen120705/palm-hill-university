using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    public class PhamViDuLieuService : IPhamViDuLieuService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Scoped theo request: đọc tài khoản một lần, luôn lấy khoa phụ trách mới nhất từ CSDL
        // nên thay đổi khoa (FR-03) áp dụng từ yêu cầu kế tiếp
        private ApplicationUser? _taiKhoan;
        private bool _daDoc;

        public PhamViDuLieuService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IQueryable<SinhVien>> LocSinhVienAsync(IQueryable<SinhVien> query, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin")) return query;

            var taiKhoan = await LayTaiKhoanAsync(user);
            if (user.IsInRole("GiaoVu"))
            {
                var maKhoa = taiKhoan?.MaKhoaPhuTrach;
                return query.Where(s => maKhoa != null && s.Lop!.MaKhoa == maKhoa);
            }
            if (user.IsInRole("SinhVien"))
            {
                var maSV = taiKhoan?.MaSV;
                return query.Where(s => maSV != null && s.MaSV == maSV);
            }
            return query.Where(s => false);
        }

        public async Task<IQueryable<Lop>> LocLopAsync(IQueryable<Lop> query, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin")) return query;

            if (user.IsInRole("GiaoVu"))
            {
                var maKhoa = (await LayTaiKhoanAsync(user))?.MaKhoaPhuTrach;
                return query.Where(l => maKhoa != null && l.MaKhoa == maKhoa);
            }
            return query.Where(l => false);
        }

        public async Task<bool> CoQuyenVoiSinhVienAsync(string maSV, ClaimsPrincipal user)
        {
            var query = await LocSinhVienAsync(_context.SinhViens.AsNoTracking(), user);
            return await query.AnyAsync(s => s.MaSV == maSV);
        }

        public async Task<bool> CoQuyenVoiLopAsync(string maLop, ClaimsPrincipal user)
        {
            var query = await LocLopAsync(_context.Lops.AsNoTracking(), user);
            return await query.AnyAsync(l => l.MaLop == maLop);
        }

        public async Task<string?> LayKhoaPhuTrachAsync(ClaimsPrincipal user)
        {
            if (!user.IsInRole("GiaoVu")) return null;
            return (await LayTaiKhoanAsync(user))?.MaKhoaPhuTrach;
        }

        private async Task<ApplicationUser?> LayTaiKhoanAsync(ClaimsPrincipal user)
        {
            if (!_daDoc)
            {
                _taiKhoan = await _userManager.GetUserAsync(user);
                _daDoc = true;
            }
            return _taiKhoan;
        }
    }
}
