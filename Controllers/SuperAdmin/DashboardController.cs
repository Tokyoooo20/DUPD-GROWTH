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

    [HttpGet("Dashboard/Detail/{segment}")]
    public IActionResult DashboardDetail(string segment, int page = 1, int? year = null, string? growth = null)
    {
        var vm = DashboardPapDetailPage.CreateViewModel(segment, page, year, growth);
        if (vm is null)
            return NotFound();

        SetNavbarYearViewData(vm.SelectedYear, detailSegment: vm.Kpi.ActiveSegment, detailPage: page);
        return View("~/Views/SuperAdmin/DashboardDetail.cshtml", vm);
    }

    /// <summary>PAP list with the same UI as dashboard detail; sidebar highlights Reports.</summary>
    [HttpGet("Reports/{segment?}")]
    public IActionResult Reports(string? segment, int page = 1, int? year = null, string? growth = null)
    {
        var seg = string.IsNullOrWhiteSpace(segment) ? DashboardSegments.Total : segment;
        var vm = DashboardPapDetailPage.CreateViewModel(seg, page, year, growth);
        if (vm is null)
            return NotFound();

        SetNavbarYearViewData(vm.SelectedYear, detailSegment: vm.Kpi.ActiveSegment, detailPage: page);
        ViewData["SidebarActive"] = "reports";
        ViewData["Title"] = "Reports";
        ViewData["PapListAction"] = "Reports";
        ViewData["NavbarYearDetailAction"] = "Reports";
        ViewData["DashboardDetailShowPriorityNoColumn"] = true;
        return View("~/Views/SuperAdmin/Reports.cshtml", vm);
    }
}
