using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSistemaVotacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Eleicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ArquivoAnexo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InicioVotacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FimVotacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApenasAssociados = table.Column<bool>(type: "bit", nullable: false),
                    ApenasAtivos = table.Column<bool>(type: "bit", nullable: false),
                    BancoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eleicoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perguntas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EleicaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    MaxVotos = table.Column<int>(type: "int", nullable: true),
                    PermiteBranco = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perguntas_Eleicoes_EleicaoId",
                        column: x => x.EleicaoId,
                        principalTable: "Eleicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EleicaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataHoraVoto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpOrigem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodigoComprovante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votos_Associados_AssociadoId",
                        column: x => x.AssociadoId,
                        principalTable: "Associados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votos_Eleicoes_EleicaoId",
                        column: x => x.EleicaoId,
                        principalTable: "Eleicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Opcoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerguntaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Foto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssociadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opcoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opcoes_Associados_AssociadoId",
                        column: x => x.AssociadoId,
                        principalTable: "Associados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Opcoes_Perguntas_PerguntaId",
                        column: x => x.PerguntaId,
                        principalTable: "Perguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotosDetalhes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerguntaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpcaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VotoBranco = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotosDetalhes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotosDetalhes_Opcoes_OpcaoId",
                        column: x => x.OpcaoId,
                        principalTable: "Opcoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotosDetalhes_Perguntas_PerguntaId",
                        column: x => x.PerguntaId,
                        principalTable: "Perguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Opcoes_AssociadoId",
                table: "Opcoes",
                column: "AssociadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Opcoes_PerguntaId",
                table: "Opcoes",
                column: "PerguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Perguntas_EleicaoId",
                table: "Perguntas",
                column: "EleicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_AssociadoId",
                table: "Votos",
                column: "AssociadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_EleicaoId_AssociadoId",
                table: "Votos",
                columns: new[] { "EleicaoId", "AssociadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotosDetalhes_OpcaoId",
                table: "VotosDetalhes",
                column: "OpcaoId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosDetalhes_PerguntaId",
                table: "VotosDetalhes",
                column: "PerguntaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropTable(
                name: "VotosDetalhes");

            migrationBuilder.DropTable(
                name: "Opcoes");

            migrationBuilder.DropTable(
                name: "Perguntas");

            migrationBuilder.DropTable(
                name: "Eleicoes");
        }
    }
}
