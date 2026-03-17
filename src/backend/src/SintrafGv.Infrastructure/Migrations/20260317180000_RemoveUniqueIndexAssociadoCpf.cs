using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <summary>Remove o índice único de CPF; validação de duplicidade fica apenas no backend.</summary>
    public partial class RemoveUniqueIndexAssociadoCpf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Associados_Cpf' AND object_id = OBJECT_ID('Associados'))
                    DROP INDEX IX_Associados_Cpf ON Associados;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Associados_Cpf",
                table: "Associados",
                column: "Cpf");
        }
    }
}
