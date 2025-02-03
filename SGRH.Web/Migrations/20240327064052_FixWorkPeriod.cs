using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDailyOTHours",
                table: "WorkPeriod",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDailyOTHours",
                table: "WorkPeriod");
        }
    }
}
