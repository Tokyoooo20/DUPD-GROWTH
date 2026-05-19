namespace DupdGrowth.Web.Models;

public static class DashboardSegments
{
    public const string Total = "total";
    public const string Completed = "completed";
    public const string Ongoing = "ongoing";
    public const string NotStarted = "not-started";

    public static bool IsValid(string? segment) =>
        segment is not null && Valid.Contains(segment);

    private static readonly HashSet<string> Valid = new(StringComparer.OrdinalIgnoreCase)
    {
        Total,
        Completed,
        Ongoing,
        NotStarted
    };
}

public class DashboardKpiStripViewModel
{
    /// <summary>When set (detail page), the matching KPI card is highlighted.</summary>
    public string? ActiveSegment { get; init; }

    public int TotalPapsValue { get; init; }

    /// <summary>Projects with at least one quarter marked Completed (same rule as client dashboard query).</summary>
    public int CompletedCount { get; init; }

    public int OngoingCount { get; init; }

    public int NotStartedCount { get; init; }

    public string CompletedPercent { get; init; } = "21.1%";
    public string OngoingPercent { get; init; } = "32.0%";
    public string NotStartedPercent { get; init; } = "46.9%";

    /// <summary>When set (e.g. client user dashboard), JSON for the Status of PAPs grouped bar chart (see <see cref="PapStatusGrowthChart"/>).</summary>
    public string? PapStatusGroupedChartJson { get; init; }
}

/// <summary>Dropdown option lists for PAP detail table (GROWTH, ACHIEVE, quarterly status).</summary>
public static class DashboardPapSelectOptions
{
    public static readonly string[] QuarterStatuses =
    [
        "To be Implemented",
        "On going",
        "Completed",
        "N/A"
    ];

    /// <summary>Quarter options for user Program/Project forms (Create/Edit); N/A is not offered in Q1–Q4 dropdowns.</summary>
    public static readonly string[] QuarterStatusesProgramForm =
    [
        "To be Implemented",
        "On going",
        "Completed",
    ];

    public static readonly string[] Growth =
    [
        "I. Global Recognition",
        "II. Research Excellence",
        "III. Optimized Accreditation",
        "IV. World-Class Digital Transformation",
        "V. Thriving Curriculum",
        "VI. Holistic Development",
        "N/A"
    ];

    public static readonly string[] Achieve =
    [
        "Expanded & Impact-Driven Internationalization",
        "Harmonized SDG-based HE R&I",
        "Effective & Efficient Public Service",
        "Integrated Real-Time Data& Analytics",
        "Centralized One-Nation Human Capital Dev.Plan",
        "Vitalized Policies, Internal System & Governance",
        "Integrated Real-Time Data",
        "Advanced & Accessible Lifelong Learning",
        "Effective Public Service",
        "Expanded Internationalization",
        "Vitalized Policies & Governance"
    ];
}

public record PapSelectCellModel(
    IReadOnlyList<string> Options,
    string CurrentValue,
    string CssClass,
    string AriaLabel);

/// <summary>Single completion photo on the client Project table (shown when any quarter is Completed).</summary>
public record PapCompletionPhotoUploadCellModel(int ProjectId, string? PhotoPath);

public static class DashboardPapRowCompletion
{
    /// <summary>
    /// True when any quarter is <c>Completed</c>. Matches the edit form: after the first Completed, later quarters may be empty/disabled.
    /// </summary>
    public static bool IsEligibleForCompletionPhoto(DashboardPapRow row)
    {
        static bool IsCompleted(string s) =>
            string.Equals(s?.Trim(), "Completed", StringComparison.OrdinalIgnoreCase);

        return IsCompleted(row.StatusQ1)
            || IsCompleted(row.StatusQ2)
            || IsCompleted(row.StatusQ3)
            || IsCompleted(row.StatusQ4);
    }
}

public class DashboardPapRow
{
    public int Id { get; init; }

    /// <summary>When set (DB-backed rows), shown in the Priority No. column; otherwise the table uses page order.</summary>
    public int? PriorityNo { get; init; }

    public string Name { get; init; } = "";
    public string ResponsiblePerson { get; init; } = "";
    public string BudgetAllocation { get; init; } = "";
    public string TimeFrameStart { get; init; } = "";
    public string TimeFrameEnd { get; init; } = "";
    public string SupportOfficesUnits { get; init; } = "";
    public string AlignmentGrowth { get; init; } = "";
    public string AlignmentAchieve { get; init; } = "";
    /// <summary>Continuing, New, etc.</summary>
    public string RemarksContinuingNew { get; init; } = "";
    public string StatusQ1 { get; init; } = "";
    public string StatusQ2 { get; init; } = "";
    public string StatusQ3 { get; init; } = "";
    public string StatusQ4 { get; init; } = "";
    public string? CompletionPhotoPath { get; init; }
    public string Remarks { get; init; } = "";
    /// <summary>Overall status used for dashboard segment filters.</summary>
    public string Status { get; init; } = "";
}

public class DashboardDetailViewModel
{
    public required DashboardKpiStripViewModel Kpi { get; init; }
    public required IReadOnlyList<DashboardPapRow> Rows { get; init; }
    public required string TableSubtitle { get; init; }

    /// <summary>Reporting year (matches top bar year dropdown).</summary>
    public int SelectedYear { get; init; } = 2026;

    /// <summary>When set, only rows with this GROWTH alignment are listed (matches <see cref="DashboardPapSelectOptions.Growth"/>).</summary>
    public string? GrowthFilter { get; init; }

    /// <summary>Optional server-side keyword search (clients PAP list toolbar); echoed in pager links.</summary>
    public string? SearchQuery { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
