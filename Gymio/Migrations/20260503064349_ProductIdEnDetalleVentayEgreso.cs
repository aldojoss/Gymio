using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class ProductIdEnDetalleVentayEgreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "VentaDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadComprada",
                table: "Egresos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Egresos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_ProductoId",
                table: "VentaDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_ProductoId",
                table: "Egresos",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Productos_ProductoId",
                table: "Egresos",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaDetalles_Productos_ProductoId",
                table: "VentaDetalles",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Productos_ProductoId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_VentaDetalles_Productos_ProductoId",
                table: "VentaDetalles");

            migrationBuilder.DropIndex(
                name: "IX_VentaDetalles_ProductoId",
                table: "VentaDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_ProductoId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "VentaDetalles");

            migrationBuilder.DropColumn(
                name: "CantidadComprada",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Egresos");
        }
    }
}
