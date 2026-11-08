using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class AccountService : IAccountService
    {
        private const int BiKhoa = MaTrangThai.TkBiKhoa, HoatDong = MaTrangThai.TkHoatDong;

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
                    throw new NghiepVuException("Tên đăng nhập (Mã SV) đã tồn tại.");

                var user = new ApplicationUser
                {
                    UserName = maSV,
                    MaSV = maSV,
                    TrangThaiTaiKhoan = HoatDong,
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
                    throw new NghiepVuException($"Tên đăng nhập '{userName}' đã tồn tại.");

                var user = new ApplicationUser
                {
                    UserName = userName,
                    HoTen = dto.HoTen.Trim(),
                    MaKhoaPhuTrach = maKhoa,
                    TrangThaiTaiKhoan = HoatDong,
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
                    if (user.TrangThaiTaiKhoan == BiKhoa)
                        throw new NghiepVuException("Tài khoản đã bị khóa.");
                    if (await _userManager.IsInRoleAsync(user, Roles.Admin) && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                        throw new NghiepVuException("Không thể khóa Admin hoạt động cuối cùng của hệ thống.");

                    // Trạng thái nghiệp vụ + LockoutEnd để Identity chặn đăng nhập
                    user.TrangThaiTaiKhoan = BiKhoa;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    KiemTraIdentity(await _userManager.UpdateAsync(user));
                    // Phiên đăng nhập hiện có mất hiệu lực từ yêu cầu kế tiếp
                    KiemTraIdentity(await _userManager.UpdateSecurityStampAsync(user));
                }
                else
                {
                    if (user.TrangThaiTaiKhoan == HoatDong)
                        throw new NghiepVuException("Tài khoản đang hoạt động.");

                    user.TrangThaiTaiKhoan = HoatDong;
                    user.LockoutEnd = null;
                    user.AccessFailedCount = 0;
                    KiemTraIdentity(await _userManager.UpdateAsync(user));
                }
                await _auditLog.LogAsync(locked ? "KhoaTaiKhoan" : "MoKhoaTaiKhoan", "TaiKhoan", user.UserName!, reason);
            });

        public Task<Result> ChangePasswordAsync(string userId, string current, string newPwd) =>
            ThucHienAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId)
                    ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", 401);

                if (!await _userManager.CheckPasswordAsync(user, current))
                    throw new NghiepVuException("Mật khẩu hiện tại không đúng.");
                if (newPwd == current)
                    throw new NghiepVuException("Mật khẩu mới phải khác mật khẩu hiện tại.");

                // ChangePasswordAsync kiểm tra chính sách mật khẩu, lưu hash và đổi SecurityStamp
                KiemTraIdentity(await _userManager.ChangePasswordAsync(user, current, newPwd));
                await _auditLog.LogAsync("DoiMatKhau", "TaiKhoan", user.UserName!);
            });

        // ===================== Quản lý tài khoản Admin/Giáo vụ (FR-01, FR-03) =====================

        public async Task<PagedResultDto<TaiKhoanItemDto>> ListStaffAsync(TaiKhoanFilterDto filter)
        {
            var query = QueryTaiKhoan();

            // Không phân biệt hoa thường và dấu: "nguyen" khớp "Nguyễn", "GV.CNTT" khớp "gv.cntt"
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

        public async Task<TaiKhoanItemDto> GetStaffAsync(string id) =>
            await QueryTaiKhoan().Where(x => x.User.Id == id).Select(ItemSelector).FirstOrDefaultAsync()
                ?? throw new NghiepVuException("Không tìm thấy tài khoản Admin/Giáo vụ.", 404);

        public async Task UpdateStaffAsync(TaiKhoanUpdateDto dto)
        {
            var result = await ThucHienAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(dto.Id)
                    ?? throw new NghiepVuException("Không tìm thấy tài khoản.", 404);
                var vaiTroCu = await LayVaiTroQuanLyAsync(user);
                var maKhoa = await KiemTraVaiTroVaKhoaAsync(dto.VaiTro, dto.MaKhoaPhuTrach);

                var doiVaiTro = vaiTroCu != dto.VaiTro;
                var doiKhoa = user.MaKhoaPhuTrach != maKhoa;

                // Không được làm mất Admin hoạt động cuối cùng (FR-03, FR-05)
                if (doiVaiTro && vaiTroCu == Roles.Admin && user.TrangThaiTaiKhoan == HoatDong
                    && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                    throw new NghiepVuException("Không thể đổi vai trò Admin hoạt động cuối cùng của hệ thống.");

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
                    if (doiVaiTro) await _auditLog.LogAsync("DoiQuyen", "TaiKhoan", user.UserName!);
                    if (doiKhoa) await _auditLog.LogAsync("DoiKhoaPhuTrach", "TaiKhoan", user.UserName!);
                }
            });
            result.ThrowIfFailed();
        }

        // ===================== Hồ sơ cá nhân (FR-11, FR-12) =====================

        // Định danh luôn lấy từ tài khoản đăng nhập, không nhận từ client (FR-09)
        public async Task<HoSoCaNhanDto> GetProfileAsync()
        {
            var taiKhoan = await LayTaiKhoanDangNhapAsync();
            var roles = await _userManager.GetRolesAsync(taiKhoan);
            var vaiTro = roles.Contains(Roles.Admin) ? Roles.Admin
                : roles.Contains(Roles.CanBo) ? Roles.CanBo
                : Roles.SinhVien;

            var hoSo = new HoSoCaNhanDto
            {
                UserName = taiKhoan.UserName!,
                HoTen = taiKhoan.HoTen,
                VaiTro = vaiTro,
                MaKhoaPhuTrach = taiKhoan.MaKhoaPhuTrach,
                TrangThaiTaiKhoanText = MaTrangThai.TenTrangThaiTaiKhoan(taiKhoan.TrangThaiTaiKhoan)
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

        public async Task<HoSoSinhVienDto> UpdateContactAsync(CapNhatLienHeDto dto)
        {
            var taiKhoan = await LayTaiKhoanDangNhapAsync();
            if (taiKhoan.MaSV == null || !await _userManager.IsInRoleAsync(taiKhoan, Roles.SinhVien))
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

            // Chỉ ghi định danh, không ghi giá trị liên hệ (NFR-07); lưu cùng một lần SaveChanges
            await _auditLog.LogAsync("CapNhatLienHe", "SinhVien", sv.MaSV);
            await _context.SaveChangesAsync();

            return (await TimHoSoSinhVienAsync(sv.MaSV))!;
        }

        // ===================== Hỗ trợ =====================

        // Chạy thao tác trong giao dịch của bên gọi; nếu bên gọi chưa mở thì tự mở và commit.
        // Lưu nhật ký đã Add trong thao tác. Lỗi nghiệp vụ trả về Result.Fail (giao dịch tự mở sẽ rollback).
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
            catch (NghiepVuException ex) when (ex.StatusCode == StatusCodes.Status400BadRequest)
            {
                return Result.Fail(ex.Message);
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

        private static readonly Expression<Func<TaiKhoanVaiTro, TaiKhoanItemDto>> ItemSelector =
            x => new TaiKhoanItemDto
            {
                Id = x.User.Id,
                UserName = x.User.UserName!,
                HoTen = x.User.HoTen,
                VaiTro = x.VaiTro,
                MaKhoaPhuTrach = x.User.MaKhoaPhuTrach,
                TenKhoa = x.User.KhoaPhuTrach != null ? x.User.KhoaPhuTrach.TenKhoa : null,
                TrangThai = x.User.TrangThaiTaiKhoan,
                TrangThaiText = x.User.TrangThaiTaiKhoan == HoatDong ? "Hoạt động" : "Bị khóa"
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
            throw new NghiepVuException("Không tìm thấy tài khoản Admin/Giáo vụ.", 404);
        }

        private Task<int> DemAdminHoatDongKhacAsync(string userId) =>
            QueryTaiKhoan().CountAsync(x => x.VaiTro == Roles.Admin && x.User.TrangThaiTaiKhoan == HoatDong && x.User.Id != userId);

        private async Task<ApplicationUser> TimTheoTenAsync(string userName) =>
            await _userManager.FindByNameAsync(userName)
                ?? throw new NghiepVuException("Không tìm thấy tài khoản.");

        private async Task<ApplicationUser> LayTaiKhoanDangNhapAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return (principal == null ? null : await _userManager.GetUserAsync(principal))
                ?? throw new NghiepVuException("Phiên đăng nhập không hợp lệ.", 401);
        }

        private Task<HoSoSinhVienDto?> TimHoSoSinhVienAsync(string maSV) =>
            _context.SinhViens.AsNoTracking()
                .Where(s => s.MaSV == maSV)
                .Select(s => new HoSoSinhVienDto
                {
                    MaSV = s.MaSV,
                    HoTen = s.HoTen,
                    NgaySinhFormatted = s.NgaySinh.ToString("dd/MM/yyyy"),
                    GioiTinhText = MaTrangThai.TenGioiTinh(s.GioiTinh),
                    MaLop = s.MaLop,
                    TenLop = s.Lop!.TenLop,
                    TenKhoa = s.Lop.Khoa!.TenKhoa,
                    MaKhoaHoc = s.Lop.MaKhoaHoc,
                    TrangThaiText = MaTrangThai.TenTrangThai(s.TrangThai),
                    SDT = s.SDT,
                    Email = s.Email,
                    DiaChi = s.DiaChi
                })
                .FirstOrDefaultAsync();

        // Tối thiểu 8 ký tự, gồm cả chữ và số (mục 3.4)
        private static void KiemTraChinhSachMatKhau(string matKhau)
        {
            if (matKhau.Length < 8 || !matKhau.Any(char.IsLetter) || !matKhau.Any(char.IsDigit))
                throw new NghiepVuException("Mật khẩu tối thiểu 8 ký tự, gồm cả chữ và số.");
        }

        private static void KiemTraIdentity(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new NghiepVuException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
