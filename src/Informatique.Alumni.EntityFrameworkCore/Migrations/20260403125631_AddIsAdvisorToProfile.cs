using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdvisorToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppEventParticipatingCompanies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppEventParticipatingCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppEventParticipatingCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppEventParticipatingCompanies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppEventParticipatingCompanies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppEventParticipatingCompanies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppEventParticipatingCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdvisor",
                table: "AppAlumniProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppEventParticipatingCompanies");

            migrationBuilder.DropColumn(
                name: "IsAdvisor",
                table: "AppAlumniProfiles");
        }
    }
}
