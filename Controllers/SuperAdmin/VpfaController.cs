using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpfaController : Controller
{
    /// <summary>Office of the Vice President for Finance & Administration (VPFA).</summary>
    private const int VpfaOfficeId = 3;

    private readonly ApplicationDbContext _db;

    public VpfaController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("VPFA")]
    public async Task<IActionResult> VPFA(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, VpfaOfficeId);
        return View("~/Views/SuperAdmin/VPFA.cshtml", kpi);
    }
}
