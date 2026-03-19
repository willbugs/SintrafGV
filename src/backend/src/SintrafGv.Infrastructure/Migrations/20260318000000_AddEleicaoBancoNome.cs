using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEleicaoBancoNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BancoNome",
                table: "Eleicoes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BancoNome",
                table: "Eleicoes");
        }
    }
}
