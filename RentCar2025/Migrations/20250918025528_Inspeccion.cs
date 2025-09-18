using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCar2025.Migrations
{
    /// <inheritdoc />
    public partial class Inspeccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspeccionId",
                table: "Rentas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rentas_InspeccionId",
                table: "Rentas",
                column: "InspeccionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentas_Inspecciones_InspeccionId",
                table: "Rentas",
                column: "InspeccionId",
                principalTable: "Inspecciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentas_Inspecciones_InspeccionId",
                table: "Rentas");

            migrationBuilder.DropIndex(
                name: "IX_Rentas_InspeccionId",
                table: "Rentas");

            migrationBuilder.DropColumn(
                name: "InspeccionId",
                table: "Rentas");
        }
    }
}
