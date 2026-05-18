using System.Globalization;
using System.Security.Claims;
using DupdGrowth.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DupdGrowth.Web.Models;

/// <summary>Builds <see cref="SidebarProfileBrandViewModel"/> for <c>Views/Client/Profile.cshtml</c> (sidebar account + inline photo upload).</summary>
public static class UserProfileSnapshot
{
    public static async Task<SidebarProfileBrandViewModel> CreateAsync(
        ClaimsPrincipal principal,
        ApplicationDbContext db,
        string? sidebarUserNameFallback,
        CancellationToken cancellationToken = default)
    {
        var displayName = principal.Identity?.Name
            ?? sidebarUserNameFallback
            ?? "User";
        displayName = string.IsNullOrWhiteSpace(displayName) ? "User" : displayName.Trim();

        var officeName = principal.FindFirst("OfficeName")?.Value ?? "Sample Office";
        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;

        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? photoUrl = null;
        var canUpload = false;

        if (int.TryParse(idClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
        {
            canUpload = true;
            photoUrl = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.ProfilePhotoPath)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(photoUrl))
            photoUrl = null;

        return new SidebarProfileBrandViewModel
        {
            DisplayName = displayName,
            OfficeName = officeName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            Initials = ComputeInitials(displayName),
            ProfilePhotoUrl = photoUrl,
            CanUploadProfilePhoto = canUpload
        };
    }

    private static string ComputeInitials(string displayName)
    {
        var parts = displayName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0)
            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpperInvariant();
        if (parts.Length == 1 && parts[0].Length == 1)
            return parts[0].Substring(0, 1).ToUpperInvariant();
        return "U";
    }
}
