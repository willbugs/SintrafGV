using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SintrafGv.Domain.Entities;
using SintrafGv.Infrastructure.Data;

// Configuração (usa diretório do executável para encontrar appsettings.json)
var basePath = AppContext.BaseDirectory;
var config = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connLegado = config.GetConnectionString("Legado")
    ?? throw new InvalidOperationException("ConnectionStrings:Legado não configurado.");
var connNovo = config.GetConnectionString("Novo")
    ?? throw new InvalidOperationException("ConnectionStrings:Novo não configurado.");

Console.WriteLine("=== Importação Legado → Sintraf_GV ===\n");
Console.WriteLine("Banco legado (somente leitura): Sintrafgv");
Console.WriteLine("Banco novo (destino): Sintraf_GV\n");

// DbContext para o banco novo
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connNovo)
    .Options;

using var db = new AppDbContext(options);

// 1. EMPRESAS → ConfiguracoesSindicato
Console.WriteLine("--- 1. Importando EMPRESAS → ConfiguracoesSindicato ---");
var empresasCount = await ImportarEmpresasAsync(connLegado, db);
Console.WriteLine($"   Importados: {empresasCount} registro(s)\n");

// 2. PESSOAS + BANCOS → Associados
Console.WriteLine("--- 2. Importando PESSOAS + BANCOS → Associados ---");
var pessoasCount = await ImportarPessoasAsync(connLegado, db);
Console.WriteLine($"   Importados: {pessoasCount} registro(s)\n");

Console.WriteLine("=== Importação concluída ===");

static async Task<int> ImportarEmpresasAsync(string connLegado, AppDbContext db)
{
    using var conn = new SqlConnection(connLegado);
    var empresas = await conn.QueryAsync<LegadoEmpresa>(@"
        SELECT Sguid, NOMEREDUZIDO, EMITENTE_CNPJ, EMITENTE_IE, EMITENTE_RAZAOSOCIAL, EMITENTE_FANTASIA, EMITENTE_UF
        FROM EMPRESAS");
    var lista = empresas.ToList();
    var count = 0;

    foreach (var e in lista)
    {
        if (!Guid.TryParse(e.Sguid, out var id)) continue;
        if (await db.ConfiguracoesSindicato.AnyAsync(c => c.Id == id)) continue;

        var config = new ConfiguracaoSindicato
        {
            Id = id,
            RazaoSocial = e.EMITENTE_RAZAOSOCIAL ?? "A preencher",
            NomeFantasia = e.EMITENTE_FANTASIA ?? e.NOMEREDUZIDO ?? "A preencher",
            CNPJ = NormalizarCnpj(e.EMITENTE_CNPJ) ?? "00000000000000",
            InscricaoEstadual = e.EMITENTE_IE,
            UF = e.EMITENTE_UF ?? "MG",
            Endereco = "A preencher",
            Numero = "S/N",
            Bairro = "A preencher",
            Cidade = "A preencher",
            CEP = "00000000",
            Presidente = "A preencher",
            CPFPresidente = "00000000000000",
            CriadoEm = DateTime.UtcNow
        };

        db.ConfiguracoesSindicato.Add(config);
        count++;
    }

    if (count > 0) await db.SaveChangesAsync();
    return count;
}

static async Task<int> ImportarPessoasAsync(string connLegado, AppDbContext db)
{
    using var conn = new SqlConnection(connLegado);
    var pessoas = await conn.QueryAsync<LegadoPessoa>(@"
        SELECT p.Sguid, p.NOME, p.CPF, p.MATRICULASINDICATO, p.MATRICULABANCARIA,
               p.SEXO, p.ESTADOCIVIL, p.DATNASCIMENTO, p.NATURALIDADE,
               p.ENDERECO, p.BAIRRO, p.CIDADE, p.ESTADO,
               p.CODAGENCIA, p.AGENCIA, p.CONTA,
               p.FUNCAO, p.CTPS, p.SERIE,
               p.DATADMISSAO, p.DATFILIACAO, p.DATDESLIGAMENTO,
               p.CELULAR, p.TELEFONE, p.EMAIL,
               p.ATIVO, p.APOSENTADO, p.DTULTIMAATUALIZACAO,
               p.CARTEIRINHA, p.BASE, p.MOTIVO,
               b.NOME AS BancoNome, b.NUMERO AS BancoNumero
        FROM PESSOAS p
        LEFT JOIN BANCOS b ON p.BANCO = b.Sguid");
    var lista = pessoas.ToList();
    var count = 0;
    var batchSize = 100;

    for (var i = 0; i < lista.Count; i += batchSize)
    {
        var batch = lista.Skip(i).Take(batchSize);
        foreach (var p in batch)
        {
            if (!Guid.TryParse(p.Sguid, out var id)) continue;
            if (await db.Associados.AnyAsync(a => a.Id == id)) continue;

            var banco = p.BancoNome != null
                ? $"{p.BancoNome} ({p.BancoNumero ?? ""})".TrimEnd(' ', '(', ')')
                : null;

            var associado = new Associado
            {
                Id = id,
                Nome = p.NOME ?? "",
                Cpf = NormalizarCpf(p.CPF) ?? "",
                MatriculaSindicato = p.MATRICULASINDICATO,
                MatriculaBancaria = p.MATRICULABANCARIA,
                Sexo = p.SEXO,
                EstadoCivil = p.ESTADOCIVIL,
                DataNascimento = p.DATNASCIMENTO,
                Naturalidade = p.NATURALIDADE,
                Endereco = p.ENDERECO,
                Bairro = p.BAIRRO,
                Cidade = p.CIDADE,
                Estado = p.ESTADO,
                CodAgencia = p.CODAGENCIA,
                Agencia = p.AGENCIA,
                Conta = p.CONTA,
                Banco = banco,
                Funcao = p.FUNCAO,
                Ctps = p.CTPS,
                Serie = p.SERIE,
                Carteirinha = p.CARTEIRINHA,
                Base = p.BASE,
                Motivo = p.MOTIVO,
                DataAdmissao = p.DATADMISSAO,
                DataFiliacao = p.DATFILIACAO,
                DataDesligamento = p.DATDESLIGAMENTO,
                Celular = p.CELULAR,
                Telefone = p.TELEFONE,
                Email = p.EMAIL,
                Ativo = p.ATIVO ?? true,
                Aposentado = p.APOSENTADO ?? false,
                DataUltimaAtualizacao = p.DTULTIMAATUALIZACAO,
                CriadoEm = p.DTULTIMAATUALIZACAO ?? DateTime.UtcNow
            };

            db.Associados.Add(associado);
            count++;
        }
        await db.SaveChangesAsync();
    }

    return count;
}

static string? NormalizarCpf(string? cpf)
{
    if (string.IsNullOrWhiteSpace(cpf)) return null;
    var digits = Regex.Replace(cpf, @"\D", "");
    return digits.Length == 11 ? digits : cpf.Trim();
}

static string? NormalizarCnpj(string? cnpj)
{
    if (string.IsNullOrWhiteSpace(cnpj)) return null;
    var digits = Regex.Replace(cnpj, @"\D", "");
    return digits.Length == 14 ? digits : cnpj.Trim();
}

// DTOs para leitura do legado
record LegadoEmpresa(string Sguid, string? NOMEREDUZIDO, string? EMITENTE_CNPJ, string? EMITENTE_IE,
    string? EMITENTE_RAZAOSOCIAL, string? EMITENTE_FANTASIA, string? EMITENTE_UF);

record LegadoPessoa(string Sguid, string? NOME, string? CPF, string? MATRICULASINDICATO, string? MATRICULABANCARIA,
    string? SEXO, string? ESTADOCIVIL, DateTime? DATNASCIMENTO, string? NATURALIDADE,
    string? ENDERECO, string? BAIRRO, string? CIDADE, string? ESTADO,
    string? CODAGENCIA, string? AGENCIA, string? CONTA,
    string? FUNCAO, string? CTPS, string? SERIE,
    DateTime? DATADMISSAO, DateTime? DATFILIACAO, DateTime? DATDESLIGAMENTO,
    string? CELULAR, string? TELEFONE, string? EMAIL,
    bool? ATIVO, bool? APOSENTADO, DateTime? DTULTIMAATUALIZACAO,
    string? CARTEIRINHA, string? BASE, string? MOTIVO,
    string? BancoNome, string? BancoNumero);
