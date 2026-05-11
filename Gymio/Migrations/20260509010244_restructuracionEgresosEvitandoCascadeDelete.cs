using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class restructuracionEgresosEvitandoCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Productos_ProductoId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Usuarios_UsuarioId",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_ProductoId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "CantidadComprada",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Egresos");

            migrationBuilder.AddColumn<string>(
                name: "FrecuenciaPago",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalarioBase",
                table: "Usuarios",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComprasInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    CantidadComprada = table.Column<int>(type: "int", nullable: false),
                    EgresoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasInventario_Egresos_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprasInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosPlanilla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntrenadorId = table.Column<int>(type: "int", nullable: false),
                    PeriodoInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodoFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EgresoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosPlanilla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosPlanilla_Egresos_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosPlanilla_Usuarios_EntrenadorId",
                        column: x => x.EntrenadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprasInventario_EgresoId",
                table: "ComprasInventario",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasInventario_ProductoId",
                table: "ComprasInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosPlanilla_EgresoId",
                table: "PagosPlanilla",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosPlanilla_EntrenadorId",
                table: "PagosPlanilla",
                column: "EntrenadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Usuarios_UsuarioId",
                table: "Egresos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Usuarios_UsuarioId",
                table: "Egresos");

            migrationBuilder.DropTable(
                name: "ComprasInventario");

            migrationBuilder.DropTable(
                name: "PagosPlanilla");

            migrationBuilder.DropColumn(
                name: "FrecuenciaPago",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SalarioBase",
                table: "Usuarios");

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
                name: "FK_Egresos_Usuarios_UsuarioId",
                table: "Egresos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
