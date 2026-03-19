-- Adiciona a coluna BancoNome na tabela Eleicoes se ainda nao existir.
-- Use se a API retornar 500 ao listar eleicoes (GET /api/eleicoes) apos o release.
-- Execucao: sqlcmd -S SEU_SERVIDOR -d Sintraf_GV -U usuario -P senha -i Fix-Eleicoes-BancoNome.sql
-- Ou no SSMS: abra o arquivo e execute (F5).

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Eleicoes') AND name = N'BancoNome'
)
BEGIN
    ALTER TABLE dbo.Eleicoes
    ADD BancoNome nvarchar(200) NULL;
    PRINT 'Coluna BancoNome adicionada à tabela Eleicoes.';
END
ELSE
    PRINT 'Coluna BancoNome já existe.';
