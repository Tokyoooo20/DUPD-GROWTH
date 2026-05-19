using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DupdGrowth.Web.Data;

/// <summary>
/// Aligns <c>projects</c> with the app model: drops legacy quarter photo columns and ensures a single
/// <c>completion_photo</c> column (renames typo <c>completation_photo</c> when present).
/// </summary>
public static class ProjectCompletionPhotoSchema
{
    private static readonly string[] LegacyQuarterPhotoColumns =
    [
        "quarter4_photo_path",
        "quarter3_photo_path",
        "quarter2_photo_path",
        "quarter1_photo_path",
    ];

    public static async Task EnsureAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var column in LegacyQuarterPhotoColumns)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE `projects` DROP COLUMN `" + column + "`;",
                    cancellationToken);
            }
            catch (MySqlException ex) when (ex.Number == 1091)
            {
                // ER_BAD_FIELD_ERROR — column does not exist.
            }
        }

        // Single canonical column: completion_photo
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE `projects`
                  CHANGE COLUMN `completation_photo` `completion_photo` VARCHAR(512) NULL DEFAULT NULL;
                """,
                cancellationToken);
            return;
        }
        catch (MySqlException ex) when (ex.Number == 1054)
        {
            // Unknown column completation_photo — add completion_photo if missing (below).
        }
        catch (MySqlException ex) when (ex.Number == 1060)
        {
            // completion_photo already exists; drop duplicate typo column if still present.
            try
            {
                await db.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE `projects` DROP COLUMN `completation_photo`;",
                    cancellationToken);
            }
            catch (MySqlException ex2) when (ex2.Number == 1091)
            {
            }

            return;
        }

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE `projects`
                  ADD COLUMN `completion_photo` VARCHAR(512) NULL DEFAULT NULL AFTER `4th_quarter`;
                """,
                cancellationToken);
        }
        catch (MySqlException ex) when (ex.Number == 1060)
        {
            // ER_DUP_FIELDNAME — already present.
        }
    }
}
