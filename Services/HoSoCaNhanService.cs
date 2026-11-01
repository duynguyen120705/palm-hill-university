using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    public class HoSoCaNhanService : IHoSoCaNhanService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INhatKyService _nhatKy;

        public HoSoCaNhanService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INhatKyService nhatKy)
        {
            _context = context;
            _userManager = userManager;
            _nhatKy = nhatKy;
        }

        public async Task<HoSoCaNhanDto> LayHoSoAsync(ClaimsPrincipal user)
        {
            var taiKhoan = await LayTaiKhoanAsync(user);
            var roles = await _userManager.GetRolesAsync(taiKhoan);
            var vaiTro = roles.Contains("Admin") ? "Admin" : roles.Contains("GiaoVu") ? "GiaoVu" : "SinhVien";

            var hoSo = new HoSoCaNhanDto
            {
                UserName = taiKhoan.UserName!,
                HoTen = taiKhoan.HoTen,
                VaiTro = vaiTro,
                MaKhoaPhuTrach = taiKhoan.MaKhoaPhuTrach,
                TrangThaiTaiKhoanText = taiKhoan.TrangThaiTaiKhoan == ITaiKhoanService.HoatDong ? "Hoạt động" : "Bị khóa"
            };

            if (taiKhoan.MaKhoaPhuTrach != null)
                hoSo.TenKhoaPhuTrach = await _context.Khoas.AsNoTracking()
                    .Where(k => k.MaKhoa == taiKhoan.MaKhoaPhuTrach).Select(k => k.TenKhoa).FirstOrDefaultAsync();

            // Admin/Giáo vụ chỉ xem thông tin tài khoản; sinh viên xem hồ sơ của mình (FR-11)
            if (vaiTro == "SinhVien" && taiKhoan.MaSV != null)
            {
                hoSo.SinhVien = await TimHoSoSinhVienAsync(taiKhoan.MaSV);
                hoSo.HoTen = hoSo.SinhVien?.HoTen;
            }
            return hoSo;
        }

        public async Task<HoSoSinhVienDto> CapNhatLienHeAsync(CapNhatLienHeDto dto, ClaimsPrincipal user)
        {
            var taiKhoan = await LayTaiKhoanAsync(user);
            if (taiKhoan.MaSV == null || !await _userManager.IsInRoleAsync(taiKhoan, "SinhVien"))
                throw new NghiepVuException("Chỉ sinh viên được cập nhật thông tin liên hệ.", 403);

            // Quyền sở hữu: chỉ hồ sơ gắn với tài khoản đang đăng nhập (global filter loại hồ sơ đã xóa mềm)
            var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.MaSV == taiKhoan.MaSV)
                ?? throw new NghiepVuException("Không tìm thấy hồ sơ sinh viên.", 404);

            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
            if (email != null && await _context.SinhViens.IgnoreQueryFilters()
                    .AnyAsync(s => s.Email == email && s.MaSV != sv.MaSV))
                throw new NghiepVuException("Email đã được sinh viên khác sử dụng.");

            sv.SDT = string.IsNullOrWhiteSpace(dto.SDT) ? null : dto.SDT.Trim();
            sv.Email = email;
            sv.DiaChi = string.IsNullOrWhiteSpace(dto.DiaChi) ? null : dto.DiaChi.Trim();

            await using var tx = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();
            // Chỉ ghi định danh, không ghi giá trị liên hệ (NFR-07)
            await _nhatKy.GhiAsync(taiKhoan.UserName!, "CapNhatLienHe", "SinhVien", sv.MaSV);
            await tx.CommitAsync();

            return (await TimHoSoSinhVienAsync(sv.MaSV))!;
        }

        public async Task DoiMatKhauAsync(DoiMatKhauDto dto, ClaimsPrincipal user)
        {
            var taiKhoan = await LayTaiKhoanAsync(user);

            if (!await _userManager.CheckPasswordAsync(taiKhoan, dto.MatKhauHienTai))
                throw new NghiepVuException("Mật khẩu hiện tại không đúng.");
            if (dto.MatKhauMoi == dto.MatKhauHienTai)
                throw new NghiepVuException("Mật khẩu mới phải khác mật khẩu hiện tại.");

            // ChangePasswordAsync kiểm tra chính sách mật khẩu, lưu hash và đổi SecurityStamp
            var result = await _userManager.ChangePasswordAsync(taiKhoan, dto.MatKhauHienTai, dto.MatKhauMoi);
            if (!result.Succeeded)
                throw new NghiepVuException(string.Join(" ", result.Errors.Select(e => e.Description)));

            await _nhatKy.GhiAsync(taiKhoan.UserName!, "DoiMatKhau", "TaiKhoan", taiKhoan.UserName!);
        }

        private async Task<ApplicationUser> LayTaiKhoanAsync(ClaimsPrincipal user) =>
            await _userManager.GetUserAsync(user) ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", 401);

        private Task<HoSoSinhVienDto?> TimHoSoSinhVienAsync(string maSV) =>
            _context.SinhViens.AsNoTracking()
                .Where(s => s.MaSV == maSV)
                .Select(s => new HoSoSinhVienDto
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    GioiTinhText = s.GioiTinh == 1 ? "Nam" : (s.GioiTinh == 0 ? "Nữ" : "Khác"),
                    MaLop = s.MaLop,
                    TenLop = s.Lop!.TenLop,
                    TenKhoa = s.Lop.Khoa!.TenKhoa,
                    MaKhoaHoc = s.Lop.MaKhoaHoc,
                    TrangThaiText = XuatNhapService.TenTrangThai(s.TrangThai),
                    SDT = s.SDT,
                    Email = s.Email,
                    DiaChi = s.DiaChi
                })
                .FirstOrDefaultAsync();
    }
}
