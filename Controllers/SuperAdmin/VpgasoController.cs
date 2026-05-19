using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VpgasoController : Controller
{
    /// <summary>Office of the Vice President for General Administration and Special Operations (VPGASO).</summary>
    private const int VpgasoOfficeId = 5;

    private readonly ApplicationDbContext _db;

    public VpgasoController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("VPGASO")]
    public async Task<IActionResult> VPGASO(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, VpgasoOfficeId);
        return View("~/Views/SuperAdmin/VPGASO.cshtml", kpi);
    }
}
