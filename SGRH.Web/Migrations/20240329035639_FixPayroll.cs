using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banco_Popular",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Base_Salary",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "End_Date",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Enfermedad_Maternidad",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Payroll_Period",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "Start_Date",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "Total_Income",
                table: "Payrolls",
                newName: "TotalDeductions");

            migrationBuilder.RenameColumn(
                name: "Total_Deductions",
                table: "Payrolls",
                newName: "OtHoursAmount");

            migrationBuilder.RenameColumn(
                name: "Regular_Hours_Amount",
                table: "Payrolls",
                newName: "OtHours");

            migrationBuilder.RenameColumn(
                name: "Regular_Hours",
                table: "Payrolls",
                newName: "NetSalary");

            migrationBuilder.RenameColumn(
                name: "Overtime_Hours_Amount",
                table: "Payrolls",
                newName: "GrossSalary");

            migrationBuilder.RenameColumn(
                name: "Overtime_Hours",
                table: "Payrolls",
                newName: "EnfermedadMaternidad");

            migrationBuilder.RenameColumn(
                name: "Net_Amount",
                table: "Payrolls",
                newName: "BancoPopular");

            migrationBuilder.AlterColumn<int>(
                name: "PayrollFrequency",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayrollPeriodId",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PayrollPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriod", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_PayrollPeriodId",
                table: "Payrolls",
                column: "PayrollPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_PayrollPeriod_PayrollPeriodId",
                table: "Payrolls",
                column: "PayrollPeriodId",
                principalTable: "PayrollPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_PayrollPeriod_PayrollPeriodId",
                table: "Payrolls");

            migrationBuilder.DropTable(
                name: "PayrollPeriod");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_PayrollPeriodId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "PayrollPeriodId",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "TotalDeductions",
                table: "Payrolls",
                newName: "Total_Income");

            migrationBuilder.RenameColumn(
                name: "OtHoursAmount",
                table: "Payrolls",
                newName: "Total_Deductions");

            migrationBuilder.RenameColumn(
                name: "OtHours",
                table: "Payrolls",
                newName: "Regular_Hours_Amount");

            migrationBuilder.RenameColumn(
                name: "NetSalary",
                table: "Payrolls",
                newName: "Regular_Hours");

            migrationBuilder.RenameColumn(
                name: "GrossSalary",
                table: "Payrolls",
                newName: "Overtime_Hours_Amount");

            migrationBuilder.RenameColumn(
                name: "EnfermedadMaternidad",
                table: "Payrolls",
                newName: "Overtime_Hours");

            migrationBuilder.RenameColumn(
                name: "BancoPopular",
                table: "Payrolls",
                newName: "Net_Amount");

            migrationBuilder.AlterColumn<string>(
                name: "PayrollFrequency",
                table: "Payrolls",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Banco_Popular",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Base_Salary",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "End_Date",
                table: "Payrolls",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Enfermedad_Maternidad",
                table: "Payrolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "Payroll_Period",
                table: "Payrolls",
                type: "datetime2",
                maxLength: 30,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Start_Date",
                table: "Payrolls",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
