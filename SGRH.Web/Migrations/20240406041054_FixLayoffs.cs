using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixLayoffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgLast6MonthsSalary",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "DailyAvgLast6Months",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "Last6MonthsSalary",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "Notice",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "NoticeAmount",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "Severance",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "SeveranceAmount",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "TotalSettlement",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "UnenjoyedVacation",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "UnenjoyedVacationAmount",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "DismissalCause",
                table: "Layoffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DismissalDate",
                table: "Layoffs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "HasEmployerResponsibility",
                table: "Layoffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LayoffId = table.Column<int>(type: "int", nullable: false),
                    Last6MonthsSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvgLast6MonthsSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyAvgLast6Months = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnenjoyedVacation = table.Column<int>(type: "int", nullable: false),
                    UnenjoyedVacationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notice = table.Column<int>(type: "int", nullable: false),
                    NoticeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Severance = table.Column<int>(type: "int", nullable: false),
                    SeveranceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSettlement = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Layoffs_LayoffId",
                        column: x => x.LayoffId,
                        principalTable: "Layoffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_LayoffId",
                table: "Settlements",
                column: "LayoffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropColumn(
                name: "DismissalCause",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "DismissalDate",
                table: "Layoffs");

            migrationBuilder.DropColumn(
                name: "HasEmployerResponsibility",
                table: "Layoffs");

            migrationBuilder.AddColumn<decimal>(
                name: "AvgLast6MonthsSalary",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Bonus",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyAvgLast6Months",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Last6MonthsSalary",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Notice",
                table: "Layoffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NoticeAmount",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Severance",
                table: "Layoffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SeveranceAmount",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSettlement",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UnenjoyedVacation",
                table: "Layoffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnenjoyedVacationAmount",
                table: "Layoffs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
