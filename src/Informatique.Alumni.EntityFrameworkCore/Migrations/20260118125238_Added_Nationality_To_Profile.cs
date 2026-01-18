using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Added_Nationality_To_Profile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NationalityId",
                table: "AppAlumniProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppNationalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNationalities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniProfiles_NationalityId",
                table: "AppAlumniProfiles",
                column: "NationalityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAlumniProfiles_AppNationalities_NationalityId",
                table: "AppAlumniProfiles",
                column: "NationalityId",
                principalTable: "AppNationalities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAlumniProfiles_AppNationalities_NationalityId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropTable(
                name: "AppNationalities");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniProfiles_NationalityId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "AppAlumniProfiles");
        }
    }
}
