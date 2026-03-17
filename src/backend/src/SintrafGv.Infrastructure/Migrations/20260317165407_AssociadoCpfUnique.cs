using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SintrafGv.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssociadoCpfUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normaliza CPFs existentes para apenas dígitos
            migrationBuilder.Sql(@"
                UPDATE Associados
                SET Cpf = REPLACE(REPLACE(REPLACE(ISNULL(Cpf,''), '.', ''), '-', ''), ' ', '')
                WHERE Cpf IS NOT NULL AND (Cpf LIKE '%.%' OR Cpf LIKE '%-%' OR Cpf LIKE '% %');
            ");
            // Remove índice único se existir; validação de CPF duplicado fica só no backend
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Associados_Cpf' AND object_id = OBJECT_ID('Associados'))
                    DROP INDEX IX_Associados_Cpf ON Associados;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não recria índice único
        }
    }
}
