using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScannerAndPicker.Models;
using System.Diagnostics;

namespace ScannerAndPicker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(
        ILogger<HomeController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult FilePicker()
    {
        var model = new FilePickerViewModel
        {
            ClientId = _configuration["AzureAd:ClientId"] ?? string.Empty,
            TenantId = _configuration["AzureAd:TenantId"] ?? string.Empty,
            SharePointUrl = _configuration["SharePoint:SharePointUrl"] ?? string.Empty,
            OneDriveUrl = _configuration["SharePoint:OneDriveUrl"] ?? string.Empty
        };
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult AuthRedirect()
    {
        ViewBag.ClientId = _configuration["AzureAd:ClientId"] ?? string.Empty;
        ViewBag.TenantId = _configuration["AzureAd:TenantId"] ?? string.Empty;
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
