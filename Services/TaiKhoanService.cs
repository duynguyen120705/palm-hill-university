using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TaiKhoanService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> TaoTaiKhoanSinhVienAsync(string maSV, string? email, string matKhau, bool khoa = false)
        {
            if (matKhau.Length < 8 || !matKhau.Any(char.IsLetter) || !matKhau.Any(char.IsDigit))
                throw new NghiepVuException("Mật khẩu tối thiểu 8 ký tự, gồm cả chữ và số.");
            if (await _userManager.FindByNameAsync(maSV) != null)
                throw new NghiepVuException("Tên đăng nhập (Mã SV) đã tồn tại.");

            var user = new ApplicationUser
            {
                UserName = maSV,
                Email = email,
                MaSV = maSV,
                TrangThaiTaiKhoan = ITaiKhoanService.HoatDong,
                LockoutEnabled = true
            };
            if (khoa) DanhDauKhoa(user);

            KiemTra(await _userManager.CreateAsync(user, matKhau));
            KiemTra(await _userManager.AddToRoleAsync(user, "SinhVien"));
            return user;
        }

        public Task<ApplicationUser?> TimTheoMaSVAsync(string maSV) =>
            _userManager.Users.FirstOrDefaultAsync(u => u.MaSV == maSV);

        public async Task DatLaiMatKhauAsync(ApplicationUser user, string matKhauMoi)
        {
            if (matKhauMoi.Length < 8 || !matKhauMoi.Any(char.IsLetter) || !matKhauMoi.Any(char.IsDigit))
                throw new NghiepVuException("Mật khẩu tối thiểu 8 ký tự, gồm cả chữ và số.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            KiemTra(await _userManager.ResetPasswordAsync(user, token, matKhauMoi));
        }

        public async Task KhoaAsync(ApplicationUser user)
        {
            DanhDauKhoa(user);
            KiemTra(await _userManager.UpdateAsync(user));
            // Phiên đăng nhập hiện có mất hiệu lực từ yêu cầu kế tiếp
            KiemTra(await _userManager.UpdateSecurityStampAsync(user));
        }

        public async Task MoKhoaAsync(ApplicationUser user)
        {
            user.TrangThaiTaiKhoan = ITaiKhoanService.HoatDong;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            KiemTra(await _userManager.UpdateAsync(user));
        }

        // Khóa theo cùng cách với dữ liệu seed: trạng thái + LockoutEnd để Identity chặn đăng nhập
        private static void DanhDauKhoa(ApplicationUser user)
        {
            user.TrangThaiTaiKhoan = ITaiKhoanService.BiKhoa;
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        private static void KiemTra(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new NghiepVuException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
