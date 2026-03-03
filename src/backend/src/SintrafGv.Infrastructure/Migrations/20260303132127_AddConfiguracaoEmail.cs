using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SmtpHost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    UsarSsl = table.Column<bool>(type: "bit", nullable: false),
                    SmtpUsuario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SmtpSenha = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailRemetente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeRemetente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Habilitado = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesEmail", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesEmail");
        }
    }
}
