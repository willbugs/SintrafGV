using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssociadoHistoricoEncerrado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Encerrado",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SubstituidoPorId",
                table: "Associados",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Encerrado",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "SubstituidoPorId",
                table: "Associados");
        }
    }
}
