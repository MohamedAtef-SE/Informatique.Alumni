using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class EnsureDirectoryUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache");

            // Deduplicate existing entries for UserId to ensure unique index creation succeeds
            migrationBuilder.Sql("DELETE FROM AppAlumniDirectoryCache WHERE Id NOT IN (SELECT MIN(Id) FROM AppAlumniDirectoryCache GROUP BY UserId)");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache",
                column: "UserId");
        }
    }
}
