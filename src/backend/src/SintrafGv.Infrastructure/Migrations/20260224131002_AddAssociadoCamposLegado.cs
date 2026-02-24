using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssociadoCamposLegado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Agencia",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Aposentado",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodAgencia",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conta",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ctps",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAdmissao",
                table: "Associados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDesligamento",
                table: "Associados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFiliacao",
                table: "Associados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Associados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeAcordo",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Funcao",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Naturalidade",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Novo",
                table: "Associados",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Serie",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Associados",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Agencia",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Aposentado",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "CodAgencia",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Conta",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Ctps",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DataAdmissao",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DataDesligamento",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DataFiliacao",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "DeAcordo",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Funcao",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Naturalidade",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Novo",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Serie",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Associados");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Associados");
        }
    }
}
