using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class ChancellorController : Controller
{
    /// <summary>Office of the Chancellor.</summary>
    private const int ChancellorOfficeId = 6;

    private readonly ApplicationDbContext _db;

    public ChancellorController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("Chancellor")]
    public async Task<IActionResult> Chancellor(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, ChancellorOfficeId);
        return View("~/Views/SuperAdmin/Chancellor.cshtml", kpi);
    }
}
