namespace DupdGrowth.Web.Models;

public class Office
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}
