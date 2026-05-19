using System.ComponentModel.DataAnnotations;

namespace DupdGrowth.Web.Models;

public class CreateSubOfficeRequest
{
    [Required(ErrorMessage = "Office name is required.")]
    [StringLength(200, MinimumLength = 1)]
    public string OfficeName { get; set; } = "";

    /// <summary>Existing <c>office_id</c> that will be the parent (must exist).</summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid parent office must be selected.")]
    public int ParentOfficeId { get; set; }
}

public class UpdateOfficeRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Office name is required.")]
    [StringLength(200, MinimumLength = 1)]
    public string OfficeName { get; set; } = "";
}

public class DeleteOfficeRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}
