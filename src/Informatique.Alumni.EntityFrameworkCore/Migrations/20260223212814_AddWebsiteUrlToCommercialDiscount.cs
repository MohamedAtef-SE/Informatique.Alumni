using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteUrlToCommercialDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "AppCommercialDiscounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCareerServices_BranchId",
                table: "AppCareerServices",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCareerServices_AppBranches_BranchId",
                table: "AppCareerServices",
                column: "BranchId",
                principalTable: "AppBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCareerServices_AppBranches_BranchId",
                table: "AppCareerServices");

            migrationBuilder.DropIndex(
                name: "IX_AppCareerServices_BranchId",
                table: "AppCareerServices");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "AppCommercialDiscounts");
        }
    }
}
