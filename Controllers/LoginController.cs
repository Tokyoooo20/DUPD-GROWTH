using System.Globalization;
using System.Security.Claims;
using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Controllers;

[Route("Pages")]
public class LoginController : Controller
{
    private const string AdminEmailNormalized = "admin@gmail.com";
    private const string AdminPassword = "Admin123";

    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View("~/Views/Login.cshtml", new LoginViewModel());
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login.cshtml", model);

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (normalizedEmail == AdminEmailNormalized && model.Password == AdminPassword)
        {
            var adminClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "admin"),
                new(ClaimTypes.Email, AdminEmailNormalized),
                new(ClaimTypes.Name, "Admin"),
                new(ClaimTypes.Role, "Admin"),
                new("OfficeName", "System Admin")
            };
            var adminIdentity = new ClaimsIdentity(
                adminClaims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(adminIdentity),
                new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
            return RedirectToAction("Dashboard", "Dashboard");
        }

        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Office)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user is null
            || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password)
                == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("~/Views/Login.cshtml", model);
        }

        if (!user.IsApproved)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is pending approval. You can log in after a Super Admin approves it.");
            return View("~/Views/Login.cshtml", model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new("OfficeName", user.Office?.Name ?? "Sample Office")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });

        return RedirectToAction("UserDashboard", "Client");
    }
}
