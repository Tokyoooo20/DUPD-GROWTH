using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class VprieController : Controller
{
    /// <summary>Office of the Vice President for Research, Innovation, and Extension (VPRIE).</summary>
    private const int VprieOfficeId = 4;

    private readonly ApplicationDbContext _db;

    public VprieController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("VPRIE")]
    public async Task<IActionResult> VPRIE(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, VprieOfficeId);
        return View("~/Views/SuperAdmin/VPRIE.cshtml", kpi);
    }
}
