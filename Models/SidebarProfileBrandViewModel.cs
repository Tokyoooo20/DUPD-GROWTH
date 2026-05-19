namespace DupdGrowth.Web.Models;

public class SidebarProfileBrandViewModel
{
    public required string DisplayName { get; init; }

    public required string OfficeName { get; init; }

    public string? Email { get; init; }

    public required string Initials { get; init; }

    /// <summary>Web root–relative URL (e.g. <c>/uploads/profiles/1.jpg</c>).</summary>
    public string? ProfilePhotoUrl { get; init; }

    /// <summary>False for built-in admin identity (non-numeric user id).</summary>
    public bool CanUploadProfilePhoto { get; init; }

    /// <summary>When true (e.g. Super Admin sidebar), profile trigger does not open a panel or upload modal.</summary>
    public bool SuppressProfilePanel { get; set; }
}
