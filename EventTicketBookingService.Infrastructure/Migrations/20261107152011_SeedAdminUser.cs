using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventTicketBookingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "login", "password_hash", "role" },
                values: new object[] { 1, "admin", "240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
