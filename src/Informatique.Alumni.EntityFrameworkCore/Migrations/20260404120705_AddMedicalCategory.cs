using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MedicalCategoryId",
                table: "AppMedicalPartners",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppMedicalCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BaseType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AppMedicalCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMedicalPartners_MedicalCategoryId",
                table: "AppMedicalPartners",
                column: "MedicalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMedicalCategories_BaseType",
                table: "AppMedicalCategories",
                column: "BaseType");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMedicalPartners_AppMedicalCategories_MedicalCategoryId",
                table: "AppMedicalPartners",
                column: "MedicalCategoryId",
                principalTable: "AppMedicalCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMedicalPartners_AppMedicalCategories_MedicalCategoryId",
                table: "AppMedicalPartners");

            migrationBuilder.DropTable(
                name: "AppMedicalCategories");

            migrationBuilder.DropIndex(
                name: "IX_AppMedicalPartners_MedicalCategoryId",
                table: "AppMedicalPartners");

            migrationBuilder.DropColumn(
                name: "MedicalCategoryId",
                table: "AppMedicalPartners");
        }
    }
}
