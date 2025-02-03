﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRH.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVacationsDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VacationDays",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VacationDays",
                table: "AspNetUsers");
        }
    }
}
