using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class vacations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Warnings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_UserId",
                table: "Warnings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warnings_AspNetUsers_UserId",
                table: "Warnings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warnings_AspNetUsers_UserId",
                table: "Warnings");

            migrationBuilder.DropIndex(
                name: "IX_Warnings_UserId",
                table: "Warnings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Warnings");
        }
    }
}
