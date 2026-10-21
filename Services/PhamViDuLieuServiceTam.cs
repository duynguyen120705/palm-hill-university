using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    // BẢN TẠM (stub) - thay bằng bản chính thức của phân hệ Tài khoản khi hoàn thành
    public class PhamViDuLieuServiceTam : IPhamViDuLieuService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PhamViDuLieuServiceTam(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IQueryable<SinhVien>> LocSinhVienAsync(IQueryable<SinhVien> query, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin")) return query;

            var taiKhoan = await _userManager.GetUserAsync(user);
            if (user.IsInRole("GiaoVu"))
            {
                var maKhoa = taiKhoan?.MaKhoaPhuTrach;
                return query.Where(s => s.Lop!.MaKhoa == maKhoa);
            }

            var maSV = taiKhoan?.MaSV;
            return query.Where(s => maSV != null && s.MaSV == maSV);
        }

        public async Task<IQueryable<Lop>> LocLopAsync(IQueryable<Lop> query, ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin")) return query;

            if (user.IsInRole("GiaoVu"))
            {
                var maKhoa = (await _userManager.GetUserAsync(user))?.MaKhoaPhuTrach;
                return query.Where(l => l.MaKhoa == maKhoa);
            }

            return query.Where(l => false);
        }
    }
}
