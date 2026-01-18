using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Added_ActivityType_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActivityTypeId",
                table: "AppEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppActivityTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppActivityTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEvents_ActivityTypeId",
                table: "AppEvents",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppActivityTypes_NameAr",
                table: "AppActivityTypes",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppActivityTypes_NameEn",
                table: "AppActivityTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEvents_AppActivityTypes_ActivityTypeId",
                table: "AppEvents",
                column: "ActivityTypeId",
                principalTable: "AppActivityTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppEvents_AppActivityTypes_ActivityTypeId",
                table: "AppEvents");

            migrationBuilder.DropTable(
                name: "AppActivityTypes");

            migrationBuilder.DropIndex(
                name: "IX_AppEvents_ActivityTypeId",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "ActivityTypeId",
                table: "AppEvents");
        }
    }
}
