using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class DashboardController : Controller
{
    private static readonly DashboardKpiStripViewModel DefaultKpi = new();

    private void SetNavbarYearViewData(int selectedYear, string? detailSegment, int? detailPage)
    {
        ViewData["NavbarSelectedYear"] = selectedYear;
        ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        ViewData["NavbarSegment"] = detailSegment;
        ViewData["NavbarPage"] = detailPage;
    }

    [HttpGet("Dashboard")]
    public IActionResult Dashboard(int? year = null)
    {
        var y = year is null ? 2026 : DashboardPapDetailPage.YearOptions.Contains(year.Value) ? year.Value : 2026;
        SetNavbarYearViewData(y, detailSegment: null, detailPage: null);
        return View("~/Views/SuperAdmin/Dashboard.cshtml", DefaultKpi);
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
