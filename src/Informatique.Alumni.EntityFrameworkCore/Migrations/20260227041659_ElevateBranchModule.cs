using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class ElevateBranchModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppBranches",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "AppBranches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "AppBranches",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AppBranches",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookPage",
                table: "AppBranches",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AppBranches",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInPage",
                table: "AppBranches",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AppBranches",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AppBranches",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PresidentId",
                table: "AppBranches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppGroup",
                table: "AppBranches",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "FacebookPage",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "LinkedInPage",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "PresidentId",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "WhatsAppGroup",
                table: "AppBranches");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppBranches",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "AppBranches",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "AppBranches",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);
        }
    }
}
