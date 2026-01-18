using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Update_Events_Registration_With_PaymentInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "AppAlumniEventRegistrations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "AppAlumniEventRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TimeslotId",
                table: "AppAlumniEventRegistrations",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "AppAlumniEventRegistrations");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AppAlumniEventRegistrations");

            migrationBuilder.DropColumn(
                name: "TimeslotId",
                table: "AppAlumniEventRegistrations");
        }
    }
}
