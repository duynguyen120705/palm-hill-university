using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLog;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(AppDbContext context, UserManager<ApplicationUser> userManager,
            IAuditLogService auditLog, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _auditLog = auditLog;
            _httpContextAccessor = httpContextAccessor;
        }

        // ===================== Phương thức chính (mục 3.2.1.4) =====================

        public Task<Result> CreateStudentAccountAsync(string maSV, string password) =>
            ThucHienAsync(async () =>
            {
                KiemTraChinhSachMatKhau(password);
                if (await _userManager.FindByNameAsync(maSV) != null)
                    throw new NghiepVuException("Tên đăng nhập (Mã SV) đã tồn tại.", StatusCodes.Status409Conflict);

                var user = new ApplicationUser
                {
                    UserName = maSV,
                    MaSV = maSV,
                    TrangThaiTaiKhoan = TrangThaiTaiKhoan.HoatDong,
                    LockoutEnabled = true
                };
                KiemTraIdentity(await _userManager.CreateAsync(user, password));
                KiemTraIdentity(await _userManager.AddToRoleAsync(user, Roles.SinhVien));
                await _auditLog.LogAsync("TaoTaiKhoan", "TaiKhoan", maSV);
            });

        public Task<Result> CreateStaffAccountAsync(StaffCreateDto dto) =>
            ThucHienAsync(async () =>
            {
                var maKhoa = await KiemTraVaiTroVaKhoaAsync(dto.VaiTro, dto.MaKhoaPhuTrach);
                var userName = dto.UserName.Trim();
                if (await _userManager.FindByNameAsync(userName) != null)
                    throw new NghiepVuException($"Tên đăng nhập '{userName}' đã tồn tại.", StatusCodes.Status409Conflict);

                var user = new ApplicationUser
                {
                    UserName = userName,
                    HoTen = dto.HoTen.Trim(),
                    MaKhoaPhuTrach = maKhoa,
                    TrangThaiTaiKhoan = TrangThaiTaiKhoan.HoatDong,
                    LockoutEnabled = true
                };
                KiemTraIdentity(await _userManager.CreateAsync(user, dto.MatKhau));
                KiemTraIdentity(await _userManager.AddToRoleAsync(user, dto.VaiTro));
                await _auditLog.LogAsync("TaoTaiKhoan", "TaiKhoan", userName);
            });

        public Task<Result> ResetPasswordAsync(string userName, string newPassword) =>
            ThucHienAsync(async () =>
            {
                KiemTraChinhSachMatKhau(newPassword);
                var user = await TimTheoTenAsync(userName);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // ResetPasswordAsync lưu hash mới và đổi SecurityStamp
                KiemTraIdentity(await _userManager.ResetPasswordAsync(user, token, newPassword));
                await _auditLog.LogAsync("CapLaiMatKhau", "TaiKhoan", user.UserName!);
            });

        public Task<Result> SetLockAsync(string userName, bool locked, string reason) =>
            ThucHienAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(reason))
                    throw new NghiepVuException("Vui lòng nhập lý do.");

                var user = await TimTheoTenAsync(userName);
                if (locked)
                {
                    if (user.TrangThaiTaiKhoan == TrangThaiTaiKhoan.BiKhoa)
                        throw new NghiepVuException("Tài khoản đã bị khóa.");
                    if (await _userManager.IsInRoleAsync(user, Roles.Admin) && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                        throw new NghiepVuException("Không thể khóa Admin hoạt động cuối cùng của hệ thống.");

                    // Trạng thái nghiệp vụ + LockoutEnd để Identity chặn đăng nhập
                    user.TrangThaiTaiKhoan = TrangThaiTaiKhoan.BiKhoa;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }
                else
                {
                    if (user.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong)
                        throw new NghiepVuException("Tài khoản đang hoạt động.");

                    user.TrangThaiTaiKhoan = TrangThaiTaiKhoan.HoatDong;
                    user.LockoutEnd = null;
                    user.AccessFailedCount = 0;
                }

                user.LyDoKhoa = reason.Trim();
                KiemTraIdentity(await _userManager.UpdateAsync(user));
                // Thay đổi có hiệu lực từ yêu cầu kế tiếp (UC08, UC16)
                KiemTraIdentity(await _userManager.UpdateSecurityStampAsync(user));
                await _auditLog.LogAsync(locked ? "KhoaTaiKhoan" : "MoKhoaTaiKhoan", "TaiKhoan", user.UserName!, reason.Trim());
            });

        public Task<Result> ChangePasswordAsync(string userId, string current, string newPwd) =>
            ThucHienAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId)
                    ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);

                if (!await _userManager.CheckPasswordAsync(user, current))
                    throw new NghiepVuException("Mật khẩu hiện tại không đúng.");
                if (newPwd == current)
                    throw new NghiepVuException("Mật khẩu mới phải khác mật khẩu hiện tại.");

                // ChangePasswordAsync kiểm tra chính sách mật khẩu, lưu hash và đổi SecurityStamp
                KiemTraIdentity(await _userManager.ChangePasswordAsync(user, current, newPwd));
                await _auditLog.LogAsync("DoiMatKhau", "TaiKhoan", user.UserName!);
            });

        // ===================== Quản lý tài khoản Admin/Giáo vụ (FR-01, FR-03) =====================

        public async Task<PagedResult<UserListItemDto>> ListStaffAsync(UserFilterVm filter)
        {
            var query = QueryTaiKhoan();

            // Không phân biệt hoa thường và dấu: "nguyen" khớp "Nguyễn"
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(x =>
                    EF.Functions.Collate(x.User.UserName!, TextHelper.CollationKhongDau).Contains(kw) ||
                    EF.Functions.Collate(x.User.HoTen!, TextHelper.CollationKhongDau).Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(filter.VaiTro))
                query = query.Where(x => x.VaiTro == filter.VaiTro);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoa))
                query = query.Where(x => x.User.MaKhoaPhuTrach == filter.MaKhoa);
            if (filter.TrangThai.HasValue)
                query = query.Where(x => x.User.TrangThaiTaiKhoan == filter.TrangThai.Value);

            // Phân trang tại CSDL: COUNT + ORDER BY UserName OFFSET/FETCH
            return await query
                .OrderBy(x => x.User.UserName)
                .ToPagedResultAsync(filter.Page, filter.PageSize, ItemSelector);
        }

        public async Task<UserListItemDto> GetStaffAsync(string id) =>
            await QueryTaiKhoan().Where(x => x.User.Id == id).Select(ItemSelector).FirstOrDefaultAsync()
                ?? throw new NghiepVuException("Không tìm thấy tài khoản Admin/Giáo vụ.", StatusCodes.Status404NotFound);

        public async Task UpdateStaffAsync(StaffEditVm dto)
        {
            var result = await ThucHienAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(dto.Id)
                    ?? throw new NghiepVuException("Không tìm thấy tài khoản.", StatusCodes.Status404NotFound);
                var vaiTroCu = await LayVaiTroQuanLyAsync(user);
                var maKhoa = await KiemTraVaiTroVaKhoaAsync(dto.VaiTro, dto.MaKhoaPhuTrach);

                var doiVaiTro = vaiTroCu != dto.VaiTro;
                var doiKhoa = user.MaKhoaPhuTrach != maKhoa;

                // Không được làm mất Admin hoạt động cuối cùng (FR-03, FR-05)
                if (doiVaiTro && vaiTroCu == Roles.Admin && user.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong
                    && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                    throw new NghiepVuException("Không thể đổi vai trò Admin hoạt động cuối cùng của hệ thống.");

                var khoaCu = user.MaKhoaPhuTrach;
                user.HoTen = dto.HoTen.Trim();
                user.MaKhoaPhuTrach = maKhoa;
                KiemTraIdentity(await _userManager.UpdateAsync(user));
                if (doiVaiTro)
                {
                    KiemTraIdentity(await _userManager.RemoveFromRoleAsync(user, vaiTroCu));
                    KiemTraIdentity(await _userManager.AddToRoleAsync(user, dto.VaiTro));
                }

                await _auditLog.LogAsync("CapNhatTaiKhoan", "TaiKhoan", user.UserName!);
                if (doiVaiTro || doiKhoa)
                {
                    // Đổi SecurityStamp để quyền/khoa mới áp dụng từ yêu cầu kế tiếp của tài khoản đó
                    KiemTraIdentity(await _userManager.UpdateSecurityStampAsync(user));
                    await _auditLog.LogAsync("DoiQuyenKhoa", "TaiKhoan", user.UserName!,
                        $"Vai trò: {vaiTroCu} -> {dto.VaiTro}; Khoa: {khoaCu ?? "-"} -> {maKhoa ?? "-"}");
                }
            });
            result.ThrowIfFailed();
        }

        // ===================== Hồ sơ cá nhân (FR-11, FR-12) =====================

        // Định danh luôn lấy từ tài khoản đăng nhập, không nhận từ client (FR-09)
        public async Task<ProfileVm> GetProfileAsync()
        {
            var taiKhoan = await LayTaiKhoanDangNhapAsync();
            var roles = await _userManager.GetRolesAsync(taiKhoan);
            var vaiTro = roles.Contains(Roles.Admin) ? Roles.Admin
                : roles.Contains(Roles.CanBo) ? Roles.CanBo
                : Roles.SinhVien;

            var hoSo = new ProfileVm
            {
                UserName = taiKhoan.UserName!,
                HoTen = taiKhoan.HoTen,
                VaiTro = vaiTro,
                MaKhoaPhuTrach = taiKhoan.MaKhoaPhuTrach,
                TrangThaiTaiKhoanText = taiKhoan.TrangThaiTaiKhoan.TenHienThi()
            };

            if (taiKhoan.MaKhoaPhuTrach != null)
                hoSo.TenKhoaPhuTrach = await _context.Khoas.AsNoTracking()
                    .Where(k => k.MaKhoa == taiKhoan.MaKhoaPhuTrach).Select(k => k.TenKhoa).FirstOrDefaultAsync();

            // Admin/Giáo vụ chỉ xem thông tin tài khoản; sinh viên xem hồ sơ của mình (FR-11)
            if (vaiTro == Roles.SinhVien && taiKhoan.MaSV != null)
            {
                hoSo.SinhVien = await TimHoSoSinhVienAsync(taiKhoan.MaSV);
                hoSo.HoTen = hoSo.SinhVien?.HoTen;
            }
            return hoSo;
        }

        public async Task<StudentProfileVm> UpdateContactAsync(ContactEditVm dto)
        {
            var taiKhoan = await LayTaiKhoanDangNhapAsync();
            if (taiKhoan.MaSV == null || !await _userManager.IsInRoleAsync(taiKhoan, Roles.SinhVien))
                throw new NghiepVuException("Chỉ sinh viên được cập nhật thông tin liên hệ.", StatusCodes.Status403Forbidden);

            // Quyền sở hữu: chỉ hồ sơ gắn với tài khoản đang đăng nhập (global filter loại hồ sơ đã xóa mềm)
            var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.MaSV == taiKhoan.MaSV)
                ?? throw new NghiepVuException("Không tìm thấy hồ sơ sinh viên.", StatusCodes.Status404NotFound);

            sv.SDT = string.IsNullOrWhiteSpace(dto.SDT) ? null : dto.SDT.Trim();
            sv.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
            sv.DiaChi = string.IsNullOrWhiteSpace(dto.DiaChi) ? null : dto.DiaChi.Trim();

            // Chỉ ghi định danh, không ghi giá trị liên hệ (NFR-07); lưu cùng một lần SaveChanges
            await _auditLog.LogAsync("CapNhatLienHe", "SinhVien", sv.MaSV);
            await _context.SaveChangesAsync();

            return (await TimHoSoSinhVienAsync(sv.MaSV))!;
        }

        // ===================== Hỗ trợ =====================

        // Chạy thao tác trong giao dịch của bên gọi; nếu bên gọi chưa mở thì tự mở và commit.
        // Lưu nhật ký đã Add trong thao tác. Lỗi 400/409 trả về Result.Fail (giao dịch tự mở sẽ rollback).
        private async Task<Result> ThucHienAsync(Func<Task> thaoTac)
        {
            var tuMo = _context.Database.CurrentTransaction == null;
            await using var tx = tuMo ? await _context.Database.BeginTransactionAsync() : null;
            try
            {
                await thaoTac();
                await _context.SaveChangesAsync();
                if (tx != null) await tx.CommitAsync();
                return Result.Ok();
            }
            catch (NghiepVuException ex) when (ex.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
            {
                return Result.Fail(ex.Message, ex.StatusCode);
            }
        }

        // Tài khoản Admin/Giáo vụ kèm vai trò; tài khoản sinh viên quản lý ở FR-19, FR-20
        private IQueryable<TaiKhoanVaiTro> QueryTaiKhoan() =>
            from u in _context.Users.AsNoTracking()
            join ur in _context.UserRoles on u.Id equals ur.UserId
            join r in _context.Roles on ur.RoleId equals r.Id
            where r.Name == Roles.Admin || r.Name == Roles.CanBo
            select new TaiKhoanVaiTro { User = u, VaiTro = r.Name! };

        private sealed class TaiKhoanVaiTro
        {
            public ApplicationUser User { get; set; } = null!;
            public string VaiTro { get; set; } = string.Empty;
        }

        private static readonly Expression<Func<TaiKhoanVaiTro, UserListItemDto>> ItemSelector =
            x => new UserListItemDto
            {
                Id = x.User.Id,
                UserName = x.User.UserName!,
                HoTen = x.User.HoTen,
                VaiTro = x.VaiTro,
                MaKhoaPhuTrach = x.User.MaKhoaPhuTrach,
                TenKhoa = x.User.KhoaPhuTrach != null ? x.User.KhoaPhuTrach.TenKhoa : null,
                TrangThai = x.User.TrangThaiTaiKhoan,
                TrangThaiText = x.User.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong ? "Hoạt động" : "Bị khóa"
            };

        // Trả về mã khoa sẽ lưu: bắt buộc với Giáo vụ, để trống với Admin
        private async Task<string?> KiemTraVaiTroVaKhoaAsync(string vaiTro, string? maKhoa)
        {
            if (vaiTro != Roles.Admin && vaiTro != Roles.CanBo)
                throw new NghiepVuException("Vai trò phải là Admin hoặc Giáo vụ.");
            if (vaiTro == Roles.Admin) return null;

            if (string.IsNullOrWhiteSpace(maKhoa))
                throw new NghiepVuException("Giáo vụ bắt buộc có khoa phụ trách.");
            // Global query filter đã loại khoa xóa mềm
            if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == maKhoa))
                throw new NghiepVuException($"Khoa '{maKhoa}' không tồn tại hoặc đã ngừng sử dụng.");
            return maKhoa;
        }

        private async Task<string> LayVaiTroQuanLyAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Admin)) return Roles.Admin;
            if (roles.Contains(Roles.CanBo)) return Roles.CanBo;
            throw new NghiepVuException("Không tìm thấy tài khoản Admin/Giáo vụ.", StatusCodes.Status404NotFound);
        }

        private Task<int> DemAdminHoatDongKhacAsync(string userId) =>
            QueryTaiKhoan().CountAsync(x => x.VaiTro == Roles.Admin
                && x.User.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong && x.User.Id != userId);

        private async Task<ApplicationUser> TimTheoTenAsync(string userName) =>
            await _userManager.FindByNameAsync(userName)
                ?? throw new NghiepVuException("Không tìm thấy tài khoản.");

        private async Task<ApplicationUser> LayTaiKhoanDangNhapAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return (principal == null ? null : await _userManager.GetUserAsync(principal))
                ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);
        }

        private Task<StudentProfileVm?> TimHoSoSinhVienAsync(string maSV) =>
            _context.SinhViens.AsNoTracking()
                .Where(s => s.MaSV == maSV)
                .Select(s => new StudentProfileVm
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    GioiTinhText = s.GioiTinh.TenHienThi(),
                    MaLop = s.MaLop,
                    TenLop = s.Lop!.TenLop,
                    TenKhoa = s.Lop.Khoa!.TenKhoa,
                    MaKhoaHoc = s.Lop.MaKhoaHoc,
                    TrangThaiText = s.TrangThai.TenHienThi(),
                    SDT = s.SDT,
                    Email = s.Email,
                    DiaChi = s.DiaChi
                })
                .FirstOrDefaultAsync();

        private static void KiemTraChinhSachMatKhau(string matKhau)
        {
            if (!ValidationRules.IsMatKhauManh(matKhau))
                throw new NghiepVuException(ValidationRules.MatKhauMessage);
        }

        private static void KiemTraIdentity(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new NghiepVuException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
