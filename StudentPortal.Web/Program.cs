using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Data;
using StudentPortal.Data.Seed;
using StudentPortal.Infrastructure;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;
using StudentPortal.Repositories;
using StudentPortal.Services;
using StudentPortal.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Chuỗi kết nối và DbContext dùng chung
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Chính sách mật khẩu (mục 3.2.1.6): tối thiểu 8 ký tự, gồm cả chữ và số
    // (Identity không có tùy chọn "bắt buộc chữ cái" - dùng LetterPasswordValidator)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;

    // Khóa tạm 15 phút sau 5 lần đăng nhập sai
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Email sinh viên không bắt buộc nên không ép Email duy nhất ở Identity
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager<AppSignInManager>()
.AddPasswordValidator<LetterPasswordValidator>()
.AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>()
.AddDefaultTokenProviders();

// Kiểm tra SecurityStamp và trạng thái tài khoản ở mọi request (FR-08):
// tài khoản bị khóa / đổi quyền bị từ chối hoặc nạp lại vai trò, khoa phụ trách từ yêu cầu kế tiếp
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// Cookie xác thực (NFR-01): HttpOnly, Secure, hết hạn sau 30 phút không hoạt động
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".StudentPortal.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Request Ajax/JSON nhận 401/403 thay vì bị chuyển hướng sang trang HTML
    options.Events.OnRedirectToLogin = context =>
    {
        if (ExceptionMiddleware.LaAjax(context.Request)) context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (ExceptionMiddleware.LaAjax(context.Request)) context.Response.StatusCode = StatusCodes.Status403Forbidden;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// 3. MVC
builder.Services.AddControllersWithViews(options =>
{
    // Anti-forgery cho mọi thao tác thay đổi dữ liệu (mục 3.2.1.5); Ajax gửi token qua header RequestVerificationToken
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<AjaxModelStateFilter>();
});
builder.Services.AddHttpContextAccessor();

// QuestPDF (xuất PDF - FR-28, FR-29) dùng giấy phép Community
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// 4. Repository và dịch vụ (mục 3.2.1.4)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Dịch vụ dùng chung: phạm vi dữ liệu (FR-09), tài khoản, nhật ký (NFR-07)
builder.Services.AddScoped<IScopeService, ScopeService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Dịch vụ nghiệp vụ
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IStudentQueryService, StudentQueryService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();

var app = builder.Build();

// 5. Seed dữ liệu mẫu
try
{
    await DbSeeder.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu mẫu (Seed Data).");
}

// 6. HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    // Trang 500 là file tĩnh để vẫn hiển thị được khi CSDL gặp sự cố (mỗi request đều kiểm tra tài khoản trong CSDL)
    app.UseExceptionHandler(nhanh => nhanh.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/html; charset=utf-8";
        return context.Response.SendFileAsync(app.Environment.WebRootFileProvider.GetFileInfo("500.html"));
    }));
    app.UseHsts();
}

// Trang 403, 404 thân thiện cho trình duyệt; request Ajax vẫn nhận mã HTTP và JSON
app.UseWhen(context => !ExceptionMiddleware.LaAjax(context.Request),
    nhanh => nhanh.UseStatusCodePagesWithReExecute("/Home/Error/{0}"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Xử lý ngoại lệ tập trung: lỗi nghiệp vụ/ngoài phạm vi trả mã HTTP, Ajax nhận JSON.
// Đặt trước Authentication vì mỗi request đều kiểm tra tài khoản trong CSDL (FR-08)
app.UseMiddleware<ExceptionMiddleware>();

// Bắt buộc theo thứ tự: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
