using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracaoSindicato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssinaturaDigital",
                table: "Votos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChaveCriptografia",
                table: "Votos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DadosDispositivo",
                table: "Votos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidacaoCartorio",
                table: "Votos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashAnterior",
                table: "Votos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashVoto",
                table: "Votos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Votos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Votos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroProtocoloCartorio",
                table: "Votos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroSequencial",
                table: "Votos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesValidacao",
                table: "Votos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespostaCriptografada",
                table: "Votos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimestampPreciso",
                table: "Votos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ValidadoCartorio",
                table: "Votos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConfiguracoesSindicato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    InscricaoEstadual = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UF = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CEP = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Presidente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CPFPresidente = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Secretario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CPFSecretario = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    TextoAutenticacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CartorioResponsavel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnderecoCartorio = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesSindicato", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_DataHoraVoto",
                table: "Votos",
                column: "DataHoraVoto");

            migrationBuilder.CreateIndex(
                name: "IX_Votos_HashVoto",
                table: "Votos",
                column: "HashVoto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesSindicato_CNPJ",
                table: "ConfiguracoesSindicato",
                column: "CNPJ",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesSindicato");

            migrationBuilder.DropIndex(
                name: "IX_Votos_DataHoraVoto",
                table: "Votos");

            migrationBuilder.DropIndex(
                name: "IX_Votos_HashVoto",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "AssinaturaDigital",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ChaveCriptografia",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "DadosDispositivo",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "DataValidacaoCartorio",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "HashAnterior",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "HashVoto",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "NumeroProtocoloCartorio",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "NumeroSequencial",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ObservacoesValidacao",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "RespostaCriptografada",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "TimestampPreciso",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ValidadoCartorio",
                table: "Votos");
        }
    }
}
