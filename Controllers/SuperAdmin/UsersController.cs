using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("Users")]
    public async Task<IActionResult> Users(int page = 1, string? search = null, string? office = null)
    {
        var vm = await BuildUserListViewModelAsync(page, search, office);

        if (string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.Ordinal))
        {
            return PartialView("_UserListResults", vm);
        }

        return View("~/Views/SuperAdmin/User.cshtml", vm);
    }

    private async Task<PagedUserListViewModel> BuildUserListViewModelAsync(int page, string? search, string? office)
    {
        const int pageSize = 10;
        search = search?.Trim() ?? "";
        if (string.IsNullOrEmpty(office))
        {
            office = "all";
        }

        var officeOptions = await _db.Offices.AsNoTracking()
            .Where(o => o.ParentId == null)
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToListAsync();

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (search.Length > 0)
        {
            query = query.Where(u =>
                u.Name.Contains(search) ||
                u.Email.Contains(search));
        }

        if (!string.Equals(office, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u =>
                u.ParentOffice != null &&
                u.ParentOffice.Name == office);
        }

        query = query
            .OrderBy(u => u.IsApproved)
            .ThenBy(u => u.Name);

        var total = await query.CountAsync();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        if (page < 1)
        {
            page = 1;
        }
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserRow
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Office = u.Office != null ? u.Office.Name : (u.ParentOffice != null ? u.ParentOffice.Name : ""),
                IsApproved = u.IsApproved
            })
            .ToListAsync();

        return new PagedUserListViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Search = search,
            Office = office,
            OfficeOptions = officeOptions
        };
    }

    [HttpPost("ApproveUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveUser(int id, int page = 1, string? search = null, string? office = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is not null)
        {
            user.IsApproved = true;
            await _db.SaveChangesAsync();
        }

        if (string.IsNullOrEmpty(office))
        {
            office = "all";
        }

        return RedirectToAction(nameof(Users), new { page, search, office });
    }

    [HttpPost("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id, int page = 1, string? search = null, string? office = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is not null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        if (string.IsNullOrEmpty(office))
        {
            office = "all";
        }

        return RedirectToAction(nameof(Users), new { page, search, office });
    }
}
