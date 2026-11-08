using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class AuditLogService : IAuditLogService
    {
        // ThoiDiem lưu UTC, hiển thị và lọc theo UTC+7 (mục 3.2.2.1)
        private const int LechGioHienThi = 7;

        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LogAsync(string hanhDong, string doiTuongLoai, string doiTuongMa, string? chiTiet = null)
        {
            _context.NhatKyThaoTacs.Add(new NhatKyThaoTac
            {
                ThoiDiem = DateTime.UtcNow,
                Username = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "unknown",
                HanhDong = hanhDong,
                DoiTuongLoai = doiTuongLoai,
                DoiTuongMa = TextHelper.CatChuoi(doiTuongMa, 256),
                ChiTiet = chiTiet == null ? null : TextHelper.CatChuoi(chiTiet, 500)
            });
            return Task.CompletedTask;
        }

        public async Task<PagedResult<AuditLogDto>> QueryAsync(AuditLogFilter filter)
        {
            var query = _context.NhatKyThaoTacs.AsNoTracking();

            if (filter.TuNgay.HasValue)
            {
                var tu = filter.TuNgay.Value.Date.AddHours(-LechGioHienThi);
                query = query.Where(n => n.ThoiDiem >= tu);
            }
            if (filter.DenNgay.HasValue)
            {
                var truoc = filter.DenNgay.Value.Date.AddDays(1).AddHours(-LechGioHienThi);
                query = query.Where(n => n.ThoiDiem < truoc);
            }
            if (!string.IsNullOrWhiteSpace(filter.NguoiThucHien))
            {
                var nguoi = filter.NguoiThucHien.Trim();
                query = query.Where(n => n.Username.Contains(nguoi));
            }
            if (!string.IsNullOrWhiteSpace(filter.HanhDong))
                query = query.Where(n => n.HanhDong == filter.HanhDong);
            if (!string.IsNullOrWhiteSpace(filter.DoiTuong))
            {
                var doiTuong = filter.DoiTuong.Trim();
                query = query.Where(n => n.DoiTuongMa.Contains(doiTuong));
            }

            return await query
                .OrderByDescending(n => n.ThoiDiem).ThenByDescending(n => n.Id)
                .ToPagedResultAsync(filter.Page, filter.PageSize, n => new AuditLogDto
                {
                    ThoiDiem = n.ThoiDiem.AddHours(LechGioHienThi).ToString("dd/MM/yyyy HH:mm:ss"),
                    Username = n.Username,
                    HanhDong = n.HanhDong,
                    DoiTuongLoai = n.DoiTuongLoai,
                    DoiTuongMa = n.DoiTuongMa,
                    ChiTiet = n.ChiTiet
                });
        }
    }
}
