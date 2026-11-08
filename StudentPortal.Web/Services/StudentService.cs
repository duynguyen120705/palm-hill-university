using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Repositories;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class StudentService : IStudentService
    {
        private const string LoaiSinhVien = "SinhVien";

        private readonly AppDbContext _context;
        private readonly IRepository<SinhVien> _students;
        private readonly IAccountService _account;
        private readonly IScopeService _scope;
        private readonly IAuditLogService _auditLog;

        public StudentService(AppDbContext context, IRepository<SinhVien> students, IAccountService account,
            IScopeService scope, IAuditLogService auditLog)
        {
            _context = context;
            _students = students;
            _account = account;
            _scope = scope;
            _auditLog = auditLog;
        }

        // ===================== FR-13 Thêm hồ sơ =====================

        public async Task CreateAsync(StudentCreateVm dto)
        {
            var maSV = dto.MaSV.Trim().ToUpperInvariant();
            await KiemTraLopAsync(dto.MaLop);

            // Mã SV duy nhất kể cả hồ sơ đã xóa mềm, không dùng lại
            if (await _students.QueryIncludingDeleted().AnyAsync(s => s.MaSV == maSV))
                throw new NghiepVuException("Mã sinh viên đã tồn tại", StatusCodes.Status409Conflict);

            var hoTen = dto.HoTen.Trim();
            await using var tx = await _context.Database.BeginTransactionAsync();

            _students.Add(new SinhVien
            {
                MaSV = maSV,
                HoTen = hoTen,
                HoTenKhongDau = TextHelper.BoDau(hoTen),
                NgaySinh = dto.NgaySinh!.Value.Date,
                GioiTinh = dto.GioiTinh!.Value,
                MaLop = dto.MaLop,
                SDT = LamSach(dto.SDT),
                Email = LamSach(dto.Email)?.ToLowerInvariant(),
                DiaChi = LamSach(dto.DiaChi),
                TrangThai = dto.TrangThai
            });
            await _auditLog.LogAsync("TaoHoSo", LoaiSinhVien, maSV);
            await _context.SaveChangesAsync();

            // Lỗi tạo tài khoản: giao dịch chưa commit nên hồ sơ cũng bị hủy (FR-13)
            var taoTaiKhoan = await _account.CreateStudentAccountAsync(maSV, dto.MatKhau);
            if (!taoTaiKhoan.Succeeded)
                throw taoTaiKhoan.StatusCode == StatusCodes.Status409Conflict
                    ? new NghiepVuException(taoTaiKhoan.Error!, StatusCodes.Status409Conflict)
                    : new NghiepVuException("Không tạo được tài khoản sinh viên, hồ sơ chưa được lưu.");

            // Hồ sơ Thôi học: tài khoản tạo ra ở trạng thái khóa, thống nhất với FR-16
            if (dto.TrangThai == TrangThaiHocTap.ThoiHoc)
                (await _account.SetLockAsync(maSV, true, "Hồ sơ tạo ở trạng thái Thôi học")).ThrowIfFailed();

            await tx.CommitAsync();
        }

        // ===================== FR-14 Chi tiết hồ sơ =====================

        public async Task<StudentDetailVm> GetDetailAsync(string maSV)
        {
            await LayHoSoAsync(maSV, tracking: false);

            var row = await _students.QueryNoTracking()
                .Where(s => s.MaSV == maSV)
                .Select(s => new
                {
                    s.MaSV, s.HoTen, s.NgaySinh, s.GioiTinh, s.MaLop, s.Lop!.TenLop, TenKhoa = s.Lop.Khoa!.TenKhoa,
                    s.Lop.MaKhoaHoc, s.SDT, s.Email, s.DiaChi, s.TrangThai,
                    TrangThaiTaiKhoan = s.TaiKhoan != null ? s.TaiKhoan.TrangThaiTaiKhoan : (TrangThaiTaiKhoan?)null
                })
                .FirstAsync();

            var isAdmin = _scope.GetCurrent().IsAdmin;
            var tkHoatDong = row.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong;
            var tkBiKhoa = row.TrangThaiTaiKhoan == TrangThaiTaiKhoan.BiKhoa;

            return new StudentDetailVm
            {
                MaSV = row.MaSV,
                HoTen = row.HoTen,
                NgaySinh = row.NgaySinh.ToString("yyyy-MM-dd"),
                NgaySinhFormatted = row.NgaySinh.ToString("dd/MM/yyyy"),
                GioiTinh = row.GioiTinh,
                GioiTinhText = row.GioiTinh.TenHienThi(),
                MaLop = row.MaLop,
                TenLop = row.TenLop,
                TenKhoa = row.TenKhoa,
                MaKhoaHoc = row.MaKhoaHoc,
                SDT = row.SDT,
                Email = row.Email,
                DiaChi = row.DiaChi,
                TrangThai = row.TrangThai,
                TrangThaiText = row.TrangThai.TenHienThi(),
                TrangThaiTaiKhoanText = row.TrangThaiTaiKhoan?.TenHienThi() ?? "Chưa có tài khoản",
                CoTheDoiTrangThai = TrangThaiDuocChuyen(row.TrangThai, isAdmin).Any(),
                // Điều kiện còn lại của Giáo vụ (chưa đăng nhập, chưa biến động) kiểm tra khi xóa để nêu lý do cụ thể
                CoTheXoa = isAdmin ? row.TrangThai != TrangThaiHocTap.DaTotNghiep : row.TrangThai == TrangThaiHocTap.DangHoc,
                CoTheCapLaiMatKhau = row.TrangThaiTaiKhoan != null,
                CoTheKhoa = tkHoatDong,
                CoTheMoKhoa = tkBiKhoa && (isAdmin || row.TrangThai != TrangThaiHocTap.ThoiHoc)
            };
        }

        // ===================== FR-15 Cập nhật hồ sơ =====================

        public async Task<StudentDetailVm> UpdateAsync(StudentEditVm dto)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: true);

            GanThongTinBatBuoc(sv, dto);
            sv.SDT = LamSach(dto.SDT);
            sv.Email = LamSach(dto.Email)?.ToLowerInvariant();
            sv.DiaChi = LamSach(dto.DiaChi);

            await _auditLog.LogAsync("SuaHoSo", LoaiSinhVien, sv.MaSV);
            await _context.SaveChangesAsync();
            return await GetDetailAsync(sv.MaSV);
        }

        // ===================== FR-16 Đổi trạng thái =====================

        public async Task<List<AllowedStatusDto>> GetAllowedStatusesAsync(string maSV)
        {
            var sv = await LayHoSoAsync(maSV, tracking: false);
            var isAdmin = _scope.GetCurrent().IsAdmin;

            return TrangThaiDuocChuyen(sv.TrangThai, isAdmin)
                .Select(t => new AllowedStatusDto
                {
                    Value = t,
                    Text = t.TenHienThi(),
                    BatBuocLyDo = sv.TrangThai == TrangThaiHocTap.DaTotNghiep
                })
                .ToList();
        }

        public async Task<StudentDetailVm> ChangeStatusAsync(ChangeStatusVm dto)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: true);
            var isAdmin = _scope.GetCurrent().IsAdmin;
            var cu = sv.TrangThai;
            var moi = dto.TrangThaiMoi!.Value;
            var lyDo = LamSach(dto.LyDo);
            KiemTraChuyenTrangThai(cu, moi, isAdmin, lyDo);

            await using var tx = await _context.Database.BeginTransactionAsync();

            sv.TrangThai = moi;
            sv.DaCoBienDong = true;
            await _auditLog.LogAsync("DoiTrangThai", LoaiSinhVien, sv.MaSV,
                $"{cu.TenHienThi()} -> {moi.TenHienThi()}" + (lyDo == null ? "" : $"; Lý do: {lyDo}"));
            await _context.SaveChangesAsync();

            // Chuyển sang Thôi học tự động khóa tài khoản trong cùng giao dịch.
            // Thôi học -> Đang học: tài khoản giữ trạng thái khóa, chỉ mở thủ công (FR-20)
            if (moi == TrangThaiHocTap.ThoiHoc)
                await KhoaTaiKhoanNeuDangHoatDongAsync(sv.MaSV, "Sinh viên chuyển sang Thôi học");

            await tx.CommitAsync();
            return await GetDetailAsync(sv.MaSV);
        }

        // ===================== FR-17 Chuyển lớp =====================

        public async Task<StudentDetailVm> TransferAsync(TransferVm dto)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: true);
            await KiemTraLopAsync(dto.MaLopDich);

            if (string.Equals(sv.MaLop, dto.MaLopDich, StringComparison.OrdinalIgnoreCase))
                throw new NghiepVuException("Lớp đích phải khác lớp hiện tại.");

            var lopCu = sv.MaLop;
            var lyDo = LamSach(dto.LyDo);
            sv.MaLop = dto.MaLopDich;
            sv.DaCoBienDong = true;

            await _auditLog.LogAsync("ChuyenLop", LoaiSinhVien, sv.MaSV,
                $"{lopCu} -> {dto.MaLopDich}" + (lyDo == null ? "" : $"; Lý do: {lyDo}"));
            await _context.SaveChangesAsync();
            return await GetDetailAsync(sv.MaSV);
        }

        // ===================== FR-18 Xóa mềm =====================

        public async Task SoftDeleteAsync(StudentDeleteVm dto)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: true);
            var scope = _scope.GetCurrent();

            if (scope.IsAdmin)
            {
                if (sv.TrangThai == TrangThaiHocTap.DaTotNghiep)
                    throw new NghiepVuException("Không được xóa hồ sơ Đã tốt nghiệp.");
            }
            else
            {
                if (sv.TrangThai != TrangThaiHocTap.DangHoc)
                    throw new NghiepVuException("Giáo vụ chỉ được xóa hồ sơ ở trạng thái Đang học.");
                if (sv.DaCoBienDong)
                    throw new NghiepVuException("Hồ sơ đã có thao tác đổi trạng thái hoặc chuyển lớp.");
                if (await _context.Users.AnyAsync(u => u.MaSV == sv.MaSV && u.LanDangNhapCuoi != null))
                    throw new NghiepVuException("Sinh viên đã từng đăng nhập.");
            }

            var lyDo = dto.LyDo.Trim();
            await using var tx = await _context.Database.BeginTransactionAsync();

            sv.DaXoa = true;
            sv.NgayXoa = DateTime.UtcNow;
            sv.NguoiXoa = scope.UserName;
            sv.LyDoXoa = lyDo;
            await _auditLog.LogAsync("XoaMemHoSo", LoaiSinhVien, sv.MaSV, lyDo);
            await _context.SaveChangesAsync();

            await KhoaTaiKhoanNeuDangHoatDongAsync(sv.MaSV, "Hồ sơ đã bị xóa mềm");
            await tx.CommitAsync();
        }

        // ===================== FR-19, FR-20 Tài khoản sinh viên =====================

        public async Task ResetPasswordAsync(StudentResetPasswordVm dto)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: false);
            var userName = await LayTenDangNhapAsync(sv.MaSV);
            (await _account.ResetPasswordAsync(userName, dto.MatKhauMoi)).ThrowIfFailed();
        }

        public async Task<StudentDetailVm> SetLockAsync(StudentSetLockVm dto)
        {
            // Hồ sơ đã xóa mềm bị từ chối với mọi vai trò nên phải đọc cả bản ghi đã xóa
            var daXoa = await _students.QueryIncludingDeleted().AnyAsync(s => s.MaSV == dto.MaSV && s.DaXoa);
            if (daXoa)
                throw new NghiepVuException("Hồ sơ đã bị xóa mềm, không được mở khóa tài khoản.");

            var sv = await LayHoSoAsync(dto.MaSV, tracking: false);

            if (!dto.Khoa && sv.TrangThai == TrangThaiHocTap.ThoiHoc && !_scope.GetCurrent().IsAdmin)
                throw new NghiepVuException("Chỉ Admin được mở khóa tài khoản sinh viên Thôi học.", StatusCodes.Status403Forbidden);

            var userName = await LayTenDangNhapAsync(sv.MaSV);
            (await _account.SetLockAsync(userName, dto.Khoa, dto.LyDo)).ThrowIfFailed();
            return await GetDetailAsync(sv.MaSV);
        }

        // ===================== FR-30 Cập nhật từ file Excel =====================

        public async Task UpdateFromImportAsync(StudentEditVm dto, string maLop, TrangThaiHocTap trangThai)
        {
            var sv = await LayHoSoAsync(dto.MaSV, tracking: true);
            var lopCu = sv.MaLop;
            var trangThaiCu = sv.TrangThai;
            var doiLop = !string.Equals(lopCu, maLop, StringComparison.OrdinalIgnoreCase);
            var doiTrangThai = trangThaiCu != trangThai;

            if (doiLop) await KiemTraLopAsync(maLop);
            // File không có cột lý do nên hồ sơ Đã tốt nghiệp phải đổi trạng thái tại màn hình chi tiết
            if (doiTrangThai) KiemTraChuyenTrangThai(trangThaiCu, trangThai, _scope.GetCurrent().IsAdmin, lyDo: null);

            await using var tx = await _context.Database.BeginTransactionAsync();

            GanThongTinBatBuoc(sv, dto);
            // Ô trống ở trường liên hệ tùy chọn giữ giá trị hiện có
            sv.SDT = LamSach(dto.SDT) ?? sv.SDT;
            sv.Email = LamSach(dto.Email)?.ToLowerInvariant() ?? sv.Email;
            sv.DiaChi = LamSach(dto.DiaChi) ?? sv.DiaChi;
            await _auditLog.LogAsync("SuaHoSo", LoaiSinhVien, sv.MaSV);

            if (doiLop)
            {
                sv.MaLop = maLop;
                await _auditLog.LogAsync("ChuyenLop", LoaiSinhVien, sv.MaSV, $"{lopCu} -> {maLop}");
            }
            if (doiTrangThai)
            {
                sv.TrangThai = trangThai;
                await _auditLog.LogAsync("DoiTrangThai", LoaiSinhVien, sv.MaSV,
                    $"{trangThaiCu.TenHienThi()} -> {trangThai.TenHienThi()}");
            }
            if (doiLop || doiTrangThai) sv.DaCoBienDong = true;
            await _context.SaveChangesAsync();

            // Thôi học -> Đang học: tài khoản giữ trạng thái khóa, chỉ mở thủ công (FR-20)
            if (doiTrangThai && trangThai == TrangThaiHocTap.ThoiHoc)
                await KhoaTaiKhoanNeuDangHoatDongAsync(sv.MaSV, "Sinh viên chuyển sang Thôi học");

            await tx.CommitAsync();
        }

        // ===================== Hỗ trợ =====================

        private static void GanThongTinBatBuoc(SinhVien sv, StudentEditVm dto)
        {
            var hoTen = dto.HoTen.Trim();
            sv.HoTen = hoTen;
            sv.HoTenKhongDau = TextHelper.BoDau(hoTen);
            sv.NgaySinh = dto.NgaySinh!.Value.Date;
            sv.GioiTinh = dto.GioiTinh!.Value;
        }

        // Cặp không có trong bảng quy tắc: chặn và nêu lý do; cặp chỉ dành cho Admin: HTTP 403
        private static void KiemTraChuyenTrangThai(TrangThaiHocTap cu, TrangThaiHocTap moi, bool isAdmin, string? lyDo)
        {
            if (!TrangThaiDuocChuyen(cu, laAdmin: true).Contains(moi))
                throw new NghiepVuException($"Không được chuyển từ {cu.TenHienThi()} sang {moi.TenHienThi()}.") { Truong = "TrangThai" };
            if (!TrangThaiDuocChuyen(cu, isAdmin).Contains(moi))
                throw new NghiepVuException("Chỉ Admin được thực hiện chuyển trạng thái này.", StatusCodes.Status403Forbidden) { Truong = "TrangThai" };
            if (cu == TrangThaiHocTap.DaTotNghiep && lyDo == null)
                throw new NghiepVuException("Sửa trạng thái hồ sơ Đã tốt nghiệp bắt buộc nhập lý do.") { Truong = "TrangThai" };
        }

        // Hồ sơ không tồn tại hoặc đã xóa mềm: 404; ngoài phạm vi: 403 (UC10)
        private async Task<SinhVien> LayHoSoAsync(string maSV, bool tracking)
        {
            var query = tracking ? _students.Query() : _students.QueryNoTracking();
            var sv = await query.FirstOrDefaultAsync(s => s.MaSV == maSV)
                ?? throw new NghiepVuException("Không tìm thấy sinh viên", StatusCodes.Status404NotFound);

            await _scope.EnsureStudentInScopeAsync(maSV);
            return sv;
        }

        // Lớp phải đang sử dụng (global query filter đã loại lớp xóa mềm) và thuộc phạm vi được phép
        private async Task KiemTraLopAsync(string maLop)
        {
            if (!await _context.Lops.AnyAsync(l => l.MaLop == maLop))
                throw new NghiepVuException("Lớp không tồn tại hoặc đã ngừng sử dụng.");
            await _scope.EnsureClassInScopeAsync(maLop);
        }

        // Bảng quy tắc chuyển trạng thái (mục 3.1.2); các cặp khác không được phép
        private static IEnumerable<TrangThaiHocTap> TrangThaiDuocChuyen(TrangThaiHocTap cu, bool laAdmin) => cu switch
        {
            TrangThaiHocTap.DangHoc => new[] { TrangThaiHocTap.BaoLuu, TrangThaiHocTap.DaTotNghiep, TrangThaiHocTap.ThoiHoc },
            TrangThaiHocTap.BaoLuu => new[] { TrangThaiHocTap.DangHoc, TrangThaiHocTap.ThoiHoc },
            TrangThaiHocTap.ThoiHoc when laAdmin => new[] { TrangThaiHocTap.DangHoc },
            TrangThaiHocTap.DaTotNghiep when laAdmin => new[] { TrangThaiHocTap.DangHoc, TrangThaiHocTap.BaoLuu, TrangThaiHocTap.ThoiHoc },
            _ => Array.Empty<TrangThaiHocTap>()
        };

        private async Task KhoaTaiKhoanNeuDangHoatDongAsync(string maSV, string lyDo)
        {
            var userName = await _context.Users.AsNoTracking()
                .Where(u => u.MaSV == maSV && u.TrangThaiTaiKhoan == TrangThaiTaiKhoan.HoatDong)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
            if (userName != null)
                (await _account.SetLockAsync(userName, true, lyDo)).ThrowIfFailed();
        }

        private async Task<string> LayTenDangNhapAsync(string maSV) =>
            await _context.Users.AsNoTracking()
                .Where(u => u.MaSV == maSV)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync()
                ?? throw new NghiepVuException("Sinh viên chưa có tài khoản.", StatusCodes.Status404NotFound);

        private static string? LamSach(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
