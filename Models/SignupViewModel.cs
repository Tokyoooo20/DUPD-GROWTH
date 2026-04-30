using System.ComponentModel.DataAnnotations;

namespace DupdGrowth.Web.Models;

public class SignupViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an office.")]
    [Display(Name = "Office")]
    public int? SignupOfficeId { get; set; }

    [Display(Name = "Sub office")]
    public int? SignupSubOfficeId { get; set; }

    public string SubOfficesJson { get; set; } = "[]";

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string Confirm { get; set; } = string.Empty;

    public IReadOnlyList<Office> OfficeOptions { get; set; } = Array.Empty<Office>();
}
