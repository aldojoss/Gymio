using Gymio.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gymio.Migrations
{
    [DbContext(typeof(GymioDbContext))]
    [Migration("20260517222000_FotoPerfilUsuarios")]
    public partial class FotoPerfilUsuarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Usuarios', 'FotoUrl') IS NULL
                BEGIN
                    ALTER TABLE [Usuarios] ADD [FotoUrl] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Clientes', 'FotoUrl') IS NULL
                BEGIN
                    ALTER TABLE [Clientes] ADD [FotoUrl] nvarchar(max) NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Usuarios', 'FotoUrl') IS NOT NULL
                BEGIN
                    ALTER TABLE [Usuarios] DROP COLUMN [FotoUrl];
                END
                """);
        }
    }
}
