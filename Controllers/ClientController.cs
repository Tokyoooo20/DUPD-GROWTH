using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers;

[Authorize]
[Route("Pages")]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _db;

    public ClientController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("User/Dashboard")]
    public IActionResult UserDashboard() =>
        View("~/Views/Client/UserDashboard.cshtml", new DashboardKpiStripViewModel());

    /// <summary>Same PAP detail UI as admin; user sidebar and routes under <c>/Pages/User/Project</c>.</summary>
    [HttpGet("User/Project/{segment?}")]
    public IActionResult UserProject(string? segment, int page = 1, int? year = null, string? growth = null)
    {
        var seg = string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment;
        var vm = DashboardPapDetailPage.CreateViewModel(seg, page, year, growth);
        if (vm is null)
            return NotFound();

        ViewData["NavbarSelectedYear"] = vm.SelectedYear;
        ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        ViewData["NavbarSegment"] = vm.Kpi.ActiveSegment;
        ViewData["NavbarPage"] = page;

        ViewData["SidebarActive"] = "project";
        ViewData["Title"] = "Project";
        ViewData["NavbarShowYear"] = false;
        ViewData["DashboardShellMode"] = "user";
        ViewData["PapListController"] = "Client";
        ViewData["PapListAction"] = "UserProject";
        ViewData["DetailBackController"] = "Client";
        ViewData["DetailBackAction"] = "UserDashboard";
        ViewData["DetailBackIncludeYear"] = false;
        ViewData["NavbarYearDetailAction"] = "UserProject";
        ViewData["DashboardSidebarStartCollapsed"] = true;

        return View("~/Views/Client/UserProject.cshtml", vm);
    }

    [HttpGet("User/Create")]
    public IActionResult CreateNew()
    {
        ViewData["SidebarActive"] = "createNew";
        ViewData["Title"] = "Create new";
        ViewData["DashboardShellMode"] = "user";
        ViewData["NavbarShowYear"] = false;
        return View("~/Views/Client/CreateNew.cshtml");
    }

    [HttpPost("User/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNew([FromBody] CreatePapRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        var userId = int.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        if (userId is null)
            return Unauthorized(new { success = false, errors = new[] { "User not authenticated." } });

        var pap = new Pap
        {
            PriorityNo = request.PriorityNo,
            PapName = request.PapName,
            ResponsiblePerson = request.ResponsiblePerson,
            Budget = request.Budget,
            TimeFrameStart = request.TimeFrameStart,
            TimeFrameEnd = request.TimeFrameEnd,
            SupportOffice = request.SupportOffice,
            AlignmentGrowth = request.AlignmentGrowth,
            AlignmentAchieve = request.AlignmentAchieve,
            RemarksType = request.RemarksType,
            RemarksTypeOther = request.RemarksTypeOther,
            Remarks = request.Remarks,
            Status = "Endorsed",
            CreatedByUserId = userId.Value
        };

        _db.Paps.Add(pap);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project saved as endorsed." });
    }

    [HttpGet("User/Drop")]
    public IActionResult UserDrop() => RedirectToAction(nameof(UserDropped));

    [HttpGet("User/Dropped")]
    public IActionResult UserDropped(int page = 1, int? year = null, string? growth = null)
    {
        var vm = DashboardPapDetailPage.CreateFilteredListViewModel(
            DashboardPapDetailPage.GetDroppedSampleRows(),
            page,
            year,
            growth);

        ConfigureUserClientPapTablePage("dropped", "Dropped", nameof(UserDropped), vm);
        return View("~/Views/Client/UserDropped.cshtml", vm);
    }

    [HttpGet("User/Completed")]
    public IActionResult Completed(int page = 1, int? year = null, string? growth = null)
    {
        var vm = DashboardPapDetailPage.CreateViewModel(DashboardSegments.Completed, page, year, growth);
        if (vm is null)
            return NotFound();

        ConfigureUserClientPapTablePage("completed", "Completed", nameof(Completed), vm);
        return View("~/Views/Client/Completed.cshtml", vm);
    }

    [HttpGet("User/Draft")]
    public IActionResult Draft(int page = 1, int? year = null, string? growth = null)
    {
        var vm = DashboardPapDetailPage.CreateFilteredListViewModel(
            DashboardPapDetailPage.GetDraftSampleRows(),
            page,
            year,
            growth);

        ConfigureUserClientPapTablePage("draft", "Draft", nameof(Draft), vm);
        return View("~/Views/Client/Draft.cshtml", vm);
    }

    private void ConfigureUserClientPapTablePage(string sidebarActive, string title, string papListAction, DashboardDetailViewModel vm)
    {
        ViewData["SidebarActive"] = sidebarActive;
        ViewData["Title"] = title;
        ViewData["DashboardShellMode"] = "user";
        ViewData["NavbarShowYear"] = false;
        ViewData["NavbarSelectedYear"] = vm.SelectedYear;
        ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        ViewData["PapListController"] = "Client";
        ViewData["PapListAction"] = papListAction;
        ViewData["DetailBackController"] = "Client";
        ViewData["DetailBackAction"] = "UserDashboard";
        ViewData["DetailBackIncludeYear"] = false;

        ViewData["DashboardDetailShowBackLink"] = false;
        ViewData["DashboardDetailShowKpiStrip"] = false;
        ViewData["DashboardDetailShowGrowthFilter"] = false;
        ViewData["DashboardDetailMainHeading"] = title;
        ViewData["DashboardDetailShowTableToolbar"] = false;
        ViewData["DashboardDetailShowPriorityNoColumn"] = true;
        ViewData["DashboardDetailHidePapEditModal"] = true;
        ViewData["DashboardDetailShowRowDelete"] = false;
        ViewData["DashboardDetailShowRowEdit"] = false;
        ViewData["DashboardSidebarStartCollapsed"] = true;
    }
}
