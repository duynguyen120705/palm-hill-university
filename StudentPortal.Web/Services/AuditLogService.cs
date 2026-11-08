using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Models.Entities;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class AuditLogService : IAuditLogService
    {
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
    }
}
