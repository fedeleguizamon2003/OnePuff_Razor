using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePuff_Razor.Migrations
{
    /// <inheritdoc />
    public partial class AddMonedero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Monederos",
                columns: table => new
                {
                    MonederoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monederos", x => x.MonederoId);
                    table.ForeignKey(
                        name: "FK_Monederos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonederoMovimientos",
                columns: table => new
                {
                    MonederoMovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonederoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonederoMovimientos", x => x.MonederoMovimientoId);
                    table.ForeignKey(
                        name: "FK_MonederoMovimientos_Monederos_MonederoId",
                        column: x => x.MonederoId,
                        principalTable: "Monederos",
                        principalColumn: "MonederoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonederoMovimientos_MonederoId_Fecha",
                table: "MonederoMovimientos",
                columns: new[] { "MonederoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Monederos_ClienteId",
                table: "Monederos",
                column: "ClienteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonederoMovimientos");

            migrationBuilder.DropTable(
                name: "Monederos");
        }
    }
}
