using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Add_EventAgendaItems_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Speaker",
                table: "EventAgendaItems");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "EventAgendaItems",
                newName: "ActivityName");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "EventAgendaItems",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "EventAgendaItems",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "AssociationEventId",
                table: "EventAgendaItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "EventAgendaItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EventAgendaItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Place",
                table: "EventAgendaItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAgendaItems_AssociationEventId",
                table: "EventAgendaItems",
                column: "AssociationEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAgendaItems_AppEvents_AssociationEventId",
                table: "EventAgendaItems",
                column: "AssociationEventId",
                principalTable: "AppEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAgendaItems_AppEvents_AssociationEventId",
                table: "EventAgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_EventAgendaItems_AssociationEventId",
                table: "EventAgendaItems");

            migrationBuilder.DropColumn(
                name: "AssociationEventId",
                table: "EventAgendaItems");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "EventAgendaItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EventAgendaItems");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "EventAgendaItems");

            migrationBuilder.RenameColumn(
                name: "ActivityName",
                table: "EventAgendaItems",
                newName: "Title");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "EventAgendaItems",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "EventAgendaItems",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<string>(
                name: "Speaker",
                table: "EventAgendaItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
