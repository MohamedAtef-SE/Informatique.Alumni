using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminLifecycleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdCardStatus",
                table: "AppAlumniProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<bool>(
                name: "IsNotable",
                table: "AppAlumniProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AppAlumniProfiles",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AppAlumniProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniProfiles_Status",
                table: "AppAlumniProfiles",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAlumniProfiles_Status",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "IdCardStatus",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "IsNotable",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppAlumniProfiles");
        }
    }
}
