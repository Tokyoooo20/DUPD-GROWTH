using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DupdGrowth.Web.Controllers.SuperAdmin;

[Route("Pages")]
public class OfficeManagementController : Controller
{
    private readonly ApplicationDbContext _db;

    public OfficeManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("OfficeManagement")]
    public async Task<IActionResult> OfficeManagement()
    {
        var rows = await _db.Offices.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new { id = o.Id, name = o.Name, parent_id = o.ParentId })
            .ToListAsync();
        ViewData["OfficesJson"] = JsonSerializer.Serialize(rows);
        return View("~/Views/SuperAdmin/OfficeM.cshtml");
    }

    /// <summary>Top-level offices only (<c>parent_id</c> IS NULL).</summary>
    [HttpGet("OfficeManagement/TopLevel")]
    public async Task<IActionResult> TopLevelOffices()
    {
        var rows = await _db.Offices.AsNoTracking()
            .Where(o => o.ParentId == null)
            .OrderBy(o => o.Name)
            .Select(o => new { id = o.Id, name = o.Name, parent_id = o.ParentId })
            .ToListAsync();
        return Json(rows);
    }

    /// <summary>Direct child offices of the given parent <c>office_id</c>.</summary>
    [HttpGet("OfficeManagement/Children/{parentId:int}")]
    public async Task<IActionResult> ChildOffices(int parentId)
    {
        var parentExists = await _db.Offices.AnyAsync(o => o.Id == parentId);
        if (!parentExists)
            return NotFound(new { success = false, message = "Parent office not found." });

        var rows = await _db.Offices.AsNoTracking()
            .Where(o => o.ParentId == parentId)
            .OrderBy(o => o.Name)
            .Select(o => new { id = o.Id, name = o.Name, parent_id = o.ParentId })
            .ToListAsync();
        return Json(rows);
    }

    /// <summary>Create a sub-office under an existing parent (<c>parent_id</c> required, must exist).</summary>
    [HttpPost("OfficeManagement/CreateSubOffice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubOffice([FromBody] CreateSubOfficeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequestFromModelState();

        var name = request.OfficeName.Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { success = false, errors = new[] { "Office name is required." } });

        var parentId = request.ParentOfficeId;
        var parentExists = await _db.Offices.AnyAsync(o => o.Id == parentId);
        if (!parentExists)
            return BadRequest(new { success = false, errors = new[] { "The selected parent office does not exist." } });

        var office = new Office { Name = name, ParentId = parentId };
        _db.Offices.Add(office);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Sub-office added successfully.",
            office = new { id = office.Id, name = office.Name, parent_id = office.ParentId }
        });
    }

    /// <summary>Update <c>office_name</c> only (hierarchy preserved).</summary>
    [HttpPost("OfficeManagement/UpdateOffice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOffice([FromBody] UpdateOfficeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequestFromModelState();

        var name = request.OfficeName.Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { success = false, errors = new[] { "Office name is required." } });

        var entity = await _db.Offices.FirstOrDefaultAsync(o => o.Id == request.Id);
        if (entity is null)
            return NotFound(new { success = false, errors = new[] { "Office not found." } });

        entity.Name = name;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Office updated successfully.",
            office = new { id = entity.Id, name = entity.Name, parent_id = entity.ParentId }
        });
    }

    /// <summary>Deletes the office and all descendants (children first, FK-safe order).</summary>
    [HttpPost("OfficeManagement/DeleteOffice")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOffice([FromBody] DeleteOfficeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequestFromModelState();

        var rootExists = await _db.Offices.AnyAsync(o => o.Id == request.Id);
        if (!rootExists)
            return NotFound(new { success = false, errors = new[] { "Office not found." } });

        var snapshot = await _db.Offices.AsNoTracking()
            .Select(o => new { o.Id, o.ParentId })
            .ToListAsync();

        var flat = snapshot.Select(s => (s.Id, s.ParentId)).ToList();
        var deleteIds = PostOrderSubtreeIds(flat, request.Id);

        foreach (var officeId in deleteIds)
        {
            var entity = await _db.Offices.FirstOrDefaultAsync(o => o.Id == officeId);
            if (entity is not null)
                _db.Offices.Remove(entity);
        }

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Office removed." });
    }

    private IActionResult BadRequestFromModelState()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message ?? "Invalid value." : e.ErrorMessage)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        return BadRequest(new { success = false, errors });
    }

    private static List<int> PostOrderSubtreeIds(List<(int Id, int? ParentId)> rows, int rootId)
    {
        if (!rows.Any(r => r.Id == rootId))
            return new List<int>();

        var childrenByParent = rows
            .Where(r => r.ParentId != null)
            .GroupBy(r => r.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var order = new List<int>();
        void Visit(int id)
        {
            if (childrenByParent.TryGetValue(id, out var kids))
            {
                foreach (var c in kids)
                    Visit(c);
            }
            order.Add(id);
        }

        Visit(rootId);
        return order;
    }
}
