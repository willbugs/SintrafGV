using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplificaStatusAssociado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssociadoAtivo",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DeAcordo",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Novo",
                table: "Associados");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AssociadoAtivo",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeAcordo",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Novo",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
