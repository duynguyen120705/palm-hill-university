using Microsoft.EntityFrameworkCore;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.Entities;
using StudentPortal.Models.ViewModels;
using StudentPortal.Repositories;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Services
{
    public class CatalogService : ICatalogService
    {
        private const string LoaiKhoa = "Khoa", LoaiKhoaHoc = "KhoaHoc", LoaiLop = "Lop";
        private const string ThemDanhMuc = "ThemDanhMuc", SuaDanhMuc = "SuaDanhMuc", XoaMemDanhMuc = "XoaMemDanhMuc";

        private readonly IRepository<Khoa> _khoas;
        private readonly IRepository<KhoaHoc> _khoaHocs;
        private readonly IRepository<Lop> _lops;
        private readonly IRepository<SinhVien> _sinhViens;
        private readonly IRepository<ApplicationUser> _users;
        private readonly IScopeService _scope;
        private readonly IAuditLogService _auditLog;

        public CatalogService(IRepository<Khoa> khoas, IRepository<KhoaHoc> khoaHocs, IRepository<Lop> lops, IRepository<SinhVien> sinhViens, IRepository<ApplicationUser> users, IScopeService scope, IAuditLogService auditLog)
        {
            _khoas = khoas;
            _khoaHocs = khoaHocs;
            _lops = lops;
            _sinhViens = sinhViens;
            _users = users;
            _scope = scope;
            _auditLog = auditLog;
        }

        public async Task<List<LookupItemDto>> LookupAsync(string type, string? maKhoa, string? maKhoaHoc)
        {
            // Global query filter đã loại danh mục xóa mềm
            switch (type?.ToLowerInvariant())
            {
                case "khoa":
                {
                    // Giáo vụ chỉ thấy khoa phụ trách
                    var scope = _scope.GetCurrent();
                    var khoaPhuTrach = scope.MaKhoaPhuTrach;
                    return await _khoas.QueryNoTracking()
                        .Where(k => scope.IsAdmin || (khoaPhuTrach != null && k.MaKhoa == khoaPhuTrach))
                        .OrderBy(k => k.MaKhoa)
                        .Select(k => new LookupItemDto { Ma = k.MaKhoa, Ten = k.TenKhoa })
                        .ToListAsync();
                }
                case "khoahoc":
                    return await _khoaHocs.QueryNoTracking()
                        .OrderBy(kh => kh.NamNhapHoc)
                        .Select(kh => new LookupItemDto { Ma = kh.MaKhoaHoc, Ten = kh.MaKhoaHoc + " (" + kh.NamNhapHoc + ")" })
                        .ToListAsync();
                case "lop":
                {
                    var query = _scope.ApplyClassScope(_lops.QueryNoTracking());
                    if (!string.IsNullOrWhiteSpace(maKhoa)) query = query.Where(l => l.MaKhoa == maKhoa);
                    if (!string.IsNullOrWhiteSpace(maKhoaHoc)) query = query.Where(l => l.MaKhoaHoc == maKhoaHoc);
                    return await query
                        .OrderBy(l => l.MaLop)
                        .Select(l => new LookupItemDto { Ma = l.MaLop, Ten = l.TenLop, MaKhoa = l.MaKhoa, MaKhoaHoc = l.MaKhoaHoc })
                        .ToListAsync();
                }
                default:
                    throw new NghiepVuException("Loại danh mục không hợp lệ.");
            }
        }

        // ===================== Khoa =====================

        public Task<PagedResult<FacultyListItemDto>> ListFacultiesAsync(CatalogFilterVm filter)
        {
            var scope = _scope.GetCurrent();
            var khoaPhuTrach = scope.MaKhoaPhuTrach;

            return _khoas.QueryNoTracking()
                .Where(k => scope.IsAdmin || (khoaPhuTrach != null && k.MaKhoa == khoaPhuTrach))
                .OrderBy(k => k.MaKhoa)
                .ToPagedResultAsync(filter.Page, filter.PageSize, k => new FacultyListItemDto
                {
                    MaKhoa = k.MaKhoa,
                    TenKhoa = k.TenKhoa,
                    SoLop = k.Lops.Count()
                });
        }

        public async Task CreateFacultyAsync(FacultyCreateVm dto)
        {
            var maKhoa = dto.MaKhoa.Trim();
            // Mã duy nhất kể cả mã đã xóa mềm
            if (await _khoas.QueryIncludingDeleted().AnyAsync(k => k.MaKhoa == maKhoa))
                throw new NghiepVuException($"Mã khoa '{maKhoa}' đã tồn tại hoặc đã từng được sử dụng.", StatusCodes.Status409Conflict);

            _khoas.Add(new Khoa { MaKhoa = maKhoa, TenKhoa = dto.TenKhoa.Trim() });
            await _auditLog.LogAsync(ThemDanhMuc, LoaiKhoa, maKhoa);
            await LuuDanhMucMoiAsync($"Mã khoa '{maKhoa}' đã tồn tại hoặc đã từng được sử dụng.");
        }

        public async Task UpdateFacultyAsync(FacultyEditVm dto)
        {
            var khoa = await _khoas.Query().FirstOrDefaultAsync(k => k.MaKhoa == dto.MaKhoa)
                ?? throw KhongTimThay("khoa");

            khoa.TenKhoa = dto.TenKhoa.Trim();
            await _auditLog.LogAsync(SuaDanhMuc, LoaiKhoa, khoa.MaKhoa);
            await _khoas.SaveChangesAsync();
        }

        public async Task DeleteFacultyAsync(string maKhoa)
        {
            var khoa = await _khoas.Query().FirstOrDefaultAsync(k => k.MaKhoa == maKhoa)
                ?? throw KhongTimThay("khoa");

            if (await _lops.Query().AnyAsync(l => l.MaKhoa == maKhoa))
                throw new NghiepVuException("Không thể xóa: khoa còn lớp đang sử dụng.");
            if (await _users.Query().AnyAsync(u => u.MaKhoaPhuTrach == maKhoa))
                throw new NghiepVuException("Không thể xóa: khoa còn tài khoản Giáo vụ được gán.");

            khoa.DaXoa = true;
            await _auditLog.LogAsync(XoaMemDanhMuc, LoaiKhoa, maKhoa);
            await _khoas.SaveChangesAsync();
        }

        // ===================== Khóa =====================

        public Task<PagedResult<CourseListItemDto>> ListCoursesAsync(CatalogFilterVm filter)
        {
            var scope = _scope.GetCurrent();
            var khoaPhuTrach = scope.MaKhoaPhuTrach;

            // Giáo vụ xem các khóa có lớp thuộc khoa phụ trách (NV03)
            return _khoaHocs.QueryNoTracking()
                .Where(kh => scope.IsAdmin || (khoaPhuTrach != null && kh.Lops.Any(l => l.MaKhoa == khoaPhuTrach)))
                .OrderBy(kh => kh.NamNhapHoc).ThenBy(kh => kh.MaKhoaHoc)
                .ToPagedResultAsync(filter.Page, filter.PageSize, kh => new CourseListItemDto
                {
                    MaKhoaHoc = kh.MaKhoaHoc,
                    NamNhapHoc = kh.NamNhapHoc,
                    SoLop = kh.Lops.Count(),
                    CoSinhVien = kh.Lops.Any(l => l.SinhViens.Any())
                });
        }

        public async Task CreateCourseAsync(CourseCreateVm dto)
        {
            var maKhoaHoc = dto.MaKhoaHoc.Trim();
            var nam = KiemTraNamNhapHoc(dto.NamNhapHoc);

            if (await _khoaHocs.QueryIncludingDeleted().AnyAsync(kh => kh.MaKhoaHoc == maKhoaHoc))
                throw new NghiepVuException($"Mã khóa '{maKhoaHoc}' đã tồn tại hoặc đã từng được sử dụng.", StatusCodes.Status409Conflict);

            _khoaHocs.Add(new KhoaHoc { MaKhoaHoc = maKhoaHoc, NamNhapHoc = nam });
            await _auditLog.LogAsync(ThemDanhMuc, LoaiKhoaHoc, maKhoaHoc);
            await LuuDanhMucMoiAsync($"Mã khóa '{maKhoaHoc}' đã tồn tại hoặc đã từng được sử dụng.");
        }

        public async Task UpdateCourseAsync(CourseEditVm dto)
        {
            var khoaHoc = await _khoaHocs.Query().FirstOrDefaultAsync(kh => kh.MaKhoaHoc == dto.MaKhoaHoc)
                ?? throw KhongTimThay("khóa");
            var nam = KiemTraNamNhapHoc(dto.NamNhapHoc);

            if (nam != khoaHoc.NamNhapHoc &&
                await _sinhViens.Query().AnyAsync(s => s.Lop!.MaKhoaHoc == khoaHoc.MaKhoaHoc))
                throw new NghiepVuException("Khóa đã có lớp chứa sinh viên: giữ cố định năm nhập học.");

            khoaHoc.NamNhapHoc = nam;
            await _auditLog.LogAsync(SuaDanhMuc, LoaiKhoaHoc, khoaHoc.MaKhoaHoc);
            await _khoas.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(string maKhoaHoc)
        {
            var khoaHoc = await _khoaHocs.Query().FirstOrDefaultAsync(kh => kh.MaKhoaHoc == maKhoaHoc)
                ?? throw KhongTimThay("khóa");

            if (await _lops.Query().AnyAsync(l => l.MaKhoaHoc == maKhoaHoc))
                throw new NghiepVuException("Không thể xóa: khóa còn lớp đang sử dụng.");

            khoaHoc.DaXoa = true;
            await _auditLog.LogAsync(XoaMemDanhMuc, LoaiKhoaHoc, maKhoaHoc);
            await _khoas.SaveChangesAsync();
        }

        // ===================== Lớp =====================

        public Task<PagedResult<ClassListItemDto>> ListClassesAsync(CatalogFilterVm filter)
        {
            var query = _scope.ApplyClassScope(_lops.QueryNoTracking());

            if (!string.IsNullOrWhiteSpace(filter.MaKhoa)) query = query.Where(l => l.MaKhoa == filter.MaKhoa);
            if (!string.IsNullOrWhiteSpace(filter.MaKhoaHoc)) query = query.Where(l => l.MaKhoaHoc == filter.MaKhoaHoc);

            return query
                .OrderBy(l => l.MaLop)
                .ToPagedResultAsync(filter.Page, filter.PageSize, l => new ClassListItemDto
                {
                    MaLop = l.MaLop,
                    TenLop = l.TenLop,
                    MaKhoa = l.MaKhoa,
                    TenKhoa = l.Khoa!.TenKhoa,
                    MaKhoaHoc = l.MaKhoaHoc,
                    SoSinhVien = l.SinhViens.Count()
                });
        }

        public async Task CreateClassAsync(ClassCreateVm dto)
        {
            var maLop = dto.MaLop.Trim();
            var tenLop = dto.TenLop.Trim();

            KiemTraKhoaTrongPhamVi(dto.MaKhoa);
            await KiemTraKhoaVaKhoaHocAsync(dto.MaKhoa, dto.MaKhoaHoc);

            if (await _lops.QueryIncludingDeleted().AnyAsync(l => l.MaLop == maLop))
                throw new NghiepVuException($"Mã lớp '{maLop}' đã tồn tại hoặc đã từng được sử dụng.", StatusCodes.Status409Conflict);
            await KiemTraTrungTenLopAsync(tenLop, dto.MaKhoa, dto.MaKhoaHoc, null);

            _lops.Add(new Lop { MaLop = maLop, TenLop = tenLop, MaKhoa = dto.MaKhoa, MaKhoaHoc = dto.MaKhoaHoc });
            await _auditLog.LogAsync(ThemDanhMuc, LoaiLop, maLop);
            await LuuDanhMucMoiAsync($"Mã lớp '{maLop}' hoặc tên lớp '{tenLop}' đã tồn tại.");
        }

        // Thêm danh mục đồng thời cùng mã: yêu cầu sau bị CSDL chặn, trả 409 thay vì 500
        private async Task LuuDanhMucMoiAsync(string thongBaoTrung)
        {
            try { await _khoas.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbLoi.LaTrungKhoa(ex))
            {
                throw new NghiepVuException(thongBaoTrung, StatusCodes.Status409Conflict);
            }
        }

        public async Task UpdateClassAsync(ClassEditVm dto)
        {
            var lop = await _lops.Query().FirstOrDefaultAsync(l => l.MaLop == dto.MaLop)
                ?? throw KhongTimThay("lớp");
            var tenLop = dto.TenLop.Trim();

            // Giáo vụ phải có quyền với cả lớp hiện tại và khoa đích
            await _scope.EnsureClassInScopeAsync(lop.MaLop);
            KiemTraKhoaTrongPhamVi(dto.MaKhoa);
            await KiemTraKhoaVaKhoaHocAsync(dto.MaKhoa, dto.MaKhoaHoc);

            // Thay đổi tổ chức lớp của sinh viên thực hiện bằng chuyển lớp (FR-17)
            var doiToChuc = lop.MaKhoa != dto.MaKhoa || lop.MaKhoaHoc != dto.MaKhoaHoc;
            if (doiToChuc && await _sinhViens.Query().AnyAsync(s => s.MaLop == lop.MaLop))
                throw new NghiepVuException("Lớp còn sinh viên: chỉ được sửa tên lớp, không đổi Khoa/Khóa. Hãy chuyển lớp cho sinh viên trước.");

            await KiemTraTrungTenLopAsync(tenLop, dto.MaKhoa, dto.MaKhoaHoc, lop.MaLop);

            lop.TenLop = tenLop;
            lop.MaKhoa = dto.MaKhoa;
            lop.MaKhoaHoc = dto.MaKhoaHoc;
            await _auditLog.LogAsync(SuaDanhMuc, LoaiLop, lop.MaLop);
            await _khoas.SaveChangesAsync();
        }

        public async Task DeleteClassAsync(string maLop)
        {
            var lop = await _lops.Query().FirstOrDefaultAsync(l => l.MaLop == maLop)
                ?? throw KhongTimThay("lớp");

            await _scope.EnsureClassInScopeAsync(maLop);

            // Global query filter chỉ loại hồ sơ đã xóa mềm nên kiểm tra mọi trạng thái học tập
            if (await _sinhViens.Query().AnyAsync(s => s.MaLop == maLop))
                throw new NghiepVuException("Không thể xóa: lớp còn hồ sơ sinh viên.");

            lop.DaXoa = true;
            await _auditLog.LogAsync(XoaMemDanhMuc, LoaiLop, maLop);
            await _khoas.SaveChangesAsync();
        }

        // ===================== Hỗ trợ =====================

        private static NghiepVuException KhongTimThay(string loai) =>
            new($"Không tìm thấy {loai} hoặc {loai} đã ngừng sử dụng.", StatusCodes.Status404NotFound);

        // Năm nhập học từ 2000 đến năm hiện tại + 1 (mục 3.2.2.1)
        private static int KiemTraNamNhapHoc(int? nam)
        {
            var namToiDa = DateTime.Today.Year + 1;
            if (nam == null || nam < 2000 || nam > namToiDa)
                throw new NghiepVuException($"Năm nhập học phải từ 2000 đến {namToiDa}.");
            return nam.Value;
        }

        // Giáo vụ chỉ thao tác trên khoa phụ trách
        private void KiemTraKhoaTrongPhamVi(string maKhoa)
        {
            var scope = _scope.GetCurrent();
            if (!scope.IsAdmin && !(scope.IsCanBo && scope.MaKhoaPhuTrach == maKhoa))
                throw new ScopeException();
        }

        // Khoa và Khóa phải tồn tại và đang sử dụng (global query filter đã loại bản ghi xóa mềm)
        private async Task KiemTraKhoaVaKhoaHocAsync(string maKhoa, string maKhoaHoc)
        {
            if (!await _khoas.Query().AnyAsync(k => k.MaKhoa == maKhoa))
                throw new NghiepVuException($"Khoa '{maKhoa}' không tồn tại hoặc đã ngừng sử dụng.");
            if (!await _khoaHocs.Query().AnyAsync(kh => kh.MaKhoaHoc == maKhoaHoc))
                throw new NghiepVuException($"Khóa '{maKhoaHoc}' không tồn tại hoặc đã ngừng sử dụng.");
        }

        // Tên lớp duy nhất trong cùng Khoa và Khóa, tính cả lớp đã xóa mềm (UNIQUE ở mục 3.2.2.1)
        private async Task KiemTraTrungTenLopAsync(string tenLop, string maKhoa, string maKhoaHoc, string? maLopHienTai)
        {
            if (await _lops.QueryIncludingDeleted().AnyAsync(l =>
                    l.TenLop == tenLop && l.MaKhoa == maKhoa && l.MaKhoaHoc == maKhoaHoc && l.MaLop != maLopHienTai))
                throw new NghiepVuException($"Tên lớp '{tenLop}' đã có trong cùng Khoa và Khóa.", StatusCodes.Status409Conflict);
        }
    }
}
