using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Added_Health_Details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineNumber",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "AppMedicalPartners",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "HotlineNumber",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "AppMedicalPartners");
        }
    }
}
