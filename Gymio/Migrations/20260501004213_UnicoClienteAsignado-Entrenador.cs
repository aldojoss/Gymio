using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class UnicoClienteAsignadoEntrenador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesEntrenadores_ClienteId",
                table: "AsignacionesEntrenadores");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesEntrenadores_ClienteId",
                table: "AsignacionesEntrenadores",
                column: "ClienteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesEntrenadores_ClienteId",
                table: "AsignacionesEntrenadores");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesEntrenadores_ClienteId",
                table: "AsignacionesEntrenadores",
                column: "ClienteId");
        }
    }
}
