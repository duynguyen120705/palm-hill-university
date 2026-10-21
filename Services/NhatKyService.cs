using StudentPortal.Data;
using StudentPortal.Models;

namespace StudentPortal.Services
{
    public class NhatKyService : INhatKyService
    {
        private readonly ApplicationDbContext _context;

        public NhatKyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dùng chung DbContext với bên gọi nên nằm trong giao dịch của bên gọi (nếu có)
        public async Task GhiAsync(string username, string hanhDong, string doiTuongLoai, string doiTuongMa)
        {
            _context.NhatKyThaoTacs.Add(new NhatKyThaoTac
            {
                ThoiDiem = DateTime.UtcNow,
                Username = username,
                HanhDong = hanhDong,
                DoiTuongLoai = doiTuongLoai,
                DoiTuongMa = doiTuongMa
            });
            await _context.SaveChangesAsync();
        }
    }
}
