using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Informatique.Alumni.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "AppBlogPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "AppBlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "AppBlogPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "AppBlogPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "AppBlogPosts");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "AppBlogPosts");
        }
    }
}
