using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyExpenses.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBase64DataToExpenseAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Base64Data",
                table: "ExpenseAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Base64Data",
                table: "ExpenseAttachments");
        }
    }
}
