using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountOT",
                table: "Overtimes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TypeOT",
                table: "Overtimes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BaseSalary",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "WorkPeriodId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxPeriodValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPeriod", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkPeriodId",
                table: "AspNetUsers",
                column: "WorkPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_WorkPeriod_WorkPeriodId",
                table: "AspNetUsers",
                column: "WorkPeriodId",
                principalTable: "WorkPeriod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_WorkPeriod_WorkPeriodId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "WorkPeriod");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkPeriodId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AmountOT",
                table: "Overtimes");

            migrationBuilder.DropColumn(
                name: "TypeOT",
                table: "Overtimes");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkPeriodId",
                table: "AspNetUsers");
        }
    }
}
