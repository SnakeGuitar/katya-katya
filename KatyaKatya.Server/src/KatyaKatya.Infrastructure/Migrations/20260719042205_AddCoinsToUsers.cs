using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KatyaKatya.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "coins",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coins",
                table: "users");
        }
    }
}
