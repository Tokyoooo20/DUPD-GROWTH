using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DupdGrowth.Web.Migrations;

/// <summary>Adds approval gate for self-registered users (maps to EntityMapping:UserColumns table, default <c>users</c>).</summary>
[Migration("20260427120000_AddUserIsApproved")]
public class AddUserIsApproved : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_approved",
            table: "users",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_approved",
            table: "users");
    }
}
