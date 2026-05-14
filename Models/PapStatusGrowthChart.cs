using System.Text.Json;

namespace DupdGrowth.Web.Models;

/// <summary>
/// Builds the "Status of PAPs" grouped bar series: per G–H pillar, counts of PAPs with ≥1 quarter in each status;
/// percentages use that pillar’s PAP count (how many R, G, etc.) as the denominator.
/// </summary>
public static class PapStatusGrowthChart
{
    public const int GrowthLetterCount = 6;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>KPI strip counts (projects with ≥1 quarter in that status). Same quarter rules as the chart.</summary>
    public static (int CompletedProjects, int OngoingProjects, int NotStartedProjects) CountProjectsForKpiStrip(
        IReadOnlyList<Project> projects)
    {
        var completed = 0;
        var ongoing = 0;
        var notStarted = 0;
        foreach (var p in projects)
        {
            if (RowHasAnyCompletedQuarter(p)) completed++;
            if (RowHasAnyOngoingQuarter(p)) ongoing++;
            if (RowHasAnyNotStartedQuarter(p)) notStarted++;
        }

        return (completed, ongoing, notStarted);
    }

    /// <summary>
    /// JSON for <c>dashboard-charts.js</c>: <c>totalPaps</c>; <c>pillarPapCounts</c>; <c>completedQuartersPerPillar</c> (count of
    /// quarter fields marked Completed per G–H, for Accomplishment chart); <c>notYetStarted</c>, <c>ongoing</c>, <c>completed</c> (per letter, length 6).
    /// </summary>
    public static string SerializeGroupedCounts(IReadOnlyList<Project> projects)
    {
        var totalPaps = projects.Count;
        var pillarPapCounts = new int[GrowthLetterCount];
        var completedQuartersPerPillar = new int[GrowthLetterCount];
        var notYetStarted = new int[GrowthLetterCount];
        var ongoing = new int[GrowthLetterCount];
        var completed = new int[GrowthLetterCount];

        foreach (var p in projects)
        {
            var idx = MapGrowthToLetterIndex(p.Growth);
            if (idx < 0)
                continue;

            pillarPapCounts[idx]++;
            IncrementIfQuarterCompleted(p.StatusQ1, idx, completedQuartersPerPillar);
            IncrementIfQuarterCompleted(p.StatusQ2, idx, completedQuartersPerPillar);
            IncrementIfQuarterCompleted(p.StatusQ3, idx, completedQuartersPerPillar);
            IncrementIfQuarterCompleted(p.StatusQ4, idx, completedQuartersPerPillar);
            if (RowHasAnyNotStartedQuarter(p))
                notYetStarted[idx]++;
            if (RowHasAnyOngoingQuarter(p))
                ongoing[idx]++;
            if (RowHasAnyCompletedQuarter(p))
                completed[idx]++;
        }

        var payload = new
        {
            totalPaps,
            pillarPapCounts,
            completedQuartersPerPillar,
            notYetStarted,
            ongoing,
            completed
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static void IncrementIfQuarterCompleted(string? status, int letterIndex, int[] completedQuarterCounts)
    {
        if (QuarterIsCompleted(status))
            completedQuarterCounts[letterIndex]++;
    }

    private static bool RowHasAnyCompletedQuarter(Project p) =>
        QuarterIsCompleted(p.StatusQ1) || QuarterIsCompleted(p.StatusQ2)
        || QuarterIsCompleted(p.StatusQ3) || QuarterIsCompleted(p.StatusQ4);

    private static bool RowHasAnyOngoingQuarter(Project p) =>
        QuarterIsOngoing(p.StatusQ1) || QuarterIsOngoing(p.StatusQ2)
        || QuarterIsOngoing(p.StatusQ3) || QuarterIsOngoing(p.StatusQ4);

    private static bool RowHasAnyNotStartedQuarter(Project p) =>
        QuarterIsNotYetStarted(p.StatusQ1) || QuarterIsNotYetStarted(p.StatusQ2)
        || QuarterIsNotYetStarted(p.StatusQ3) || QuarterIsNotYetStarted(p.StatusQ4);

    private static bool QuarterIsCompleted(string? status) => ClassifyQuarter(status) == QuarterKind.Completed;

    private static bool QuarterIsOngoing(string? status) => ClassifyQuarter(status) == QuarterKind.Ongoing;

    private static bool QuarterIsNotYetStarted(string? status) => ClassifyQuarter(status) == QuarterKind.NotYetStarted;

    private enum QuarterKind { Skip, NotYetStarted, Ongoing, Completed }

    private static QuarterKind ClassifyQuarter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return QuarterKind.Skip;

        var s = status.Trim();
        if (string.Equals(s, "N/A", StringComparison.OrdinalIgnoreCase))
            return QuarterKind.Skip;

        if (IsNotYetStarted(s))
            return QuarterKind.NotYetStarted;
        if (IsOngoing(s))
            return QuarterKind.Ongoing;
        if (IsCompleted(s))
            return QuarterKind.Completed;
        return QuarterKind.Skip;
    }

    private static int MapGrowthToLetterIndex(string? growth)
    {
        if (string.IsNullOrWhiteSpace(growth))
            return -1;

        var trimmed = growth.Trim();
        if (string.Equals(trimmed, "N/A", StringComparison.OrdinalIgnoreCase))
            return -1;

        var normalizedInput = NormalizeGrowthText(trimmed);

        for (var i = 0; i < GrowthLetterCount; i++)
        {
            var canon = DashboardPapSelectOptions.Growth[i];
            if (string.Equals(NormalizeGrowthText(canon), normalizedInput, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        for (var i = 0; i < GrowthLetterCount; i++)
        {
            var canon = DashboardPapSelectOptions.Growth[i]!;
            if (trimmed.StartsWith(canon, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        if (TryMapRomanGrowthPrefix(trimmed, out var romanIdx))
            return romanIdx;

        for (var i = 0; i < GrowthLetterCount; i++)
        {
            var canon = DashboardPapSelectOptions.Growth[i]!;
            var nCanon = NormalizeGrowthText(canon);
            if (normalizedInput.Contains(nCanon, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static string NormalizeGrowthText(string s) =>
        string.Join(' ', s.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Map leading I.–VI. (GROWTH pillars) to G–H index; order avoids e.g. <c>I.</c> matching <c>II.</c>.</summary>
    private static bool TryMapRomanGrowthPrefix(string trimmedGrowth, out int index)
    {
        ReadOnlySpan<(string Prefix, int Idx)> prefixes =
        [
            ("VI.", 5),
            ("V.", 4),
            ("IV.", 3),
            ("III.", 2),
            ("II.", 1),
            ("I.", 0),
        ];

        foreach (var (prefix, idx) in prefixes)
        {
            if (trimmedGrowth.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                index = idx;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private static bool IsNotYetStarted(string s) =>
        string.Equals(s, "To be Implemented", StringComparison.OrdinalIgnoreCase)
        || string.Equals(s, "Not Yet Started", StringComparison.OrdinalIgnoreCase)
        || string.Equals(s, "Not yet started", StringComparison.OrdinalIgnoreCase);

    private static bool IsOngoing(string s) =>
        string.Equals(s, "On going", StringComparison.OrdinalIgnoreCase)
        || string.Equals(s, "Ongoing", StringComparison.OrdinalIgnoreCase);

    private static bool IsCompleted(string s) =>
        string.Equals(s, "Completed", StringComparison.OrdinalIgnoreCase);
}
