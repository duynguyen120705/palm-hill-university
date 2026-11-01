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
.AddSignInManager<StudentPortal.Services.AppSignInManager>()
.AddDefaultTokenProviders();

// Kiểm tra SecurityStamp và trạng thái tài khoản ở mọi request (FR-08):
// tài khoản bị khóa / đổi quyền bị từ chối hoặc nạp lại vai trò từ yêu cầu kế tiếp
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// Cookie xác thực (NFR-01): HttpOnly, Secure, hết hạn sau 60 phút không hoạt động
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".StudentPortal.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Request Ajax/JSON nhận 401/403 thay vì bị chuyển hướng sang trang HTML
    static bool LaAjax(HttpRequest request) =>
        request.Headers.XRequestedWith == "XMLHttpRequest" ||
        request.Headers.Accept.ToString().Contains("application/json") ||
        (request.ContentType?.StartsWith("application/json") ?? false);

    options.Events.OnRedirectToLogin = context =>
    {
        if (LaAjax(context.Request)) context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (LaAjax(context.Request)) context.Response.StatusCode = StatusCodes.Status403Forbidden;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// 3. Đăng ký dịch vụ MVC
builder.Services.AddControllersWithViews();

// QuestPDF (xuất PDF - FR-28, FR-29) dùng giấy phép Community
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Dịch vụ dùng chung: bộ lọc phạm vi (FR-09), tài khoản, nhật ký (NFR-07)
builder.Services.AddScoped<StudentPortal.Services.IPhamViDuLieuService, StudentPortal.Services.PhamViDuLieuService>();
builder.Services.AddScoped<StudentPortal.Services.ITaiKhoanService, StudentPortal.Services.TaiKhoanService>();

// Dịch vụ nghiệp vụ
builder.Services.AddScoped<StudentPortal.Services.IThongKeService, StudentPortal.Services.ThongKeService>();
builder.Services.AddScoped<StudentPortal.Services.INhatKyService, StudentPortal.Services.NhatKyService>();
builder.Services.AddScoped<StudentPortal.Services.IXuatNhapService, StudentPortal.Services.XuatNhapService>();
builder.Services.AddScoped<StudentPortal.Services.IQuanLyTaiKhoanService, StudentPortal.Services.QuanLyTaiKhoanService>();
builder.Services.AddScoped<StudentPortal.Services.IHoSoCaNhanService, StudentPortal.Services.HoSoCaNhanService>();

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
