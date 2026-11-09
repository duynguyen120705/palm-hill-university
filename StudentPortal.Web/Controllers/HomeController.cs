using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Infrastructure.Authorization;
using StudentPortal.Models.ViewModels;
using StudentPortal.Services.Interfaces;

namespace StudentPortal.Controllers;

public class HomeController : Controller
{
    private readonly IStatisticsService _statisticsService;
    private readonly IAccountService _accountService;

    public HomeController(IStatisticsService statisticsService, IAccountService accountService)
    {
        _statisticsService = statisticsService;
        _accountService = accountService;
    }

    // GET: / - trang chủ theo vai trò (FR-08)
    [Authorize]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.SinhVien))
            return View(new HomeVm { HoSo = (await _accountService.GetProfileAsync()).SinhVien });

        // Tổng sinh viên theo trạng thái trong phạm vi được phép
        var thongKe = await _statisticsService.GetAsync(new StatisticsFilterVm { NhomTheo = NhomThongKe.Khoa });
        return View(new HomeVm { SoLieu = thongKe.TongCong });
    }

    // Trang lỗi 403, 404, 500 thân thiện, không lộ thông tin kỹ thuật
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id)
    {
        var ma = id is >= 400 and < 600 ? id.Value : StatusCodes.Status500InternalServerError;
        Response.StatusCode = ma;
        return ma switch
        {
            StatusCodes.Status403Forbidden => View("~/Views/Account/AccessDenied.cshtml"),
            StatusCodes.Status404NotFound => View("NotFound"),
            _ => View()
        };
    }
}
