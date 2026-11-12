using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;

namespace StudentPortal.Data.Seed
{
    // Dữ liệu mẫu theo mục 3.2.2.5: 3 vai trò, 1 Admin, 3 Giáo vụ, 4 khoa, 4 khóa, khoảng 30 lớp, khoảng 1.000 sinh viên.
    // Mật khẩu đọc từ cấu hình (user-secrets/biến môi trường), không commit lên Git:
    //   Seed:AdminPassword - tài khoản Admin; Seed:DemoPassword - Giáo vụ và sinh viên mẫu
    public static class DbSeeder
    {
        private const int SoSinhVien = 1000;
        private const int SoHoSoXoaMem = 10;
        private const string NguoiTaoMau = "admin01";

        private static readonly (string Ma, string Ten)[] DanhSachKhoa =
        {
            ("CNTT", "Công nghệ thông tin"),
            ("QTKD", "Quản trị kinh doanh"),
            ("KT", "Kế toán"),
            ("NNA", "Ngôn ngữ Anh")
        };

        private static readonly int[] NamNhapHoc = { 2022, 2023, 2024, 2025 };
        private static readonly string[] HauToLop = { "A", "B" };

        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await db.Database.MigrateAsync();

            foreach (var role in new[] { Roles.Admin, Roles.CanBo, Roles.SinhVien })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            await ChuanHoaHoTenDaCoAsync(db);
            if (await db.Users.AnyAsync()) return; // đã có dữ liệu thì bỏ qua

            var hasher = new PasswordHasher<ApplicationUser>();
            var hashAdmin = hasher.HashPassword(null!, DocMatKhau(config, "Seed:AdminPassword"));
            // Băm một lần rồi dùng chung cho dữ liệu mẫu: băm riêng cho 1.000 tài khoản làm chậm lần khởi động đầu
            var hashMau = hasher.HashPassword(null!, DocMatKhau(config, "Seed:DemoPassword"));

            var lops = TaoDanhMuc(db);
            await db.SaveChangesAsync();

            var sinhViens = TaoSinhVien(lops);
            db.SinhViens.AddRange(sinhViens);
            await db.SaveChangesAsync();

            var roleIds = await db.Roles.ToDictionaryAsync(r => r.Name!, r => r.Id);
            ThemTaiKhoan(db, roleIds, "admin01", "Quản trị viên hệ thống", null, null, Roles.Admin, hashAdmin,
                TrangThaiTaiKhoan.HoatDong, null, DateTime.UtcNow);
            ThemTaiKhoan(db, roleIds, "gvcntt01", "Trần Thị Hồng Nhung", "CNTT", null, Roles.CanBo, hashMau,
                TrangThaiTaiKhoan.HoatDong, null, DateTime.UtcNow);
            ThemTaiKhoan(db, roleIds, "gvqtkd01", "Lê Văn Phúc", "QTKD", null, Roles.CanBo, hashMau,
                TrangThaiTaiKhoan.HoatDong, null, DateTime.UtcNow);
            ThemTaiKhoan(db, roleIds, "gvkt01", "Phạm Thị Thu Hà", "KT", null, Roles.CanBo, hashMau,
                TrangThaiTaiKhoan.HoatDong, null, DateTime.UtcNow);

            var faker = new Faker("vi") { Random = new Randomizer(20261108) };
            foreach (var sv in sinhViens)
            {
                var biKhoa = sv.DaXoa || sv.TrangThai == TrangThaiHocTap.ThoiHoc;
                var lyDo = sv.DaXoa ? "Hồ sơ đã xóa mềm" : biKhoa ? "Sinh viên thôi học" : null;
                // Một phần sinh viên chưa từng đăng nhập để kiểm thử điều kiện xóa của Giáo vụ (FR-18)
                DateTime? dangNhapCuoi = !sv.DaXoa && faker.Random.Bool(0.7f)
                    ? faker.Date.Past(1, DateTime.UtcNow)
                    : null;
                ThemTaiKhoan(db, roleIds, sv.MaSV, null, null, sv.MaSV, Roles.SinhVien, hashMau, biKhoa
                    ? TrangThaiTaiKhoan.BiKhoa : TrangThaiTaiKhoan.HoatDong, lyDo, dangNhapCuoi);
            }
            await db.SaveChangesAsync();
        }

        private static string DocMatKhau(IConfiguration config, string khoa)
        {
            var matKhau = config[khoa];
            if (string.IsNullOrEmpty(matKhau))
                throw new InvalidOperationException(
                    $"Thiếu cấu hình {khoa}. Chạy: dotnet user-secrets set \"{khoa}\" \"<mật khẩu>\" --project StudentPortal.Web");
            if (!ValidationRules.IsMatKhauManh(matKhau))
                throw new InvalidOperationException($"{khoa}: {ValidationRules.MatKhauMessage}");
            return matKhau;
        }

        private static List<Lop> TaoDanhMuc(AppDbContext db)
        {
            db.Khoas.AddRange(DanhSachKhoa.Select(k => new Khoa { MaKhoa = k.Ma, TenKhoa = k.Ten }));
            db.KhoaHocs.AddRange(NamNhapHoc.Select(n => new KhoaHoc { MaKhoaHoc = "K" + n, NamNhapHoc = n }));

            var lops = (from k in DanhSachKhoa
                        from n in NamNhapHoc
                        from h in HauToLop
                        let yy = (n % 100).ToString("00")
                        select new Lop
                        {
                            MaLop = k.Ma + yy + h,
                            TenLop = k.Ten + " " + yy + h,
                            MaKhoa = k.Ma,
                            MaKhoaHoc = "K" + n
                        }).ToList();
            db.Lops.AddRange(lops);
            return lops;
        }

        // Sửa dữ liệu đã seed trước đây: họ tên chứa Ð/ð và cột HoTenKhongDau còn chữ ð (FR-22). Chạy lại nhiều lần không đổi kết quả
        private static async Task ChuanHoaHoTenDaCoAsync(AppDbContext db)
        {
            var canSua = await db.SinhViens.IgnoreQueryFilters()
                .Where(s => s.HoTen.Contains("Ð") || s.HoTen.Contains("ð") || s.HoTenKhongDau.Contains("ð"))
                .ToListAsync();
            foreach (var sv in canSua)
            {
                sv.HoTen = TextHelper.ChuanHoaHoTen(sv.HoTen);
                sv.HoTenKhongDau = TextHelper.BoDau(sv.HoTen);
            }
            if (canSua.Count > 0) await db.SaveChangesAsync();
        }

        private static List<SinhVien> TaoSinhVien(List<Lop> lops)
        {
            // Seed cố định để dữ liệu mẫu giống nhau giữa các máy
            var f = new Faker("vi") { Random = new Randomizer(20261108) };
            var ds = new List<SinhVien>(SoSinhVien);

            for (int i = 0; i < SoSinhVien; i++)
            {
                var lop = lops[i % lops.Count];
                var nam = int.Parse(lop.MaKhoaHoc[1..]);
                var maKhoaSo = Array.FindIndex(DanhSachKhoa, k => k.Ma == lop.MaKhoa) + 1;
                var gioiTinh = f.PickRandom(GioiTinh.Nam, GioiTinh.Nu);
                // Dữ liệu tiếng Việt của Bogus có tên viết bằng Ð (U+00D0) thay cho Đ (FR-22)
                var hoTen = TextHelper.ChuanHoaHoTen(f.Name.LastName() + " " + f.Name.FirstName());
                var trangThai = f.Random.WeightedRandom(
                    new[] { TrangThaiHocTap.DangHoc, TrangThaiHocTap.BaoLuu, TrangThaiHocTap.DaTotNghiep, TrangThaiHocTap.ThoiHoc },
                    new[] { 0.70f, 0.10f, 0.12f, 0.08f });
                var maSV = $"{nam % 100:00}{maKhoaSo:00}{i + 1:0000}";
                var ngayTao = new DateTime(nam, 9, 5, 8, 0, 0, DateTimeKind.Utc);

                ds.Add(new SinhVien
                {
                    MaSV = maSV,
                    HoTen = hoTen,
                    HoTenKhongDau = TextHelper.BoDau(hoTen),
                    NgaySinh = new DateTime(nam - 18, 1, 1).AddDays(f.Random.Int(0, 364)),
                    GioiTinh = gioiTinh,
                    MaLop = lop.MaLop,
                    SDT = "0" + f.Random.ReplaceNumbers("#########"),
                    Email = maSV.ToLowerInvariant() + "@sv.edu.vn",
                    DiaChi = f.Address.City(),
                    TrangThai = trangThai,
                    DaCoBienDong = trangThai != TrangThaiHocTap.DangHoc,
                    NgayTao = ngayTao
                });
            }

            // Hồ sơ đã xóa mềm để kiểm thử các quy tắc xóa mềm
            foreach (var sv in ds.Where(s => s.TrangThai == TrangThaiHocTap.DangHoc).Take(SoHoSoXoaMem))
            {
                sv.DaXoa = true;
                sv.NgayXoa = DateTime.UtcNow;
                sv.NguoiXoa = NguoiTaoMau;
                sv.LyDoXoa = "Nhập sai mã sinh viên";
            }
            return ds;
        }

        private static void ThemTaiKhoan(AppDbContext db, Dictionary<string, string> roleIds, string userName, string? hoTen,
            string? maKhoaPhuTrach, string? maSV, string vaiTro, string passwordHash, TrangThaiTaiKhoan trangThai,
            string? lyDoKhoa, DateTime? dangNhapCuoi)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                HoTen = hoTen,
                MaKhoaPhuTrach = maKhoaPhuTrach,
                MaSV = maSV,
                TrangThaiTaiKhoan = trangThai,
                LyDoKhoa = lyDoKhoa,
                LanDangNhapCuoi = vaiTro == Roles.SinhVien ? dangNhapCuoi : null,
                PasswordHash = passwordHash,
                SecurityStamp = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                LockoutEnabled = true,
                LockoutEnd = trangThai == TrangThaiTaiKhoan.BiKhoa ? DateTimeOffset.MaxValue : null
            };
            db.Users.Add(user);
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = roleIds[vaiTro] });
        }
    }
}
