namespace DupdGrowth.Web.Models;

public class Project
{
    public int Id { get; set; }

    public int PriorityNo { get; set; }

    public string Paps { get; set; } = "";

    public string ResponsiblePerson { get; set; } = "";

    public decimal Budget { get; set; }

    public string TimeStart { get; set; } = "";

    public string TimeEnd { get; set; } = "";

    public string? Units { get; set; }

    public string? Growth { get; set; }

    public string? Achieve { get; set; }

    public string RemarksType { get; set; } = "";

    public string? Remarks { get; set; }

    public string? StatusQ1 { get; set; }

    public string? StatusQ2 { get; set; }

    public string? StatusQ3 { get; set; }

    public string? StatusQ4 { get; set; }

    public string? ProjectStatus { get; set; }

    public DateTime? DroppedAt { get; set; }

    public DateTime? DraftAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? OfficeId { get; set; }

    public int? ParentId { get; set; }
}
