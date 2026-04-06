using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisoryCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdvisoryBio",
                table: "AppAlumniProfiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdvisoryExperienceYears",
                table: "AppAlumniProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AdvisoryRejectionReason",
                table: "AppAlumniProfiles",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdvisoryStatus",
                table: "AppAlumniProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.CreateTable(
                name: "AppAdvisoryCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_AppAdvisoryCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAlumniAdvisorExpertises",
                columns: table => new
                {
                    AlumniProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvisoryCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAlumniAdvisorExpertises", x => new { x.AlumniProfileId, x.AdvisoryCategoryId });
                    table.ForeignKey(
                        name: "FK_AppAlumniAdvisorExpertises_AppAdvisoryCategories_AdvisoryCategoryId",
                        column: x => x.AdvisoryCategoryId,
                        principalTable: "AppAdvisoryCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppAlumniAdvisorExpertises_AppAlumniProfiles_AlumniProfileId",
                        column: x => x.AlumniProfileId,
                        principalTable: "AppAlumniProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAdvisoryCategories_NameEn",
                table: "AppAdvisoryCategories",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniAdvisorExpertises_AdvisoryCategoryId",
                table: "AppAlumniAdvisorExpertises",
                column: "AdvisoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniAdvisorExpertises_AlumniProfileId",
                table: "AppAlumniAdvisorExpertises",
                column: "AlumniProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAlumniAdvisorExpertises");

            migrationBuilder.DropTable(
                name: "AppAdvisoryCategories");

            migrationBuilder.DropColumn(
                name: "AdvisoryBio",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "AdvisoryExperienceYears",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "AdvisoryRejectionReason",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "AdvisoryStatus",
                table: "AppAlumniProfiles");
        }
    }
}
