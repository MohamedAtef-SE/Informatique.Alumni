using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedinUrl",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedinUrl",
                table: "AppAlumniProfiles");
        }
    }
}
