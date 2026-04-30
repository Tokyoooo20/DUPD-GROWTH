using System.Text.Json;
using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Controllers;

[Route("Pages")]
public class SignupController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public SignupController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("Signup")]
    public async Task<IActionResult> Signup()
    {
        var vm = new SignupViewModel();
        await PopulateSignupOfficeFieldsAsync(vm);
        return View("~/Views/Signup.cshtml", vm);
    }

    [HttpPost("Signup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        await PopulateSignupOfficeFieldsAsync(model);

        if (model.SignupOfficeId is int officeId
            && !await _db.Offices.AnyAsync(o => o.Id == officeId && o.ParentId == null))
        {
            ModelState.AddModelError(nameof(model.SignupOfficeId), "Select a valid office.");
        }

        if (model.SignupSubOfficeId is int subId)
        {
            if (model.SignupOfficeId is not int parentId
                || !await _db.Offices.AnyAsync(o => o.Id == subId && o.ParentId == parentId))
            {
                ModelState.AddModelError(nameof(model.SignupSubOfficeId), "Select a valid sub office.");
            }
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        if (ModelState.IsValid
            && await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        }

        if (!ModelState.IsValid)
            return View("~/Views/Signup.cshtml", model);

        var resolvedOfficeId = model.SignupSubOfficeId ?? model.SignupOfficeId!.Value;

        var name = model.Name.Trim();
        if (name.Length > 200)
            name = name[..200];

        var user = new User
        {
            Email = normalizedEmail.Length <= 150 ? normalizedEmail : normalizedEmail[..150],
            Name = name,
            ParentOfficeId = model.SignupOfficeId!.Value,
            OfficeId = resolvedOfficeId,
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["SignupSuccess"] = "Account created. A Super Admin must approve your account before you can log in.";
        return RedirectToAction("Login", "Login");
    }

    private async Task PopulateSignupOfficeFieldsAsync(SignupViewModel vm)
    {
        vm.OfficeOptions = await _db.Offices.AsNoTracking()
            .Where(o => o.ParentId == null)
            .OrderBy(o => o.Name)
            .ToListAsync();

        var subRows = await _db.Offices.AsNoTracking()
            .Where(o => o.ParentId != null)
            .OrderBy(o => o.Name)
            .Select(o => new
            {
                officeId = o.Id,
                name = o.Name,
                parentOfficeId = o.ParentId!.Value
            })
            .ToListAsync();

        vm.SubOfficesJson = JsonSerializer.Serialize(subRows);
    }
}
