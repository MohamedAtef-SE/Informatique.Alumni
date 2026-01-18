using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class Added_ParticipationType_Entity_Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAlumniProfiles_AbpUsers_UserId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppCertificateRequests_AppCertificateDefinitions_CertificateDefinitionId",
                table: "AppCertificateRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEducation_AppAlumniProfiles_AlumniProfileId",
                table: "AppEducation");

            migrationBuilder.DropForeignKey(
                name: "FK_AppEventAgendaItems_AppEvents_EventId",
                table: "AppEventAgendaItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppExperience_AppAlumniProfiles_AlumniProfileId",
                table: "AppExperience");

            migrationBuilder.DropForeignKey(
                name: "FK_AppGalleryImages_AppGalleryAlbums_GalleryAlbumId",
                table: "AppGalleryImages");

            migrationBuilder.DropForeignKey(
                name: "FK_AppPaymentTransactions_AppAssociationRequests_RequestId",
                table: "AppPaymentTransactions");

            migrationBuilder.DropTable(
                name: "AppCareerActivities");

            migrationBuilder.DropIndex(
                name: "IX_AppCertificateRequests_CertificateDefinitionId",
                table: "AppCertificateRequests");

            migrationBuilder.DropIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniProfiles_UserId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniCareerSubscriptions_ActivityId_AlumniId",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppExperience",
                table: "AppExperience");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppEventAgendaItems",
                table: "AppEventAgendaItems");

            migrationBuilder.DropIndex(
                name: "IX_AppEventAgendaItems_EventId",
                table: "AppEventAgendaItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppEducation",
                table: "AppEducation");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AppGuidanceSessionRules");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "CertificateDefinitionId",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "GenerationDate",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "QrCodeContent",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "VerificationHash",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AppCertificateDefinitions");

            migrationBuilder.RenameTable(
                name: "AppExperience",
                newName: "AppExperiences");

            migrationBuilder.RenameTable(
                name: "AppEventAgendaItems",
                newName: "EventAgendaItems");

            migrationBuilder.RenameTable(
                name: "AppEducation",
                newName: "AppEducations");

            migrationBuilder.RenameColumn(
                name: "DayOfWeek",
                table: "AppGuidanceSessionRules",
                newName: "SessionDurationMinutes");

            migrationBuilder.RenameColumn(
                name: "AdvisorId",
                table: "AppGuidanceSessionRules",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_AppGuidanceSessionRules_AdvisorId",
                table: "AppGuidanceSessionRules",
                newName: "IX_AppGuidanceSessionRules_BranchId");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AppEvents",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AppEvents",
                newName: "LastSubscriptionDate");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "AppCompanies",
                newName: "WebsiteUrl");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppCompanies",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "AppAlumniCareerSubscriptions",
                newName: "TimeslotId");

            migrationBuilder.RenameIndex(
                name: "IX_AppExperience_AlumniProfileId",
                table: "AppExperiences",
                newName: "IX_AppExperiences_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppEducation_AlumniProfileId",
                table: "AppEducations",
                newName: "IX_AppEducations_AlumniProfileId");

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "AppSyndicateSubscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GatewayToken",
                table: "AppSyndicateSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidByGateway",
                table: "AppSyndicateSubscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidByWallet",
                table: "AppSyndicateSubscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "AppSyndicateSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "AppSyndicateSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "AppGalleryImages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "AppGalleryAlbums",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AppEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AppEvents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "AppEvents",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapUrl",
                table: "AppEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFees",
                table: "AppEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppEvents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppCompanies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppCompanies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppCompanies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppCompanies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppCompanies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoBlobName",
                table: "AppCompanies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppCompanies",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "AppCommercialDiscounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "UserNotes",
                table: "AppCertificateRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "AppCertificateRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "AppCertificateRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "AppCertificateRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryMethod",
                table: "AppCertificateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidGatewayAmount",
                table: "AppCertificateRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetBranchId",
                table: "AppCertificateRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalItemFees",
                table: "AppCertificateRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UsedWalletAmount",
                table: "AppCertificateRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppCertificateDefinitions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DegreeType",
                table: "AppCertificateDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppCertificateDefinitions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "AppCertificateDefinitions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocuments",
                table: "AppCertificateDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "AppBlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AppBlogPosts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "AppAssociationRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryMethod",
                table: "AppAssociationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PersonalPhotoBlobName",
                table: "AppAssociationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "AppAssociationRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetBranchId",
                table: "AppAssociationRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "UsedWalletAmount",
                table: "AppAssociationRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidityEndDate",
                table: "AppAssociationRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidityStartDate",
                table: "AppAssociationRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerPerson",
                table: "AppAlumniTrips",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "MaxCapacity",
                table: "AppAlumniTrips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "AdminFees",
                table: "AppAlumniTrips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AppAlumniTrips",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CapacityLimit",
                table: "AppAlumniTrips",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFrom",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTo",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmbassyRequirements",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCapacityLimit",
                table: "AppAlumniTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCancellationDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSubscriptionDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeFrom",
                table: "AppAlumniTrips",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "TripProvider",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "AppAlumniTrips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AppAlumniProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "AppAlumniProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "AppAlumniProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WalletBalance",
                table: "AppAlumniProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AppAlumniDirectoryCache",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AppAlumniDirectoryCache",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "AppAlumniCareerSubscriptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CareerServiceId",
                table: "AppAlumniCareerSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsRefunded",
                table: "AppAlumniCareerSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "AppAlumniCareerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "AppAlumniCareerSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "AppAdvisingRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "AppAdvisingRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "AppExperiences",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "AppExperiences",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EventAgendaItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionName",
                table: "AppEducations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Degree",
                table: "AppEducations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<Guid>(
                name: "CollegeId",
                table: "AppEducations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GraduationSemester",
                table: "AppEducations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "MajorId",
                table: "AppEducations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MinorId",
                table: "AppEducations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppExperiences",
                table: "AppExperiences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventAgendaItems",
                table: "EventAgendaItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppEducations",
                table: "AppEducations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppCertificateRequestHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCertificateRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCertificateRequestHistories_AppCertificateRequests_CertificateRequestId",
                        column: x => x.CertificateRequestId,
                        principalTable: "AppCertificateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCertificateRequestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QualificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Language = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VerificationHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenerationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QrCodeContent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCertificateRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCertificateRequestItems_AppCertificateDefinitions_CertificateDefinitionId",
                        column: x => x.CertificateDefinitionId,
                        principalTable: "AppCertificateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCertificateRequestItems_AppCertificateRequests_CertificateRequestId",
                        column: x => x.CertificateRequestId,
                        principalTable: "AppCertificateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCommunicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCommunicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEventTimeslots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEventTimeslots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEventTimeslots_AppEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "AppEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppParticipationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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
                    table.PrimaryKey("PK_AppParticipationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerServiceType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_CareerServiceType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumniProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactEmail_AppAlumniProfiles_AlumniProfileId",
                        column: x => x.AlumniProfileId,
                        principalTable: "AppAlumniProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactMobile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumniProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMobile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactMobile_AppAlumniProfiles_AlumniProfileId",
                        column: x => x.AlumniProfileId,
                        principalTable: "AppAlumniProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactPhone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumniProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPhone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPhone_AppAlumniProfiles_AlumniProfileId",
                        column: x => x.AlumniProfileId,
                        principalTable: "AppAlumniProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GalleryMediaItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GalleryAlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryMediaItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryMediaItem_AppGalleryAlbums_GalleryAlbumId",
                        column: x => x.GalleryAlbumId,
                        principalTable: "AppGalleryAlbums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuidanceSessionRuleWeekDay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    GuidanceSessionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuidanceSessionRuleWeekDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuidanceSessionRuleWeekDay_AppGuidanceSessionRules_GuidanceSessionRuleId",
                        column: x => x.GuidanceSessionRuleId,
                        principalTable: "AppGuidanceSessionRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociationRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestStatusHistory_AppAssociationRequests_AssociationRequestId",
                        column: x => x.AssociationRequestId,
                        principalTable: "AppAssociationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TripPricingTier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanionPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChildPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChildAgeFrom = table.Column<int>(type: "int", nullable: false),
                    ChildAgeTo = table.Column<int>(type: "int", nullable: false),
                    CountsTowardsRoomCapacity = table.Column<bool>(type: "bit", nullable: false),
                    AlumniTripId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripPricingTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripPricingTier_AppAlumniTrips_AlumniTripId",
                        column: x => x.AlumniTripId,
                        principalTable: "AppAlumniTrips",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TripRequirement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetAudience = table.Column<int>(type: "int", nullable: false),
                    SubmissionType = table.Column<int>(type: "int", nullable: false),
                    AlumniTripId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripRequirement_AppAlumniTrips_AlumniTripId",
                        column: x => x.AlumniTripId,
                        principalTable: "AppAlumniTrips",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppEventParticipatingCompanies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEventParticipatingCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEventParticipatingCompanies_AppCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AppCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppEventParticipatingCompanies_AppEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "AppEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppEventParticipatingCompanies_AppParticipationTypes_ParticipationTypeId",
                        column: x => x.ParticipationTypeId,
                        principalTable: "AppParticipationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCareerServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MapUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasFees = table.Column<bool>(type: "bit", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastSubscriptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_AppCareerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCareerServices_CareerServiceType_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "CareerServiceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CareerServiceTimeslot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareerServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LecturerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CareerServiceTimeslot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareerServiceTimeslot_AppCareerServices_CareerServiceId",
                        column: x => x.CareerServiceId,
                        principalTable: "AppCareerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCompanies_NameAr",
                table: "AppCompanies",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCompanies_NameEn",
                table: "AppCompanies",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniProfiles_UserId",
                table: "AppAlumniProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniCareerSubscriptions_CareerServiceId_AlumniId",
                table: "AppAlumniCareerSubscriptions",
                columns: new[] { "CareerServiceId", "AlumniId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCareerServices_ServiceTypeId",
                table: "AppCareerServices",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCertificateRequestHistories_CertificateRequestId",
                table: "AppCertificateRequestHistories",
                column: "CertificateRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCertificateRequestItems_CertificateDefinitionId",
                table: "AppCertificateRequestItems",
                column: "CertificateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCertificateRequestItems_CertificateRequestId",
                table: "AppCertificateRequestItems",
                column: "CertificateRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCommunicationLogs_CreationTime",
                table: "AppCommunicationLogs",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppCommunicationLogs_RecipientId",
                table: "AppCommunicationLogs",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCommunicationLogs_SenderId",
                table: "AppCommunicationLogs",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEventParticipatingCompanies_CompanyId",
                table: "AppEventParticipatingCompanies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEventParticipatingCompanies_EventId",
                table: "AppEventParticipatingCompanies",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEventParticipatingCompanies_ParticipationTypeId",
                table: "AppEventParticipatingCompanies",
                column: "ParticipationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEventTimeslots_EventId",
                table: "AppEventTimeslots",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AppParticipationTypes_NameAr",
                table: "AppParticipationTypes",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppParticipationTypes_NameEn",
                table: "AppParticipationTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareerServiceTimeslot_CareerServiceId",
                table: "CareerServiceTimeslot",
                column: "CareerServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactEmail_AlumniProfileId",
                table: "ContactEmail",
                column: "AlumniProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMobile_AlumniProfileId",
                table: "ContactMobile",
                column: "AlumniProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_AlumniProfileId",
                table: "ContactPhone",
                column: "AlumniProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryMediaItem_GalleryAlbumId",
                table: "GalleryMediaItem",
                column: "GalleryAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_GuidanceSessionRuleWeekDay_GuidanceSessionRuleId",
                table: "GuidanceSessionRuleWeekDay",
                column: "GuidanceSessionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestStatusHistory_AssociationRequestId",
                table: "RequestStatusHistory",
                column: "AssociationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TripPricingTier_AlumniTripId",
                table: "TripPricingTier",
                column: "AlumniTripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripRequirement_AlumniTripId",
                table: "TripRequirement",
                column: "AlumniTripId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppEducations_AppAlumniProfiles_AlumniProfileId",
                table: "AppEducations",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppExperiences_AppAlumniProfiles_AlumniProfileId",
                table: "AppExperiences",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppEducations_AppAlumniProfiles_AlumniProfileId",
                table: "AppEducations");

            migrationBuilder.DropForeignKey(
                name: "FK_AppExperiences_AppAlumniProfiles_AlumniProfileId",
                table: "AppExperiences");

            migrationBuilder.DropTable(
                name: "AppCertificateRequestHistories");

            migrationBuilder.DropTable(
                name: "AppCertificateRequestItems");

            migrationBuilder.DropTable(
                name: "AppCommunicationLogs");

            migrationBuilder.DropTable(
                name: "AppEventParticipatingCompanies");

            migrationBuilder.DropTable(
                name: "AppEventTimeslots");

            migrationBuilder.DropTable(
                name: "CareerServiceTimeslot");

            migrationBuilder.DropTable(
                name: "ContactEmail");

            migrationBuilder.DropTable(
                name: "ContactMobile");

            migrationBuilder.DropTable(
                name: "ContactPhone");

            migrationBuilder.DropTable(
                name: "GalleryMediaItem");

            migrationBuilder.DropTable(
                name: "GuidanceSessionRuleWeekDay");

            migrationBuilder.DropTable(
                name: "RequestStatusHistory");

            migrationBuilder.DropTable(
                name: "TripPricingTier");

            migrationBuilder.DropTable(
                name: "TripRequirement");

            migrationBuilder.DropTable(
                name: "AppParticipationTypes");

            migrationBuilder.DropTable(
                name: "AppCareerServices");

            migrationBuilder.DropTable(
                name: "CareerServiceType");

            migrationBuilder.DropIndex(
                name: "IX_AppCompanies_NameAr",
                table: "AppCompanies");

            migrationBuilder.DropIndex(
                name: "IX_AppCompanies_NameEn",
                table: "AppCompanies");

            migrationBuilder.DropIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniProfiles_UserId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache");

            migrationBuilder.DropIndex(
                name: "IX_AppAlumniCareerSubscriptions_CareerServiceId_AlumniId",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventAgendaItems",
                table: "EventAgendaItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppExperiences",
                table: "AppExperiences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppEducations",
                table: "AppEducations");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "GatewayToken",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaidByGateway",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaidByWallet",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "AppSyndicateSubscriptions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "AppGalleryAlbums");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "GoogleMapUrl",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "HasFees",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppEvents");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "LogoBlobName",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppCompanies");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AppCommercialDiscounts");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "PaidGatewayAmount",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "TargetBranchId",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "TotalItemFees",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "UsedWalletAmount",
                table: "AppCertificateRequests");

            migrationBuilder.DropColumn(
                name: "DegreeType",
                table: "AppCertificateDefinitions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppCertificateDefinitions");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "AppCertificateDefinitions");

            migrationBuilder.DropColumn(
                name: "RequiredDocuments",
                table: "AppCertificateDefinitions");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "PersonalPhotoBlobName",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "TargetBranchId",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "UsedWalletAmount",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "ValidityEndDate",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "ValidityStartDate",
                table: "AppAssociationRequests");

            migrationBuilder.DropColumn(
                name: "AdminFees",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "CapacityLimit",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "DateFrom",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "DateTo",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "EmbassyRequirements",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "HasCapacityLimit",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "LastCancellationDate",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "LastSubscriptionDate",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "TimeFrom",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "TripProvider",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AppAlumniTrips");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "WalletBalance",
                table: "AppAlumniProfiles");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropColumn(
                name: "CareerServiceId",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsRefunded",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "AppAlumniCareerSubscriptions");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "AppAdvisingRequests");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "AppAdvisingRequests");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "AppEducations");

            migrationBuilder.DropColumn(
                name: "GraduationSemester",
                table: "AppEducations");

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "AppEducations");

            migrationBuilder.DropColumn(
                name: "MinorId",
                table: "AppEducations");

            migrationBuilder.RenameTable(
                name: "EventAgendaItems",
                newName: "AppEventAgendaItems");

            migrationBuilder.RenameTable(
                name: "AppExperiences",
                newName: "AppExperience");

            migrationBuilder.RenameTable(
                name: "AppEducations",
                newName: "AppEducation");

            migrationBuilder.RenameColumn(
                name: "SessionDurationMinutes",
                table: "AppGuidanceSessionRules",
                newName: "DayOfWeek");

            migrationBuilder.RenameColumn(
                name: "BranchId",
                table: "AppGuidanceSessionRules",
                newName: "AdvisorId");

            migrationBuilder.RenameIndex(
                name: "IX_AppGuidanceSessionRules_BranchId",
                table: "AppGuidanceSessionRules",
                newName: "IX_AppGuidanceSessionRules_AdvisorId");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "AppEvents",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "LastSubscriptionDate",
                table: "AppEvents",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "WebsiteUrl",
                table: "AppCompanies",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "AppCompanies",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TimeslotId",
                table: "AppAlumniCareerSubscriptions",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_AppExperiences_AlumniProfileId",
                table: "AppExperience",
                newName: "IX_AppExperience_AlumniProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppEducations_AlumniProfileId",
                table: "AppEducation",
                newName: "IX_AppEducation_AlumniProfileId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AppGuidanceSessionRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "AppGalleryImages",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "AppEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "AppCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "AppCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserNotes",
                table: "AppCertificateRequests",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "AppCertificateRequests",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CertificateDefinitionId",
                table: "AppCertificateRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "GenerationDate",
                table: "AppCertificateRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeContent",
                table: "AppCertificateRequests",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationHash",
                table: "AppCertificateRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppCertificateDefinitions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AppCertificateDefinitions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "AppBlogPosts",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AppBlogPosts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerPerson",
                table: "AppAlumniTrips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxCapacity",
                table: "AppAlumniTrips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "AppAlumniTrips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Destination",
                table: "AppAlumniTrips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "AppAlumniProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "AppAlumniProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "AppAlumniProfiles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "AppAlumniProfiles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AppAlumniDirectoryCache",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AppAlumniDirectoryCache",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "AppEventAgendaItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "AppExperience",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "AppExperience",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionName",
                table: "AppEducation",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Degree",
                table: "AppEducation",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppEventAgendaItems",
                table: "AppEventAgendaItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppExperience",
                table: "AppExperience",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppEducation",
                table: "AppEducation",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppCareerActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<byte[]>(type: "rowversion", maxLength: 40, rowVersion: true, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SubscribedCount = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCareerActivities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCertificateRequests_CertificateDefinitionId",
                table: "AppCertificateRequests",
                column: "CertificateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAssociationRequests_IdempotencyKey",
                table: "AppAssociationRequests",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniProfiles_UserId",
                table: "AppAlumniProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniDirectoryCache_UserId",
                table: "AppAlumniDirectoryCache",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAlumniCareerSubscriptions_ActivityId_AlumniId",
                table: "AppAlumniCareerSubscriptions",
                columns: new[] { "ActivityId", "AlumniId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppEventAgendaItems_EventId",
                table: "AppEventAgendaItems",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAlumniProfiles_AbpUsers_UserId",
                table: "AppAlumniProfiles",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCertificateRequests_AppCertificateDefinitions_CertificateDefinitionId",
                table: "AppCertificateRequests",
                column: "CertificateDefinitionId",
                principalTable: "AppCertificateDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEducation_AppAlumniProfiles_AlumniProfileId",
                table: "AppEducation",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppEventAgendaItems_AppEvents_EventId",
                table: "AppEventAgendaItems",
                column: "EventId",
                principalTable: "AppEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppExperience_AppAlumniProfiles_AlumniProfileId",
                table: "AppExperience",
                column: "AlumniProfileId",
                principalTable: "AppAlumniProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppGalleryImages_AppGalleryAlbums_GalleryAlbumId",
                table: "AppGalleryImages",
                column: "GalleryAlbumId",
                principalTable: "AppGalleryAlbums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppPaymentTransactions_AppAssociationRequests_RequestId",
                table: "AppPaymentTransactions",
                column: "RequestId",
                principalTable: "AppAssociationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
