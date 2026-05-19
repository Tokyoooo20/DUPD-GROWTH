using DupdGrowth.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Models;

/// <summary>
/// Builds KPI strip / chart payloads for Super Admin office dashboards (projects filtered by <c>ParentId</c>).
/// </summary>
public static class SuperAdminOfficeDashboard
{
    public static int ResolveNavbarYear(int? year)
    {
        return year is null ? 2026 : DashboardPapDetailPage.YearOptions.Contains(year.Value) ? year.Value : 2026;
    }

    public static void SetNavbarYearViewData(Controller controller, int selectedYear)
    {
        controller.ViewData["NavbarSelectedYear"] = selectedYear;
        controller.ViewData["NavbarYears"] = DashboardPapDetailPage.YearOptions;
        controller.ViewData["NavbarSegment"] = null;
        controller.ViewData["NavbarPage"] = null;
    }

    public static async Task<DashboardKpiStripViewModel> BuildKpiForParentOfficeAsync(
        ApplicationDbContext db,
        int parentOfficeId,
        CancellationToken cancellationToken = default)
    {
        var chartProjects = await db.Projects.AsNoTracking()
            .Where(p => p.ParentId == parentOfficeId)
            .ToListAsync(cancellationToken);

        return new DashboardKpiStripViewModel
        {
            PapStatusGroupedChartJson = PapStatusGrowthChart.SerializeGroupedCounts(chartProjects)
        };
    }
}
