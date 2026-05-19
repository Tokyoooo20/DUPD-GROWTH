using System.Globalization;

namespace DupdGrowth.Web.Models;

/// <summary>Maps <see cref="Project"/> entities to <see cref="DashboardPapRow"/> for dashboard / Reports tables.</summary>
public static class ProjectDashboardRowMapper
{
    private static readonly CultureInfo BudgetCulture = new("en-PH");

    public static DashboardPapRow FromProject(Project p) =>
        new()
        {
            Id = p.Id,
            PriorityNo = p.PriorityNo,
            Name = p.Paps,
            ResponsiblePerson = p.ResponsiblePerson,
            BudgetAllocation = p.Budget.ToString("C0", BudgetCulture),
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
            CompletionPhotoPath = p.CompletionPhotoPath,
            Remarks = p.Remarks ?? "",
            Status = DeriveDisplayStatus(p)
        };

    private static string DeriveDisplayStatus(Project p)
    {
        var s = p.ProjectStatus?.Trim();
        if (!string.IsNullOrEmpty(s))
        {
            if (string.Equals(s, "draft", StringComparison.OrdinalIgnoreCase))
                return "Draft";
            if (string.Equals(s, "dropped", StringComparison.OrdinalIgnoreCase))
                return "Dropped";
        }

        if (PapStatusGrowthChart.HasAnyCompletedQuarter(p))
            return "Completed";
        if (PapStatusGrowthChart.HasAnyOngoingQuarter(p))
            return "Ongoing";
        if (PapStatusGrowthChart.HasAnyNotStartedQuarter(p))
            return "Not started";
        return "Ongoing";
    }
}
