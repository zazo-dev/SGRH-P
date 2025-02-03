using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasProcessed",
                table: "Settlements");

            migrationBuilder.AddColumn<bool>(
                name: "HasProcessed",
                table: "Layoffs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasProcessed",
                table: "Layoffs");

            migrationBuilder.AddColumn<bool>(
                name: "HasProcessed",
                table: "Settlements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
