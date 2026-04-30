namespace DupdGrowth.Web.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? ParentOfficeId { get; set; }

    public Office? ParentOffice { get; set; }

    public int? OfficeId { get; set; }

    public Office? Office { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    /// <summary>When false, the user may not sign in until a Super Admin approves the account.</summary>
    public bool IsApproved { get; set; }
}
