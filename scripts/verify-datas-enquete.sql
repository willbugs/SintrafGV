-- Verificacao de horarios da assembleia 452221F5-6C7B-4F59-8A68-34296A50BD64
-- Timestamps no banco sao UTC. Brasilia = UTC - 3h (sem horario de verao).

SET NOCOUNT ON;

DECLARE @Id UNIQUEIDENTIFIER = '452221F5-6C7B-4F59-8A68-34296A50BD64';

SELECT
    Status,
    InicioVotacao AS InicioUtc,
    FimVotacao AS FimUtc,
    DATEADD(HOUR, -3, InicioVotacao) AS InicioBrasiliaAprox,
    DATEADD(HOUR, -3, FimVotacao) AS FimBrasiliaAprox
FROM Eleicoes WHERE Id = @Id;

SELECT COUNT(*) AS TotalVotos,
       MIN(DataHoraVoto) AS PrimeiroVotoUtc,
       MAX(DataHoraVoto) AS UltimoVotoUtc,
       DATEADD(HOUR, -3, MAX(DataHoraVoto)) AS UltimoVotoBrasiliaAprox
FROM Votos WHERE EleicaoId = @Id;

SELECT COUNT(*) AS VotosAposFimCadastrado
FROM Votos v
JOIN Eleicoes e ON e.Id = v.EleicaoId
WHERE v.EleicaoId = @Id AND v.DataHoraVoto > e.FimVotacao;

SELECT TOP 5 a.Nome, v.DataHoraVoto AS Utc, v.TimestampPreciso
FROM Votos v
JOIN Associados a ON a.Id = v.AssociadoId
WHERE v.EleicaoId = @Id
ORDER BY v.DataHoraVoto DESC;
