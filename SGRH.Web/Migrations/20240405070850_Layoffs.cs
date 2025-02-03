using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class Layoffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenefitsLiquidations");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Layoffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalActionId_Action = table.Column<int>(type: "int", nullable: true),
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
                    TotalSettlement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegisteredBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layoffs_PersonalActions_PersonalActionId_Action",
                        column: x => x.PersonalActionId_Action,
                        principalTable: "PersonalActions",
                        principalColumn: "Id_Action");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Layoffs_PersonalActionId_Action",
                table: "Layoffs",
                column: "PersonalActionId_Action");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Layoffs");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "BenefitsLiquidations",
                columns: table => new
                {
                    Id_Benefit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalActionId_Action = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Benefit_Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Net_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitsLiquidations", x => x.Id_Benefit);
                    table.ForeignKey(
                        name: "FK_BenefitsLiquidations_PersonalActions_PersonalActionId_Action",
                        column: x => x.PersonalActionId_Action,
                        principalTable: "PersonalActions",
                        principalColumn: "Id_Action");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitsLiquidations_PersonalActionId_Action",
                table: "BenefitsLiquidations",
                column: "PersonalActionId_Action");
        }
    }
}
