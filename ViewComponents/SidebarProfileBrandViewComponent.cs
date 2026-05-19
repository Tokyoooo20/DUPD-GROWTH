using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.ViewComponents;

public class SidebarProfileBrandViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public SidebarProfileBrandViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool suppressProfilePanel = false)
    {
        var principal = ViewContext.HttpContext.User;
        var fallback = ViewContext.ViewData["SidebarUserName"] as string;
        var vm = await UserProfileSnapshot.CreateAsync(principal, _db, fallback);
        vm.SuppressProfilePanel = suppressProfilePanel;
        return View("~/Views/Client/Profile.cshtml", vm);
    }
}
