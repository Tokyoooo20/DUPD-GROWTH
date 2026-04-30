using System.ComponentModel.DataAnnotations;

namespace DupdGrowth.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
