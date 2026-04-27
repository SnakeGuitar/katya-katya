using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoryGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToPendingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "pending_registrations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "username",
                table: "pending_registrations");
        }
    }
}
