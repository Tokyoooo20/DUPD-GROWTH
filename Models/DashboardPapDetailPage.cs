namespace DupdGrowth.Web.Models;

/// <summary>Sample PAP data and <see cref="DashboardDetailViewModel"/> builder shared by admin dashboard detail, admin reports, and client user project page.</summary>
public static class DashboardPapDetailPage
{
    public static readonly int[] YearOptions = [2026, 2025, 2024, 2023];

    private const int PapListPageSize = 10;

    private static readonly IReadOnlyList<DashboardPapRow> SamplePaps =
    [
        Pap("Institutional research plan", "Dr. A. Santos", "₱850,000", "Jan 2026", "Dec 2026", "VPAA", "I. Global Recognition", "Harmonized SDG-based HE R&I", "Continuing", "Completed", "Completed", "To be Implemented", "To be Implemented", "On track", "Completed"),
        Pap("Campus sustainability review", "Prof. B. Cruz", "₱420,000", "Mar 2026", "Feb 2027", "VPRIE, OSU", "II. Research Excellence", "Effective & Efficient Public Service", "New", "To be Implemented", "On going", "To be Implemented", "To be Implemented", "Pending data", "Ongoing"),
        Pap("Faculty development program", "Dr. C. Reyes", "₱1,200,000", "Jun 2026", "May 2028", "Chancellor", "III. Optimized Accreditation", "Integrated Real-Time Data& Analytics", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Not yet started", "Not started"),
        Pap("Student services assessment", "Ms. D. Lim", "₱310,000", "Feb 2026", "Nov 2026", "VPGASO", "IV. World-Class Digital Transformation", "Centralized One-Nation Human Capital Dev.Plan", "Continuing", "Completed", "Completed", "Completed", "To be Implemented", "Closed", "Completed"),
        Pap("Infrastructure modernization", "Engr. E. Ramos", "₱4,500,000", "Jan 2025", "Dec 2027", "VPFA, PPO", "V. Thriving Curriculum", "Vitalized Policies, Internal System & Governance", "Continuing", "To be Implemented", "To be Implemented", "On going", "To be Implemented", "Delayed permit", "Ongoing"),
        Pap("Digital learning roadmap", "Dr. F. Tan", "₱600,000", "Aug 2026", "Jul 2027", "VPAA, ICT", "VI. Holistic Development", "Integrated Real-Time Data", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Awaiting approval", "Not started"),
        Pap("Community extension mapping", "Prof. G. Bautista", "₱250,000", "Apr 2026", "Mar 2027", "Chancellor", "N/A", "Advanced & Accessible Lifelong Learning", "Continuing", "To be Implemented", "On going", "To be Implemented", "To be Implemented", "Field work", "Ongoing"),
        Pap("Quality assurance cycle", "Dr. H. Gomez", "₱180,000", "Jan 2026", "Jun 2026", "VPAA", "I. Global Recognition", "Effective Public Service", "Continuing", "Completed", "Completed", "Completed", "Completed", "Complete", "Completed"),
        Pap("Research ethics guidelines", "Dr. I. Navarro", "₱95,000", "Sep 2026", "Aug 2027", "VPRIE", "II. Research Excellence", "Expanded Internationalization", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Draft", "Not started"),
        Pap("Annual report draft", "Ms. P. Salazar", "₱40,000", "Feb 2026", "Apr 2026", "Chancellor", "III. Optimized Accreditation", "Vitalized Policies & Governance", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Draft", "Not started"),
        Pap("Budget alignment PAP", "Mr. J. Ortiz", "₱2,100,000", "May 2026", "Apr 2028", "VPFA", "III. Optimized Accreditation", "Vitalized Policies & Governance", "Continuing", "To be Implemented", "To be Implemented", "On going", "To be Implemented", "Monitoring", "Ongoing"),
        Pap("Library digitization initiative", "Ms. K. Vergara", "₱340,000", "Jul 2026", "Jun 2027", "VPAA", "IV. World-Class Digital Transformation", "Expanded & Impact-Driven Internationalization", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Scoping", "Not started"),
        Pap("Alumni engagement network", "Dr. L. Mendoza", "₱220,000", "Jan 2026", "Dec 2026", "Chancellor", "V. Thriving Curriculum", "Harmonized SDG-based HE R&I", "Continuing", "On going", "To be Implemented", "To be Implemented", "To be Implemented", "Pilot", "Ongoing"),
        Pap("Campus security upgrade", "Mr. M. Dela Cruz", "₱1,800,000", "Mar 2026", "Feb 2028", "VPFA", "VI. Holistic Development", "Effective & Efficient Public Service", "New", "To be Implemented", "To be Implemented", "To be Implemented", "To be Implemented", "Bid stage", "Not started"),
        Pap("International partnerships MOU", "Dr. N. Aquino", "₱150,000", "Aug 2026", "Jul 2027", "VPRIE", "N/A", "Integrated Real-Time Data& Analytics", "Continuing", "To be Implemented", "Completed", "To be Implemented", "To be Implemented", "Signed", "Completed"),
        Pap("Wellness program rollout", "Ms. O. Ramos", "₱480,000", "Apr 2026", "Mar 2027", "VPGASO", "I. Global Recognition", "Advanced & Accessible Lifelong Learning", "New", "To be Implemented", "On going", "To be Implemented", "To be Implemented", "Training", "Ongoing")
    ];

    private static readonly DashboardKpiStripViewModel DefaultKpi = new();

    public static DashboardDetailViewModel? CreateViewModel(string segment, int page, int? year, string? growth)
    {
        if (!DashboardSegments.IsValid(segment))
            return null;

        var key = segment.ToLowerInvariant();
        var growthFilter = ResolveGrowthFilter(growth);
        var allRows = ApplyGrowthFilter(FilterRows(key), growthFilter);
        var totalCount = allRows.Count;

        if (page < 1)
            page = 1;
        var totalPages = PapListPageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)PapListPageSize);
        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var rows = allRows.Skip((page - 1) * PapListPageSize).Take(PapListPageSize).ToList();
        var subtitle = TableSubtitleFor(key);

        var kpi = new DashboardKpiStripViewModel
        {
            ActiveSegment = key,
            TotalPapsValue = DefaultKpi.TotalPapsValue,
            CompletedPercent = DefaultKpi.CompletedPercent,
            OngoingPercent = DefaultKpi.OngoingPercent,
            NotStartedPercent = DefaultKpi.NotStartedPercent
        };

        var selectedYear = ResolveDashboardYear(year);

        return new DashboardDetailViewModel
        {
            Kpi = kpi,
            Rows = rows,
            TableSubtitle = subtitle,
            SelectedYear = selectedYear,
            GrowthFilter = growthFilter,
            Page = page,
            PageSize = PapListPageSize,
            TotalCount = totalCount
        };
    }

    private static readonly IReadOnlyList<DashboardPapRow> DraftSampleRows =
        SamplePaps.Where(r => string.Equals(r.Remarks, "Draft", StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>Rows shown on the client Draft page (remarks marked Draft).</summary>
    public static IReadOnlyList<DashboardPapRow> GetDraftSampleRows() => DraftSampleRows;

    /// <summary>Sample rows for the client Dropped page (until a backend exists).</summary>
    private static readonly IReadOnlyList<DashboardPapRow> DroppedSampleRows =
    [
        SamplePaps[1],
        SamplePaps[4],
        SamplePaps[11]
    ];

    public static IReadOnlyList<DashboardPapRow> GetDroppedSampleRows() => DroppedSampleRows;

    /// <summary>Paginated list for client pages that are not KPI segments (e.g. Draft).</summary>
    public static DashboardDetailViewModel CreateFilteredListViewModel(
        IReadOnlyList<DashboardPapRow> sourceRows,
        int page,
        int? year,
        string? growth)
    {
        var growthFilter = ResolveGrowthFilter(growth);
        var allRows = ApplyGrowthFilter(sourceRows.ToList(), growthFilter);
        var totalCount = allRows.Count;

        if (page < 1)
            page = 1;
        var totalPages = PapListPageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)PapListPageSize);
        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var rows = allRows.Skip((page - 1) * PapListPageSize).Take(PapListPageSize).ToList();

        var kpi = new DashboardKpiStripViewModel
        {
            ActiveSegment = null,
            TotalPapsValue = DefaultKpi.TotalPapsValue,
            CompletedPercent = DefaultKpi.CompletedPercent,
            OngoingPercent = DefaultKpi.OngoingPercent,
            NotStartedPercent = DefaultKpi.NotStartedPercent
        };

        var selectedYear = ResolveDashboardYear(year);

        return new DashboardDetailViewModel
        {
            Kpi = kpi,
            Rows = rows,
            TableSubtitle = "",
            SelectedYear = selectedYear,
            GrowthFilter = growthFilter,
            Page = page,
            PageSize = PapListPageSize,
            TotalCount = totalCount
        };
    }

    private static int ResolveDashboardYear(int? year)
    {
        if (year is null)
            return 2026;
        return YearOptions.Contains(year.Value) ? year.Value : 2026;
    }

    private static string? ResolveGrowthFilter(string? growth)
    {
        if (string.IsNullOrWhiteSpace(growth))
            return null;
        var t = growth.Trim();
        return DashboardPapSelectOptions.Growth.FirstOrDefault(g => string.Equals(g, t, StringComparison.OrdinalIgnoreCase));
    }

    private static List<DashboardPapRow> ApplyGrowthFilter(IReadOnlyList<DashboardPapRow> rows, string? growthFilter)
    {
        if (growthFilter is null)
            return rows.ToList();
        return rows.Where(r => string.Equals(r.AlignmentGrowth, growthFilter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static DashboardPapRow Pap(
        string name,
        string responsible,
        string budget,
        string start,
        string end,
        string support,
        string growth,
        string achieve,
        string remarksType,
        string q1,
        string q2,
        string q3,
        string q4,
        string remarks,
        string status) =>
        new()
        {
            Name = name,
            ResponsiblePerson = responsible,
            BudgetAllocation = budget,
            TimeFrameStart = start,
            TimeFrameEnd = end,
            SupportOfficesUnits = support,
            AlignmentGrowth = growth,
            AlignmentAchieve = achieve,
            RemarksContinuingNew = remarksType,
            StatusQ1 = q1,
            StatusQ2 = q2,
            StatusQ3 = q3,
            StatusQ4 = q4,
            Remarks = remarks,
            Status = status
        };

    private static IReadOnlyList<DashboardPapRow> FilterRows(string segmentKey) =>
        segmentKey switch
        {
            DashboardSegments.Total => SamplePaps,
            DashboardSegments.Completed => SamplePaps.Where(r => string.Equals(r.Status, "Completed", StringComparison.OrdinalIgnoreCase)).ToList(),
            DashboardSegments.Ongoing => SamplePaps.Where(r => string.Equals(r.Status, "Ongoing", StringComparison.OrdinalIgnoreCase)).ToList(),
            DashboardSegments.NotStarted => SamplePaps.Where(r => string.Equals(r.Status, "Not started", StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => SamplePaps
        };

    private static string TableSubtitleFor(string segmentKey) =>
        segmentKey switch
        {
            DashboardSegments.Total => "All PAPs",
            DashboardSegments.Completed => "Completed PAPs",
            DashboardSegments.Ongoing => "Ongoing PAPs",
            DashboardSegments.NotStarted => "Not started PAPs",
            _ => "PAPs"
        };
}
