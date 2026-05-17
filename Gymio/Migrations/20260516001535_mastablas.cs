using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    /// <inheritdoc />
    public partial class mastablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurnoCajaId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurnoCajaId",
                table: "Egresos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorId",
                table: "ComprasInventario",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Maquinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoInventario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maquinas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ruc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TurnosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoCalculado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoRealFisico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstaAbierto = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MantenimientosMaquinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaquinaId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EgresoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MantenimientosMaquinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MantenimientosMaquinas_Egresos_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egresos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MantenimientosMaquinas_Maquinas_MaquinaId",
                        column: x => x.MaquinaId,
                        principalTable: "Maquinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TurnoCajaId",
                table: "Ventas",
                column: "TurnoCajaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Usuarios_Email",
            //    table: "Usuarios",
            //    column: "Email",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_TurnoCajaId",
                table: "Egresos",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasInventario_ProveedorId",
                table: "ComprasInventario",
                column: "ProveedorId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Clientes_CodigoQR",
            //    table: "Clientes",
            //    column: "CodigoQR",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Clientes_Email",
            //    table: "Clientes",
            //    column: "Email",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientosMaquinas_EgresoId",
                table: "MantenimientosMaquinas",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientosMaquinas_MaquinaId",
                table: "MantenimientosMaquinas",
                column: "MaquinaId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioId",
                table: "TurnosCaja",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasInventario_Proveedores_ProveedorId",
                table: "ComprasInventario",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_TurnosCaja_TurnoCajaId",
                table: "Egresos",
                column: "TurnoCajaId",
                principalTable: "TurnosCaja",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_TurnosCaja_TurnoCajaId",
                table: "Ventas",
                column: "TurnoCajaId",
                principalTable: "TurnosCaja",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasInventario_Proveedores_ProveedorId",
                table: "ComprasInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_TurnosCaja_TurnoCajaId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_TurnosCaja_TurnoCajaId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "MantenimientosMaquinas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "TurnosCaja");

            migrationBuilder.DropTable(
                name: "Maquinas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_TurnoCajaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_TurnoCajaId",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_ComprasInventario_ProveedorId",
                table: "ComprasInventario");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_CodigoQR",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TurnoCajaId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "TurnoCajaId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "ComprasInventario");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
