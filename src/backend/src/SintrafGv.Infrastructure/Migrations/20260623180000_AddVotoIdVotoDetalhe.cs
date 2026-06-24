using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    public partial class AddVotoIdVotoDetalhe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VotoId",
                table: "VotosDetalhes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotosDetalhes_VotoId",
                table: "VotosDetalhes",
                column: "VotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_VotosDetalhes_Votos_VotoId",
                table: "VotosDetalhes",
                column: "VotoId",
                principalTable: "Votos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Vincula detalhes existentes ao voto pela proximidade de data/hora (até 30s)
            migrationBuilder.Sql("""
                UPDATE vd
                SET vd.VotoId = v.Id
                FROM VotosDetalhes vd
                INNER JOIN Perguntas p ON p.Id = vd.PerguntaId
                INNER JOIN Votos v ON v.EleicaoId = p.EleicaoId
                    AND ABS(DATEDIFF(SECOND, vd.DataHora, v.DataHoraVoto)) <= 30
                WHERE vd.VotoId IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM VotosDetalhes vd2
                      WHERE vd2.VotoId = v.Id AND vd2.Id <> vd.Id
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VotosDetalhes_Votos_VotoId",
                table: "VotosDetalhes");

            migrationBuilder.DropIndex(
                name: "IX_VotosDetalhes_VotoId",
                table: "VotosDetalhes");

            migrationBuilder.DropColumn(
                name: "VotoId",
                table: "VotosDetalhes");
        }
    }
}
