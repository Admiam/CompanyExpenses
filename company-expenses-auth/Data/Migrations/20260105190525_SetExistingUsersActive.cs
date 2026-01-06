using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace company_expenses_auth.Migrations
{
    /// <inheritdoc />
    public partial class SetExistingUsersActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set all existing users to active
            migrationBuilder.Sql("UPDATE AspNetUsers SET IsActive = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed
        }
    }
}
