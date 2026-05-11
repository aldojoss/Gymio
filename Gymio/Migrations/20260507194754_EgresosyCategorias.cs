using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class EgresosyCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaEgresoId",
                table: "Egresos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CategoriasEgresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasEgresos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_CategoriaEgresoId",
                table: "Egresos",
                column: "CategoriaEgresoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_CategoriasEgresos_CategoriaEgresoId",
                table: "Egresos",
                column: "CategoriaEgresoId",
                principalTable: "CategoriasEgresos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_CategoriasEgresos_CategoriaEgresoId",
                table: "Egresos");

            migrationBuilder.DropTable(
                name: "CategoriasEgresos");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_CategoriaEgresoId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "CategoriaEgresoId",
                table: "Egresos");
        }
    }
}
