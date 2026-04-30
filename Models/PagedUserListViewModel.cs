namespace DupdGrowth.Web.Models;

public class UserRow
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Office { get; init; } = "";
    public string Email { get; init; } = "";
    public bool IsApproved { get; init; }
}

public class PagedUserListViewModel
{
    public IReadOnlyList<UserRow> Items { get; init; } = Array.Empty<UserRow>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public string Search { get; init; } = "";
    /// <summary>Selected office filter, or "all" for every office.</summary>
    public string Office { get; init; } = "all";
    public IReadOnlyList<string> OfficeOptions { get; init; } = Array.Empty<string>();

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
