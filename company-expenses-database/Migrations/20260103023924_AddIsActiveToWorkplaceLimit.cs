using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyExpenses.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToWorkplaceLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkplaceLimits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkplaceLimits",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkplaceLimits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkplaceLimits");
        }
    }
}
