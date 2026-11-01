using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;
using StudentPortal.Models.DTOs;

namespace StudentPortal.Services
{
    public class QuanLyTaiKhoanService : IQuanLyTaiKhoanService
    {
        private const string Admin = "Admin", GiaoVu = "GiaoVu";

        private const int BiKhoa = ITaiKhoanService.BiKhoa, HoatDong = ITaiKhoanService.HoatDong;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITaiKhoanService _taiKhoan;
        private readonly INhatKyService _nhatKy;

        public QuanLyTaiKhoanService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            ITaiKhoanService taiKhoan, INhatKyService nhatKy)
        {
            _context = context;
            _userManager = userManager;
            _taiKhoan = taiKhoan;
            _nhatKy = nhatKy;
        }

        // Tài khoản Admin/Giáo vụ kèm vai trò; tài khoản sinh viên quản lý ở FR-19, FR-20
        private IQueryable<TaiKhoanVaiTro> QueryTaiKhoan() =>
            from u in _context.Users.AsNoTracking()
            join ur in _context.UserRoles on u.Id equals ur.UserId
            join r in _context.Roles on ur.RoleId equals r.Id
            where r.Name == Admin || r.Name == GiaoVu
            select new TaiKhoanVaiTro { User = u, VaiTro = r.Name! };

        private sealed class TaiKhoanVaiTro
        {
            public ApplicationUser User { get; set; } = null!;
            public string VaiTro { get; set; } = string.Empty;
        }

        private static IQueryable<TaiKhoanItemDto> ToItem(IQueryable<TaiKhoanVaiTro> query) => query.Select(ItemSelector);

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

        public async Task<PagedResultDto<TaiKhoanItemDto>> DanhSachAsync(TaiKhoanFilterDto filter)
        {
            var query = QueryTaiKhoan();

            // Không phân biệt hoa thường và dấu: "nguyen" khớp "Nguyễn", "hung" khớp "Hưng", "GV.CNTT" khớp "gv.cntt"
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(x =>
                    EF.Functions.Collate(x.User.UserName!, TimKiemKhongDau.Collation).Contains(kw) ||
                    EF.Functions.Collate(x.User.HoTen!, TimKiemKhongDau.Collation).Contains(kw));
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

        public async Task<List<LuaChonDto>> DanhSachKhoaAsync()
        {
            return await _context.Khoas.AsNoTracking()
                .OrderBy(k => k.MaKhoa)
                .Select(k => new LuaChonDto { Ma = k.MaKhoa, Ten = k.TenKhoa })
                .ToListAsync();
        }

        public async Task<TaiKhoanItemDto> TaoAsync(TaiKhoanCreateDto dto, ClaimsPrincipal admin)
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

            await using var tx = await _context.Database.BeginTransactionAsync();
            KiemTraIdentity(await _userManager.CreateAsync(user, dto.MatKhau));
            KiemTraIdentity(await _userManager.AddToRoleAsync(user, dto.VaiTro));
            await _nhatKy.GhiAsync(TenDangNhap(admin), "TaoTaiKhoan", "TaiKhoan", userName);
            await tx.CommitAsync();

            return await LayItemAsync(user.Id);
        }

        public async Task<TaiKhoanItemDto> CapNhatAsync(TaiKhoanUpdateDto dto, ClaimsPrincipal admin)
        {
            var user = await TimTaiKhoanAsync(dto.Id);
            var vaiTroCu = await LayVaiTroQuanLyAsync(user);
            var maKhoa = await KiemTraVaiTroVaKhoaAsync(dto.VaiTro, dto.MaKhoaPhuTrach);

            var doiVaiTro = vaiTroCu != dto.VaiTro;
            var doiKhoa = user.MaKhoaPhuTrach != maKhoa;

            // Không được làm mất Admin hoạt động cuối cùng (FR-03, FR-05)
            if (doiVaiTro && vaiTroCu == Admin && user.TrangThaiTaiKhoan == HoatDong && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                throw new NghiepVuException("Không thể đổi vai trò Admin hoạt động cuối cùng của hệ thống.");

            user.HoTen = dto.HoTen.Trim();
            user.MaKhoaPhuTrach = maKhoa;

            await using var tx = await _context.Database.BeginTransactionAsync();
            KiemTraIdentity(await _userManager.UpdateAsync(user));
            if (doiVaiTro)
            {
                KiemTraIdentity(await _userManager.RemoveFromRoleAsync(user, vaiTroCu));
                KiemTraIdentity(await _userManager.AddToRoleAsync(user, dto.VaiTro));
            }

            var nguoiThucHien = TenDangNhap(admin);
            await _nhatKy.GhiAsync(nguoiThucHien, "CapNhatTaiKhoan", "TaiKhoan", user.UserName!);
            if (doiVaiTro || doiKhoa)
            {
                // Đổi SecurityStamp để quyền/khoa mới áp dụng từ yêu cầu kế tiếp của tài khoản đó
                KiemTraIdentity(await _userManager.UpdateSecurityStampAsync(user));
                if (doiVaiTro) await _nhatKy.GhiAsync(nguoiThucHien, "DoiQuyen", "TaiKhoan", user.UserName!);
                if (doiKhoa) await _nhatKy.GhiAsync(nguoiThucHien, "DoiKhoaPhuTrach", "TaiKhoan", user.UserName!);
            }
            await tx.CommitAsync();

            return await LayItemAsync(user.Id);
        }

        public async Task DatLaiMatKhauAsync(DatLaiMatKhauDto dto, ClaimsPrincipal admin)
        {
            var user = await TimTaiKhoanAsync(dto.Id);
            await LayVaiTroQuanLyAsync(user);

            await _taiKhoan.DatLaiMatKhauAsync(user, dto.MatKhauMoi);
            await _nhatKy.GhiAsync(TenDangNhap(admin), "CapLaiMatKhau", "TaiKhoan", user.UserName!);
        }

        public async Task<TaiKhoanItemDto> DoiTrangThaiAsync(DoiTrangThaiTaiKhoanDto dto, ClaimsPrincipal admin)
        {
            if (string.IsNullOrWhiteSpace(dto.LyDo))
                throw new NghiepVuException("Vui lòng nhập lý do.");

            var user = await TimTaiKhoanAsync(dto.Id);
            var vaiTro = await LayVaiTroQuanLyAsync(user);

            if (dto.Khoa)
            {
                if (user.TrangThaiTaiKhoan == BiKhoa)
                    throw new NghiepVuException("Tài khoản đã bị khóa.");
                if (vaiTro == Admin && await DemAdminHoatDongKhacAsync(user.Id) == 0)
                    throw new NghiepVuException("Không thể khóa Admin hoạt động cuối cùng của hệ thống.");
            }
            else if (user.TrangThaiTaiKhoan == HoatDong)
            {
                throw new NghiepVuException("Tài khoản đang hoạt động.");
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            if (dto.Khoa) await _taiKhoan.KhoaAsync(user); else await _taiKhoan.MoKhoaAsync(user);
            await _nhatKy.GhiAsync(TenDangNhap(admin), dto.Khoa ? "KhoaTaiKhoan" : "MoKhoaTaiKhoan", "TaiKhoan", user.UserName!);
            await tx.CommitAsync();

            return await LayItemAsync(user.Id);
        }

        // ===================== Hỗ trợ =====================

        // Trả về mã khoa sẽ lưu: bắt buộc với Giáo vụ, để trống với Admin
        private async Task<string?> KiemTraVaiTroVaKhoaAsync(string vaiTro, string? maKhoa)
        {
            if (vaiTro != Admin && vaiTro != GiaoVu)
                throw new NghiepVuException("Vai trò phải là Admin hoặc Giáo vụ.");
            if (vaiTro == Admin) return null;

            if (string.IsNullOrWhiteSpace(maKhoa))
                throw new NghiepVuException("Giáo vụ bắt buộc có khoa phụ trách.");
            // Global query filter đã loại khoa xóa mềm
            if (!await _context.Khoas.AnyAsync(k => k.MaKhoa == maKhoa))
                throw new NghiepVuException($"Khoa '{maKhoa}' không tồn tại hoặc đã ngừng sử dụng.");
            return maKhoa;
        }

        private async Task<ApplicationUser> TimTaiKhoanAsync(string id)
        {
            return await _userManager.FindByIdAsync(id)
                ?? throw new NghiepVuException("Không tìm thấy tài khoản.", 404);
        }

        // Chỉ quản lý tài khoản Admin/Giáo vụ ở đây
        private async Task<string> LayVaiTroQuanLyAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Admin)) return Admin;
            if (roles.Contains(GiaoVu)) return GiaoVu;
            throw new NghiepVuException("Không tìm thấy tài khoản Admin/Giáo vụ.", 404);
        }

        private Task<int> DemAdminHoatDongKhacAsync(string userId) =>
            QueryTaiKhoan().CountAsync(x => x.VaiTro == Admin && x.User.TrangThaiTaiKhoan == HoatDong && x.User.Id != userId);

        private async Task<TaiKhoanItemDto> LayItemAsync(string id) =>
            await ToItem(QueryTaiKhoan().Where(x => x.User.Id == id)).FirstAsync();

        private static string TenDangNhap(ClaimsPrincipal user) => user.Identity?.Name ?? "unknown";

        // Lỗi từ Identity (VD: mật khẩu không đạt chính sách) - giao dịch chưa commit sẽ tự rollback khi dispose
        private static void KiemTraIdentity(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new NghiepVuException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
