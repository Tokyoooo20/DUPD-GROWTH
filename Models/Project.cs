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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? OfficeId { get; set; }

    public int? ParentId { get; set; }
}
