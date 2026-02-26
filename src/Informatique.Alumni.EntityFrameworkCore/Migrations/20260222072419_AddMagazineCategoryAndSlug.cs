using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class AddMagazineCategoryAndSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppBlogPosts_Category",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AppBlogPosts");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "AppBlogPosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "AppBlogPosts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AppArticleCategories",
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
                    table.PrimaryKey("PK_AppArticleCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppBlogPosts_CategoryId",
                table: "AppBlogPosts",
                column: "CategoryId");

            migrationBuilder.Sql("UPDATE AppBlogPosts SET Slug = CAST(Id AS NVARCHAR(36)) WHERE Slug = '' OR Slug IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_AppBlogPosts_Slug",
                table: "AppBlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppBlogPosts_AppArticleCategories_CategoryId",
                table: "AppBlogPosts",
                column: "CategoryId",
                principalTable: "AppArticleCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppBlogPosts_AppArticleCategories_CategoryId",
                table: "AppBlogPosts");

            migrationBuilder.DropTable(
                name: "AppArticleCategories");

            migrationBuilder.DropIndex(
                name: "IX_AppBlogPosts_CategoryId",
                table: "AppBlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_AppBlogPosts_Slug",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "AppBlogPosts");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AppBlogPosts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBlogPosts_Category",
                table: "AppBlogPosts",
                column: "Category");
        }
    }
}
