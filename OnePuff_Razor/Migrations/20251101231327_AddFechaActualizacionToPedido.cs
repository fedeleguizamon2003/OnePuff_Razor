using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePuff_Razor.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaActualizacionToPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Pedidos");
        }
    }
}
