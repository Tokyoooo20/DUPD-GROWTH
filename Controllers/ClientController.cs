using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> UserDashboard()
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        if (userId is null) return Unauthorized();

        var currentUser = await _db.Users.FindAsync(userId.Value);
        if (currentUser == null) return Unauthorized();

        var query = _db.Projects.AsQueryable();
        if (currentUser.OfficeId.HasValue)
        {
            query = query.Where(p => p.OfficeId == currentUser.OfficeId || p.ParentId == currentUser.OfficeId);
        }

        var totalCount = await query.CountAsync();

        var kpi = new DashboardKpiStripViewModel
        {
            ActiveSegment = null,
            TotalPapsValue = totalCount,
            CompletedPercent = "0%",
            OngoingPercent = totalCount > 0 ? "100%" : "0%",
            NotStartedPercent = "0%"
        };

        ViewData["SidebarActive"] = "dashboard";
        ViewData["Title"] = "Dashboard";
        ViewData["DashboardShellMode"] = "user";
        ViewData["NavbarShowYear"] = false;
        ViewData["DashboardSidebarStartCollapsed"] = true;

        return View("~/Views/Client/UserDashboard.cshtml", kpi);
    }

    /// <summary>Same PAP detail UI as admin; user sidebar and routes under <c>/Pages/User/Project</c>.</summary>
    [HttpGet("User/Project/{segment?}")]
    public async Task<IActionResult> UserProject(string? segment, int page = 1, int? year = null, string? growth = null)
    {
        var seg = string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment;
        var vm = await GetProjectFilteredViewModel("project", "Project", nameof(UserProject), page, year, growth, seg);
        
        ViewData["NavbarSelectedYear"] = vm.SelectedYear;
        ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        ViewData["NavbarSegment"] = seg.ToLowerInvariant();
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

    [HttpGet("User/Edit/{id}")]
    public async Task<IActionResult> EditProject(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        ViewData["SidebarActive"] = "project";
        ViewData["Title"] = "Edit Project";
        ViewData["DashboardShellMode"] = "user";
        ViewData["NavbarShowYear"] = false;

        return View("~/Views/Client/EditProject.cshtml", project);
    }

    [HttpPost("User/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProject(int id, [FromBody] CreatePapRequest request)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

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

        project.PriorityNo = request.PriorityNo;
        project.Paps = request.PapName;
        project.ResponsiblePerson = request.ResponsiblePerson;
        project.Budget = request.Budget;
        project.TimeStart = request.TimeFrameStart;
        project.TimeEnd = request.TimeFrameEnd;
        project.Units = request.SupportOffice;
        project.Growth = request.AlignmentGrowth;
        project.Achieve = request.AlignmentAchieve;
        project.RemarksType = request.RemarksType == "Others" && !string.IsNullOrWhiteSpace(request.RemarksTypeOther) ? request.RemarksTypeOther : request.RemarksType;
        project.Remarks = request.Remarks;
        project.StatusQ1 = request.StatusQ1;
        project.StatusQ2 = request.StatusQ2;
        project.StatusQ3 = request.StatusQ3;
        project.StatusQ4 = request.StatusQ4;
        project.UpdatedAt = DateTime.Now;

        _db.Projects.Update(project);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project updated successfully." });
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

        var currentUser = await _db.Users.FindAsync(userId.Value);
        if (currentUser == null)
            return Unauthorized(new { success = false, errors = new[] { "User not found in database." } });

        var project = new Project
        {
            PriorityNo = request.PriorityNo,
            Paps = request.PapName,
            ResponsiblePerson = request.ResponsiblePerson,
            Budget = request.Budget,
            TimeStart = request.TimeFrameStart,
            TimeEnd = request.TimeFrameEnd,
            Units = request.SupportOffice,
            Growth = request.AlignmentGrowth,
            Achieve = request.AlignmentAchieve,
            RemarksType = request.RemarksType == "Others" && !string.IsNullOrWhiteSpace(request.RemarksTypeOther) ? request.RemarksTypeOther : request.RemarksType,
            Remarks = request.Remarks,
            OfficeId = currentUser.OfficeId,
            ParentId = currentUser.ParentOfficeId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project saved as endorsed." });
    }

    [HttpPost("User/Drop/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DropProject(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound(new { success = false, message = "Project not found." });

        project.ProjectStatus = "dropped";
        project.DroppedAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;

        _db.Projects.Update(project);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project dropped successfully." });
    }

    [HttpPost("User/MoveToDraft/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToDraft(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound(new { success = false, message = "Project not found." });

        project.ProjectStatus = "draft";
        project.DraftAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;

        _db.Projects.Update(project);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project moved to Draft successfully." });
    }

    [HttpPost("User/Cancel/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelProject(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound(new { success = false, message = "Project not found." });

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Project cancelled and removed successfully." });
    }

    [HttpGet("User/Drop")]
    public IActionResult UserDrop() => RedirectToAction(nameof(UserDropped));

    [HttpGet("User/Dropped")]
    public async Task<IActionResult> UserDropped(int page = 1, int? year = null, string? growth = null)
    {
        var vm = await GetProjectFilteredViewModel("dropped", "Dropped", nameof(UserDropped), page, year, growth);
        return View("~/Views/Client/UserDropped.cshtml", vm);
    }

    [HttpGet("User/Completed")]
    public async Task<IActionResult> Completed(int page = 1, int? year = null, string? growth = null)
    {
        var vm = await GetProjectFilteredViewModel("completed", "Completed", nameof(Completed), page, year, growth);
        return View("~/Views/Client/Completed.cshtml", vm);
    }

    [HttpGet("User/Draft")]
    public async Task<IActionResult> Draft(int page = 1, int? year = null, string? growth = null)
    {
        var vm = await GetProjectFilteredViewModel("draft", "Draft", nameof(Draft), page, year, growth);
        return View("~/Views/Client/Draft.cshtml", vm);
    }

    private async Task<DashboardDetailViewModel> GetProjectFilteredViewModel(string sidebarActive, string title, string actionName, int page, int? year, string? growth, string? segment = null)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        var currentUser = userId.HasValue ? await _db.Users.FindAsync(userId.Value) : null;

        var query = _db.Projects.AsQueryable();
        if (currentUser?.OfficeId.HasValue == true)
        {
            query = query.Where(p => p.OfficeId == currentUser.OfficeId || p.ParentId == currentUser.OfficeId);
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(growth))
        {
            query = query.Where(p => p.Growth == growth);
        }

        // Filter by the sidebar category
        if (sidebarActive == "draft")
        {
            query = query.Where(p => p.ProjectStatus == "draft");
        }
        else if (sidebarActive == "dropped")
        {
            query = query.Where(p => p.ProjectStatus == "dropped");
        }
        else if (sidebarActive == "completed")
        {
            // If we had a completed status, we would filter here.
            query = query.Where(p => false);
        }
        else
        {
            // Default: Project list (show everything NOT dropped or draft)
            query = query.Where(p => p.ProjectStatus != "dropped" && p.ProjectStatus != "draft");
        }

        // Segment filtering (if provided)
        if (!string.IsNullOrWhiteSpace(segment) && segment != DashboardSegments.Total)
        {
            // Add custom segment logic here if needed (e.g. by a Status column)
            // For now we keep it simple as requested.
        }

        var totalCount = await query.CountAsync();
        var pageSize = 10;
        var projects = await query
            .OrderBy(p => p.PriorityNo)
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = projects.Select(p => new DashboardPapRow
        {
            Id = p.Id,
            Name = p.Paps,
            ResponsiblePerson = p.ResponsiblePerson,
            BudgetAllocation = p.Budget.ToString("C0", new System.Globalization.CultureInfo("en-PH")),
            TimeFrameStart = p.TimeStart,
            TimeFrameEnd = p.TimeEnd,
            SupportOfficesUnits = p.Units ?? "",
            AlignmentGrowth = p.Growth ?? "N/A",
            AlignmentAchieve = p.Achieve ?? "N/A",
            RemarksContinuingNew = p.RemarksType,
            StatusQ1 = p.StatusQ1 ?? "",
            StatusQ2 = p.StatusQ2 ?? "",
            StatusQ3 = p.StatusQ3 ?? "",
            StatusQ4 = p.StatusQ4 ?? "",
            Remarks = p.Remarks ?? "",
            Status = p.ProjectStatus ?? "Ongoing"
        }).ToList();

        var vm = new DashboardDetailViewModel
        {
            Kpi = new DashboardKpiStripViewModel
            {
                ActiveSegment = segment?.ToLowerInvariant(),
                TotalPapsValue = totalCount,
                CompletedPercent = "0%",
                OngoingPercent = totalCount > 0 ? "100%" : "0%",
                NotStartedPercent = "0%"
            },
            Rows = rows,
            TableSubtitle = title,
            SelectedYear = year ?? 2026,
            GrowthFilter = growth,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        ConfigureUserClientPapTablePage(sidebarActive, title, actionName, vm);
        return vm;
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
