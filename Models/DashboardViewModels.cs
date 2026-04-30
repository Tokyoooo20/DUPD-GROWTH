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

    public int TotalPapsValue { get; init; } = 1750;
    public string CompletedPercent { get; init; } = "21.1%";
    public string OngoingPercent { get; init; } = "32.0%";
    public string NotStartedPercent { get; init; } = "46.9%";
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

public class DashboardPapRow
{
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

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
