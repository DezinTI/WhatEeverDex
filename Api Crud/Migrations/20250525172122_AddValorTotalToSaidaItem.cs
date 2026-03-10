using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzDex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddValorTotalToSaidaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TicketSaida",
                table: "SaidaItens",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "SaidaItens",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "SaidaItens");

            migrationBuilder.AlterColumn<string>(
                name: "TicketSaida",
                table: "SaidaItens",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}

