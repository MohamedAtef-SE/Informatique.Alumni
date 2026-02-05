using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCareerServices_CareerServiceType_ServiceTypeId",
                table: "AppCareerServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerServiceType",
                table: "CareerServiceType");

            migrationBuilder.RenameTable(
                name: "CareerServiceType",
                newName: "AppCareerServiceTypes");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "AppCareerServiceTypes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "AppCareerServiceTypes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppCareerServiceTypes",
                table: "AppCareerServiceTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCareerServices_AppCareerServiceTypes_ServiceTypeId",
                table: "AppCareerServices",
                column: "ServiceTypeId",
                principalTable: "AppCareerServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCareerServices_AppCareerServiceTypes_ServiceTypeId",
                table: "AppCareerServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppCareerServiceTypes",
                table: "AppCareerServiceTypes");

            migrationBuilder.RenameTable(
                name: "AppCareerServiceTypes",
                newName: "CareerServiceType");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "CareerServiceType",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "CareerServiceType",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerServiceType",
                table: "CareerServiceType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCareerServices_CareerServiceType_ServiceTypeId",
                table: "AppCareerServices",
                column: "ServiceTypeId",
                principalTable: "CareerServiceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
