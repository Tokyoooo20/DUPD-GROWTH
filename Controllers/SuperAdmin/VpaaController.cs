using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpaaController : Controller
{
    /// <summary>Office of the Vice President for Academic Affairs (VPAA).</summary>
    private const int VpaaOfficeId = 2;

    private readonly ApplicationDbContext _db;

    public VpaaController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("VPAA")]
    public async Task<IActionResult> VPAA(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);

        // PAPs where `projects.parent_id` is VPAA (2)—matches parent office on the project row, not the `offices` tree join.
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, VpaaOfficeId);
        return View("~/Views/SuperAdmin/VPAA.cshtml", kpi);
    }
}
