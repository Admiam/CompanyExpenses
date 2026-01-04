using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyExpenses.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToWorkplaceLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "WorkplaceLimits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkplaceLimits_CategoryId",
                table: "WorkplaceLimits",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkplaceLimits_ExpenseCategories_CategoryId",
                table: "WorkplaceLimits",
                column: "CategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkplaceLimits_ExpenseCategories_CategoryId",
                table: "WorkplaceLimits");

            migrationBuilder.DropIndex(
                name: "IX_WorkplaceLimits_CategoryId",
                table: "WorkplaceLimits");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "WorkplaceLimits");
        }
    }
}
