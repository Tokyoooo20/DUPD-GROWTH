using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class UniversityPresidentController : Controller
{
    /// <summary>Office of the University President.</summary>
    private const int UniversityPresidentOfficeId = 1;

    private readonly ApplicationDbContext _db;

    public UniversityPresidentController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("University-President")]
    public async Task<IActionResult> UniversityPresident(int? year = null)
    {
        var y = SuperAdminOfficeDashboard.ResolveNavbarYear(year);
        SuperAdminOfficeDashboard.SetNavbarYearViewData(this, y);
        var kpi = await SuperAdminOfficeDashboard.BuildKpiForParentOfficeAsync(_db, UniversityPresidentOfficeId);
        return View("~/Views/SuperAdmin/University President.cshtml", kpi);
    }
}
