namespace DupdGrowth.Web.Models;

public class Pap
{
    public int Id { get; set; }

    public int PriorityNo { get; set; }

    public string PapName { get; set; } = "";

    public string ResponsiblePerson { get; set; } = "";

    public decimal Budget { get; set; }

    public int TimeFrameStart { get; set; }

    public int TimeFrameEnd { get; set; }

    public string SupportOffice { get; set; } = "";

    public string? AlignmentGrowth { get; set; }

    public string? AlignmentAchieve { get; set; }

    public string RemarksType { get; set; } = "";

    public string? RemarksTypeOther { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = "Endorsed";

    public int CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
