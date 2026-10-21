using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình chuỗi kết nối Database và đăng ký DbContext (Mục 5 & 6)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Cấu hình ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình ràng buộc mật khẩu theo SRS mục 3.4: tối thiểu 8 ký tự, gồm cả chữ và số
    // (Identity không có tùy chọn "bắt buộc chữ cái" - phần này kiểm tra ở tầng Service)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;

    // Cấu hình khóa tài khoản
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình người dùng: SRS quy định Email sinh viên không bắt buộc nên không ép Email duy nhất ở Identity
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Đăng ký dịch vụ MVC
builder.Services.AddControllersWithViews();

// QuestPDF (xuất PDF - FR-28, FR-29) dùng giấy phép Community
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Dịch vụ nghiệp vụ (PhamViDuLieuServiceTam là bản tạm, thay khi phân hệ Tài khoản hoàn thành)
builder.Services.AddScoped<StudentPortal.Services.IPhamViDuLieuService, StudentPortal.Services.PhamViDuLieuServiceTam>();
builder.Services.AddScoped<StudentPortal.Services.IThongKeService, StudentPortal.Services.ThongKeService>();
builder.Services.AddScoped<StudentPortal.Services.INhatKyService, StudentPortal.Services.NhatKyService>();
builder.Services.AddScoped<StudentPortal.Services.IXuatNhapService, StudentPortal.Services.XuatNhapService>();

var app = builder.Build();

// 4. Seed dữ liệu mẫu (Mục 8)
try
{
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu mẫu (Seed Data).");
}

// 5. Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bắt buộc theo thứ tự: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
