using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    private void SetNavbarYearViewData(int selectedYear, string? detailSegment, int? detailPage)
    {
        ViewData["NavbarSelectedYear"] = selectedYear;
        ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        ViewData["NavbarSegment"] = detailSegment;
        ViewData["NavbarPage"] = detailPage;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(int? year = null)
    {
        var y = year is null ? 2026 : DashboardPapDetailPage.YearOptions.Contains(year.Value) ? year.Value : 2026;
        SetNavbarYearViewData(y, detailSegment: null, detailPage: null);

        var chartProjects = await _db.Projects.AsNoTracking().ToListAsync();
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

        return View("~/Views/SuperAdmin/Dashboard.cshtml", kpi);
    }

    private bool IsDashboardSwappablePartialRequest() =>
        string.Equals(Request.Headers["X-Dashboard-Partial"], "1", StringComparison.Ordinal);

    [HttpGet("Dashboard/Detail/{segment}")]
    public IActionResult DashboardDetail(string segment, int page = 1, int? year = null, string? growth = null)
    {
        var vm = DashboardPapDetailPage.CreateViewModel(segment, page, year, growth);
        if (vm is null)
            return NotFound();

        SetNavbarYearViewData(vm.SelectedYear, detailSegment: vm.Kpi.ActiveSegment, detailPage: page);
        ViewData["DashboardDetailAjaxNav"] = true;
        if (IsDashboardSwappablePartialRequest())
            return PartialView("~/Views/Shared/_DashboardDetailSwappable.cshtml", vm);
        return View("~/Views/SuperAdmin/DashboardDetail.cshtml", vm);
    }

    /// <summary>PAP list from the <c>projects</c> table (same table UI as dashboard detail).</summary>
    [HttpGet("Reports/{segment?}")]
    public async Task<IActionResult> Reports(string? segment, int page = 1, int? year = null, string? growth = null)
    {
        var seg = string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment;
        if (!DashboardSegments.IsValid(seg))
            return NotFound();

        var allProjects = await _db.Projects.AsNoTracking()
            .OrderBy(p => p.PriorityNo)
            .ThenBy(p => p.Id)
            .ToListAsync();

        var totalAll = allProjects.Count;
        var (completedCount, ongoingCount, notStartedCount) = PapStatusGrowthChart.CountProjectsForKpiStrip(allProjects);
        static string Pct(int n, int tot) =>
            tot > 0 ? $"{Math.Round((double)n / tot * 100)}%" : "0%";

        var growthFilter = ResolveReportsGrowthFilter(growth);
        IEnumerable<Project> afterGrowth = allProjects;
        if (growthFilter is not null)
        {
            afterGrowth = allProjects.Where(p =>
            {
                var g = p.Growth?.Trim();
                if (string.Equals(growthFilter, "N/A", StringComparison.OrdinalIgnoreCase))
                    return string.IsNullOrEmpty(g) || string.Equals(g, "N/A", StringComparison.OrdinalIgnoreCase);
                return string.Equals(g ?? "", growthFilter, StringComparison.OrdinalIgnoreCase);
            });
        }

        var segKey = seg.ToLowerInvariant();
        IEnumerable<Project> segFiltered = segKey switch
        {
            DashboardSegments.Completed => afterGrowth.Where(PapStatusGrowthChart.HasAnyCompletedQuarter),
            DashboardSegments.Ongoing => afterGrowth.Where(PapStatusGrowthChart.HasAnyOngoingQuarter),
            DashboardSegments.NotStarted => afterGrowth.Where(PapStatusGrowthChart.HasAnyNotStartedQuarter),
            _ => afterGrowth
        };

        var filteredList = segFiltered.ToList();
        var totalCount = filteredList.Count;

        if (page < 1)
            page = 1;
        var pageSize = DashboardPapDetailPage.PapListPageSize;
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var pageProjects = filteredList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var rows = pageProjects.Select(ProjectDashboardRowMapper.FromProject).ToList();

        var selectedYear = year is null ? 2026 : DashboardPapDetailPage.YearOptions.Contains(year.Value) ? year.Value : 2026;

        var kpi = new DashboardKpiStripViewModel
        {
            ActiveSegment = segKey,
            TotalPapsValue = totalAll,
            CompletedCount = completedCount,
            OngoingCount = ongoingCount,
            NotStartedCount = notStartedCount,
            CompletedPercent = Pct(completedCount, totalAll),
            OngoingPercent = Pct(ongoingCount, totalAll),
            NotStartedPercent = Pct(notStartedCount, totalAll),
            PapStatusGroupedChartJson = PapStatusGrowthChart.SerializeGroupedCounts(allProjects)
        };

        var vm = new DashboardDetailViewModel
        {
            Kpi = kpi,
            Rows = rows,
            TableSubtitle = ReportsTableSubtitle(segKey),
            SelectedYear = selectedYear,
            GrowthFilter = growthFilter,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        SetNavbarYearViewData(selectedYear, detailSegment: vm.Kpi.ActiveSegment, detailPage: page);
        ViewData["SidebarActive"] = "reports";
        ViewData["Title"] = "Reports";
        ViewData["PapListAction"] = "Reports";
        ViewData["NavbarYearDetailAction"] = "Reports";
        ViewData["DashboardDetailShowPriorityNoColumn"] = true;
        ViewData["DashboardDetailShowRowEdit"] = false;
        ViewData["DashboardDetailShowCompletionPhotoUpload"] = true;
        ViewData["DashboardDetailCompletionPhotoViewOnly"] = true;
        ViewData["ProjectCompletionPhotoUploadUrl"] = null;
        ViewData["DashboardDetailCompletionPhotoPreviewModalId"] = "reportsCompletionPhotoModal";
        ViewData["DashboardDetailAjaxNav"] = true;
        if (IsDashboardSwappablePartialRequest())
            return PartialView("~/Views/Shared/_DashboardDetailSwappable.cshtml", vm);
        return View("~/Views/SuperAdmin/Reports.cshtml", vm);
    }

    private static string? ResolveReportsGrowthFilter(string? growth)
    {
        if (string.IsNullOrWhiteSpace(growth))
            return null;
        var t = growth.Trim();
        return DashboardPapSelectOptions.Growth.FirstOrDefault(g =>
            string.Equals(g, t, StringComparison.OrdinalIgnoreCase));
    }

    private static string ReportsTableSubtitle(string segmentKey) =>
        segmentKey switch
        {
            DashboardSegments.Total => "All PAPs (database)",
            DashboardSegments.Completed => "Completed PAPs (database)",
            DashboardSegments.Ongoing => "Ongoing PAPs (database)",
            DashboardSegments.NotStarted => "Not started PAPs (database)",
            _ => "PAPs"
        };
}
