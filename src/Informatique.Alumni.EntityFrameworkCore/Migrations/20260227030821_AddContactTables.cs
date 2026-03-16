using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddContactTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactEmail_AppAlumniProfiles_AlumniProfileId",
                table: "ContactEmail");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactMobile_AppAlumniProfiles_AlumniProfileId",
                table: "ContactMobile");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPhone_AppAlumniProfiles_AlumniProfileId",
                table: "ContactPhone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactPhone",
                table: "ContactPhone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactMobile",
                table: "ContactMobile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactEmail",
                table: "ContactEmail");

            migrationBuilder.RenameTable(
                name: "ContactPhone",
                newName: "AppContactPhones");

            migrationBuilder.RenameTable(
                name: "ContactMobile",
                newName: "AppContactMobiles");

            migrationBuilder.RenameTable(
                name: "ContactEmail",
                newName: "AppContactEmails");

            migrationBuilder.RenameIndex(
                name: "IX_ContactPhone_AlumniProfileId",
                table: "AppContactPhones",
                newName: "IX_AppContactPhones_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactMobile_AlumniProfileId",
                table: "AppContactMobiles",
                newName: "IX_AppContactMobiles_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactEmail_AlumniProfileId",
                table: "AppContactEmails",
                newName: "IX_AppContactEmails_AlumniProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AppContactPhones",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "AppContactMobiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AppContactEmails",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppContactPhones",
                table: "AppContactPhones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppContactMobiles",
                table: "AppContactMobiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppContactEmails",
                table: "AppContactEmails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppContactEmails_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactEmails",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppContactMobiles_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactMobiles",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppContactPhones_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactPhones",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppContactEmails_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_AppContactMobiles_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactMobiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppContactPhones_AppAlumniProfiles_AlumniProfileId",
                table: "AppContactPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppContactPhones",
                table: "AppContactPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppContactMobiles",
                table: "AppContactMobiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppContactEmails",
                table: "AppContactEmails");

            migrationBuilder.RenameTable(
                name: "AppContactPhones",
                newName: "ContactPhone");

            migrationBuilder.RenameTable(
                name: "AppContactMobiles",
                newName: "ContactMobile");

            migrationBuilder.RenameTable(
                name: "AppContactEmails",
                newName: "ContactEmail");

            migrationBuilder.RenameIndex(
                name: "IX_AppContactPhones_AlumniProfileId",
                table: "ContactPhone",
                newName: "IX_ContactPhone_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppContactMobiles_AlumniProfileId",
                table: "ContactMobile",
                newName: "IX_ContactMobile_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppContactEmails_AlumniProfileId",
                table: "ContactEmail",
                newName: "IX_ContactEmail_AlumniProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ContactPhone",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "ContactMobile",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactEmail",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactPhone",
                table: "ContactPhone",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactMobile",
                table: "ContactMobile",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactEmail",
                table: "ContactEmail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactEmail_AppAlumniProfiles_AlumniProfileId",
                table: "ContactEmail",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMobile_AppAlumniProfiles_AlumniProfileId",
                table: "ContactMobile",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactPhone_AppAlumniProfiles_AlumniProfileId",
                table: "ContactPhone",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
