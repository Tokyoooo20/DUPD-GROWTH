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

    /// <summary>Allowed <c>list</c> values for <see cref="UserPapTableData"/> (client PAP table AJAX).</summary>
    private static class ClientPapSearchLists
    {
        public const string Project = "project";
        public const string Completed = "completed";
        public const string Draft = "draft";
        public const string Dropped = "dropped";
    }

    /// <summary>Support Offices/Units label stored on the project; always the logged-in user's office, not client-submitted text.</summary>
    private static string ResolveSupportOfficeUnits(User user, string? officeNameClaim)
    {
        if (user.Office != null && !string.IsNullOrWhiteSpace(user.Office.Name))
            return user.Office.Name.Trim();
        if (user.ParentOffice != null && !string.IsNullOrWhiteSpace(user.ParentOffice.Name))
            return user.ParentOffice.Name.Trim();
        return officeNameClaim?.Trim() ?? "";
    }

    [HttpGet("User/Dashboard")]
    public async Task<IActionResult> UserDashboard()
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        if (userId is null) return Unauthorized();

        var currentUser = await _db.Users.FindAsync(userId.Value);
        if (currentUser == null) return Unauthorized();

        var query = ApplyClientProjectOfficeScope(_db.Projects.AsQueryable(), currentUser);

        var chartProjects = await query
            .AsNoTracking()
            .ToListAsync();

        var totalCount = chartProjects.Count;
        var (completedCount, ongoingCount, notStartedCount) = PapStatusGrowthChart.CountProjectsForKpiStrip(chartProjects);

        var completedPercent = totalCount > 0 ? Math.Round((double)completedCount / totalCount * 100) + "%" : "0%";
        var ongoingPercent = totalCount > 0 ? Math.Round((double)ongoingCount / totalCount * 100) + "%" : "0%";
        var notStartedPercent = totalCount > 0 ? Math.Round((double)notStartedCount / totalCount * 100) + "%" : "0%";

        var kpi = new DashboardKpiStripViewModel
        {
            ActiveSegment = null,
            TotalPapsValue = totalCount,
            CompletedCount = completedCount,
            OngoingCount = ongoingCount,
            NotStartedCount = notStartedCount,
            CompletedPercent = completedPercent,
            OngoingPercent = ongoingPercent,
            NotStartedPercent = notStartedPercent,
            PapStatusGroupedChartJson = PapStatusGrowthChart.SerializeGroupedCounts(chartProjects)
        };

        ViewData["SidebarActive"] = "dashboard";
        ViewData["Title"] = "Dashboard";
        ViewData["DashboardShellMode"] = "user";
        ViewData["NavbarShowYear"] = false;
        ViewData["DashboardSidebarStartCollapsed"] = true;

        return View("~/Views/Client/UserDashboard.cshtml", kpi);
    }

    /// <summary>Same PAP detail UI as admin; user sidebar and routes under <c>/Pages/User/Project</c>.</summary>
    [HttpGet("User/PapTableData")]
    public async Task<IActionResult> UserPapTableData(
        string list,
        string? q = null,
        int page = 1,
        int? year = null,
        string? growth = null,
        string? segment = null)
    {
        string sidebar;
        string title;
        string actionName;

        switch (list?.Trim().ToLowerInvariant())
        {
            case ClientPapSearchLists.Project:
                sidebar = "project";
                title = "Project";
                actionName = nameof(UserProject);
                break;
            case ClientPapSearchLists.Completed:
                sidebar = "completed";
                title = "Completed Projects";
                actionName = nameof(Completed);
                break;
            case ClientPapSearchLists.Draft:
                sidebar = "draft";
                title = "Draft";
                actionName = nameof(Draft);
                break;
            case ClientPapSearchLists.Dropped:
                sidebar = "dropped";
                title = "Dropped";
                actionName = nameof(UserDropped);
                break;
            default:
                return BadRequest();
        }

        var seg = sidebar == ClientPapSearchLists.Project
            ? (string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment!)
            : DashboardSegments.Total;

        var vm = await GetProjectFilteredViewModel(sidebar, title, actionName, page, year, growth, seg, search: q);
        Response.Headers.CacheControl = "no-store";
        return PartialView("~/Views/Shared/_DashboardPapAjaxTableUpdate.cshtml", vm);
    }

    [HttpGet("User/Project/{segment?}")]
    public async Task<IActionResult> UserProject(string? segment, int page = 1, int? year = null, string? growth = null, string? q = null)
    {
        var seg = string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment;
        var vm = await GetProjectFilteredViewModel("project", "Project", nameof(UserProject), page, year, growth, seg, search: q);
        
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

        var editorUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var eid) ? eid : (int?)null;
        if (editorUserId is null)
            return Unauthorized(new { success = false, errors = new[] { "User not authenticated." } });

        var editor = await _db.Users
            .Include(u => u.Office)
            .Include(u => u.ParentOffice)
            .FirstOrDefaultAsync(u => u.Id == editorUserId.Value);
        if (editor == null)
            return Unauthorized(new { success = false, errors = new[] { "User not found in database." } });

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
        project.Units = ResolveSupportOfficeUnits(editor, User.FindFirstValue("OfficeName"));
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

        var currentUser = await _db.Users
            .Include(u => u.Office)
            .Include(u => u.ParentOffice)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (currentUser == null)
            return Unauthorized(new { success = false, errors = new[] { "User not found in database." } });

        var supportUnits = ResolveSupportOfficeUnits(currentUser, User.FindFirstValue("OfficeName"));

        var inScope = ApplyClientProjectOfficeScope(_db.Projects.AsQueryable(), currentUser);
        var priorityTaken = await inScope.AnyAsync(p => p.PriorityNo == request.PriorityNo);
        if (priorityTaken)
        {
            return Conflict(new
            {
                success = false,
                duplicatePriority = true,
                message = "This priority number is already assigned to another project in your office. Please choose a different priority number."
            });
        }

        var project = new Project
        {
            PriorityNo = request.PriorityNo,
            Paps = request.PapName,
            ResponsiblePerson = request.ResponsiblePerson,
            Budget = request.Budget,
            TimeStart = request.TimeFrameStart,
            TimeEnd = request.TimeFrameEnd,
            Units = supportUnits,
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

        if (string.Equals(project.ProjectStatus, "dropped", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { success = false, message = "This project is already dropped.", alreadyInStatus = true });
        }

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

        if (string.Equals(project.ProjectStatus, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { success = false, message = "This project is already in draft status.", alreadyInStatus = true });
        }

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
    public async Task<IActionResult> UserDropped(int page = 1, int? year = null, string? growth = null, string? q = null)
    {
        var vm = await GetProjectFilteredViewModel("dropped", "Dropped", nameof(UserDropped), page, year, growth, segment: null, search: q);
        return View("~/Views/Client/UserDropped.cshtml", vm);
    }

    [HttpGet("User/Completed")]
    public async Task<IActionResult> Completed(int page = 1, int? year = null, string? growth = null, string? q = null)
    {
        var vm = await GetProjectFilteredViewModel("completed", "Completed", nameof(Completed), page, year, growth, segment: null, search: q);
        return View("~/Views/Client/Completed.cshtml", vm);
    }

    [HttpGet("User/Draft")]
    public async Task<IActionResult> Draft(int page = 1, int? year = null, string? growth = null, string? q = null)
    {
        var vm = await GetProjectFilteredViewModel("draft", "Draft", nameof(Draft), page, year, growth, segment: null, search: q);
        return View("~/Views/Client/Draft.cshtml", vm);
    }

    private async Task<DashboardDetailViewModel> GetProjectFilteredViewModel(string sidebarActive, string title, string actionName, int page, int? year, string? growth, string? segment = null, string? search = null)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        var currentUser = userId.HasValue ? await _db.Users.FindAsync(userId.Value) : null;

        var query = ApplyClientProjectOfficeScope(_db.Projects.AsQueryable(), currentUser);

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
            // Filter projects where at least one quarter is marked as "Completed"
            query = query.Where(p => 
                p.ProjectStatus != "dropped" && 
                p.ProjectStatus != "draft" &&
                (p.StatusQ1 == "Completed" || 
                 p.StatusQ2 == "Completed" || 
                 p.StatusQ3 == "Completed" || 
                 p.StatusQ4 == "Completed"));
        }
        else if (sidebarActive == "project")
        {
            // User Project table: omit draft and dropped (those have dedicated pages).
            query = query.Where(p => p.ProjectStatus != "dropped" && p.ProjectStatus != "draft");
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

        query = ApplyClientProjectKeywordSearch(query, search);

        var totalCount = await query.CountAsync();
        var pageSize = 10;
        var projects = await query
            .OrderBy(p => p.PriorityNo)
            .ThenBy(p => p.Id)
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
                CompletedCount = 0,
                OngoingCount = 0,
                NotStartedCount = 0,
                CompletedPercent = "0%",
                OngoingPercent = totalCount > 0 ? "100%" : "0%",
                NotStartedPercent = "0%"
            },
            Rows = rows,
            TableSubtitle = title,
            SelectedYear = year ?? 2026,
            GrowthFilter = growth,
            SearchQuery = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
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
        ViewData["DashboardDetailShowTableToolbar"] = true;
        ViewData["DashboardDetailShowPriorityNoColumn"] = true;
        ViewData["DashboardDetailHidePapEditModal"] = true;
        ViewData["DashboardDetailShowRowDelete"] = false;
        ViewData["DashboardDetailShowRowEdit"] = false;
        ViewData["DashboardSidebarStartCollapsed"] = true;
        ViewData["PapSearchListKey"] =
            sidebarActive.Equals(ClientPapSearchLists.Dropped, StringComparison.OrdinalIgnoreCase)
                ? ClientPapSearchLists.Dropped
                : sidebarActive;
        ViewData["UserReportToolbarShowAdd"] =
            sidebarActive.Equals(ClientPapSearchLists.Project, StringComparison.OrdinalIgnoreCase);
        ViewData["UserReportToolbarShowSearch"] = true;
    }

    /// <summary>
    /// Client-visible projects: same office hierarchy as <see cref="Project"/> rows created for the user
    /// (<c>OfficeId</c> / <c>ParentId</c> from the signed-in user's office fields). No global list when offices are unset.
    /// </summary>
    /// <summary>AND-of-tokens substring match across common project text fields.</summary>
    private static IQueryable<Project> ApplyClientProjectKeywordSearch(IQueryable<Project> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var tokens = search.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in tokens)
        {
            var term = raw;
            decimal? amtExact =
                decimal.TryParse(term.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amt)
                    ? amt
                    : null;
            int? prExact =
                int.TryParse(term.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var pn)
                    ? pn
                    : null;

            query = query.Where(p =>
                (p.Paps != null && p.Paps.Contains(term)) ||
                (p.ResponsiblePerson != null && p.ResponsiblePerson.Contains(term)) ||
                (p.Units != null && p.Units.Contains(term)) ||
                (p.Remarks != null && p.Remarks.Contains(term)) ||
                (!string.IsNullOrEmpty(p.RemarksType) && p.RemarksType.Contains(term)) ||
                (p.Growth != null && p.Growth.Contains(term)) ||
                (p.Achieve != null && p.Achieve.Contains(term)) ||
                (p.StatusQ1 != null && p.StatusQ1.Contains(term)) ||
                (p.StatusQ2 != null && p.StatusQ2.Contains(term)) ||
                (p.StatusQ3 != null && p.StatusQ3.Contains(term)) ||
                (p.StatusQ4 != null && p.StatusQ4.Contains(term)) ||
                (p.ProjectStatus != null && p.ProjectStatus.Contains(term)) ||
                (p.TimeStart != null && p.TimeStart.Contains(term)) ||
                (p.TimeEnd != null && p.TimeEnd.Contains(term)) ||
                (prExact.HasValue && p.PriorityNo == prExact.Value) ||
                (amtExact.HasValue && p.Budget == amtExact.Value));
        }

        return query;
    }

    private static IQueryable<Project> ApplyClientProjectOfficeScope(IQueryable<Project> query, User? user)
    {
        if (user is null)
            return query.Where(_ => false);

        if (user.OfficeId.HasValue)
        {
            var oid = user.OfficeId.Value;
            return query.Where(p => p.OfficeId == oid || p.ParentId == oid);
        }

        if (user.ParentOfficeId.HasValue)
        {
            var pid = user.ParentOfficeId.Value;
            return query.Where(p => p.OfficeId == pid || p.ParentId == pid);
        }

        return query.Where(_ => false);
    }
}
