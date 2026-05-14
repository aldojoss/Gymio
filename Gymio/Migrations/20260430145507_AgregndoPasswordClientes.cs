using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class AgregndoPasswordClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Clientes");
        }
    }
}
