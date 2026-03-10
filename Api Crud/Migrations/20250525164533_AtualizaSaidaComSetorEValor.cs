using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzDex.API.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaSaidaComSetorEValor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TicketSaida",
                table: "SaidaItens",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "SetorDestino",
                table: "SaidaItens",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetorDestino",
                table: "SaidaItens");

            migrationBuilder.AlterColumn<string>(
                name: "TicketSaida",
                table: "SaidaItens",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}

