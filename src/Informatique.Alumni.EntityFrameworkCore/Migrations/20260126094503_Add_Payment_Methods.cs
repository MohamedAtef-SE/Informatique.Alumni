using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Add_Payment_Methods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAssociationRequests_AppSubscriptionFees_SubscriptionFeeId",
                table: "AppAssociationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerServiceTimeslot_AppCareerServices_CareerServiceId",
                table: "CareerServiceTimeslot");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestStatusHistory_AppAssociationRequests_AssociationRequestId",
                table: "RequestStatusHistory");

            migrationBuilder.DropIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestStatusHistory",
                table: "RequestStatusHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerServiceTimeslot",
                table: "CareerServiceTimeslot");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "AppAcademicGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppAcademicGrants");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                table: "AppAcademicGrants");

            migrationBuilder.RenameTable(
                name: "RequestStatusHistory",
                newName: "AppRequestStatusHistory");

            migrationBuilder.RenameTable(
                name: "CareerServiceTimeslot",
                newName: "CareerServiceTimeslots");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AppAcademicGrants",
                newName: "NameEn");

            migrationBuilder.RenameIndex(
                name: "IX_RequestStatusHistory_AssociationRequestId",
                table: "AppRequestStatusHistory",
                newName: "IX_AppRequestStatusHistory_AssociationRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_CareerServiceTimeslot_CareerServiceId",
                table: "CareerServiceTimeslots",
                newName: "IX_CareerServiceTimeslots_CareerServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppSubscriptionFees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppPaymentTransactions",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppPaymentTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "AppBranches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRefunded",
                table: "AppAlumniEventRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppActivityTypes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppActivityTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppActivityTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppActivityTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppActivityTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppActivityTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppActivityTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppActivityTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppActivityTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppActivityTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppAcademicGrants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "AppAcademicGrants",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "AppAcademicGrants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "AppRequestStatusHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "CareerServiceTimeslots",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "CareerServiceTimeslots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppRequestStatusHistory",
                table: "AppRequestStatusHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerServiceTimeslots",
                table: "CareerServiceTimeslots",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppColleges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_AppColleges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppColleges_AppBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "AppBranches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppMajors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CollegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AppMajors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMembershipFeeConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OneYearFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TwoYearsFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoreThanTwoYearsFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMembershipFeeConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CollegeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_AppColleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "AppColleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specialization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Specialization_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAssociationRequests_AlumniId",
                table: "AppAssociationRequests",
                column: "AlumniId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAssociationRequests_Status",
                table: "AppAssociationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppColleges_BranchId",
                table: "AppColleges",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMajors_CollegeId",
                table: "AppMajors",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_CollegeId",
                table: "Department",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialization_DepartmentId",
                table: "Specialization",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppRequestStatusHistory_AppAssociationRequests_AssociationRequestId",
                table: "AppRequestStatusHistory",
                column: "AssociationRequestId",
                principalTable: "AppAssociationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerServiceTimeslots_AppCareerServices_CareerServiceId",
                table: "CareerServiceTimeslots",
                column: "CareerServiceId",
                principalTable: "AppCareerServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppRequestStatusHistory_AppAssociationRequests_AssociationRequestId",
                table: "AppRequestStatusHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_CareerServiceTimeslots_AppCareerServices_CareerServiceId",
                table: "CareerServiceTimeslots");

            migrationBuilder.DropTable(
                name: "AppMajors");

            migrationBuilder.DropTable(
                name: "AppMembershipFeeConfigs");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Specialization");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "AppColleges");

            migrationBuilder.DropIndex(
                name: "IX_AppAssociationRequests_AlumniId",
                table: "AppAssociationRequests");

            migrationBuilder.DropIndex(
                name: "IX_AppAssociationRequests_Status",
                table: "AppAssociationRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CareerServiceTimeslots",
                table: "CareerServiceTimeslots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppRequestStatusHistory",
                table: "AppRequestStatusHistory");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppPaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppPaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "AppBranches");

            migrationBuilder.DropColumn(
                name: "IsRefunded",
                table: "AppAlumniEventRegistrations");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppActivityTypes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppAcademicGrants");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "AppAcademicGrants");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AppAcademicGrants");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "CareerServiceTimeslots");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "CareerServiceTimeslots");

            migrationBuilder.RenameTable(
                name: "CareerServiceTimeslots",
                newName: "CareerServiceTimeslot");

            migrationBuilder.RenameTable(
                name: "AppRequestStatusHistory",
                newName: "RequestStatusHistory");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "AppAcademicGrants",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_CareerServiceTimeslots_CareerServiceId",
                table: "CareerServiceTimeslot",
                newName: "IX_CareerServiceTimeslot_CareerServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_AppRequestStatusHistory_AssociationRequestId",
                table: "RequestStatusHistory",
                newName: "IX_RequestStatusHistory_AssociationRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppSubscriptionFees",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "AppAcademicGrants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppAcademicGrants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                table: "AppAcademicGrants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "RequestStatusHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CareerServiceTimeslot",
                table: "CareerServiceTimeslot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestStatusHistory",
                table: "RequestStatusHistory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests",
                column: "IdempotencyKey");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAssociationRequests_AppSubscriptionFees_SubscriptionFeeId",
                table: "AppAssociationRequests",
                column: "SubscriptionFeeId",
                principalTable: "AppSubscriptionFees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CareerServiceTimeslot_AppCareerServices_CareerServiceId",
                table: "CareerServiceTimeslot",
                column: "CareerServiceId",
                principalTable: "AppCareerServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestStatusHistory_AppAssociationRequests_AssociationRequestId",
                table: "RequestStatusHistory",
                column: "AssociationRequestId",
                principalTable: "AppAssociationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
