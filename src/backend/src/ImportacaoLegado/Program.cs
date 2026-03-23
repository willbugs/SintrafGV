using System.Data;
using System.Globalization;
using System.Text;
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

// 3. Normalização de cidades já persistidas no destino
Console.WriteLine("--- 3. Normalizando cidades em Associados (destino) ---");
var cidadesAjustadas = await NormalizarCidadesAssociadosAsync(db);
Console.WriteLine($"   Cidades ajustadas: {cidadesAjustadas} registro(s)\n");

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
    var ignoradosSemCpf = 0;
    var batchSize = 100;

    // Consolida duplicidades por CPF normalizado e enriquece dados
    var consolidados = ConsolidarPessoasPorCpf(lista, out ignoradosSemCpf);
    Console.WriteLine($"   Legado: {lista.Count} registro(s); consolidados por CPF: {consolidados.Count}; ignorados sem CPF válido: {ignoradosSemCpf}");

    for (var i = 0; i < consolidados.Count; i += batchSize)
    {
        var batch = consolidados.Skip(i).Take(batchSize);
        foreach (var p in batch)
        {
            if (!Guid.TryParse(p.Sguid, out var id)) continue;
            if (await db.Associados.AnyAsync(a => a.Cpf == p.CpfNormalizado)) continue;

            var banco = p.BancoNome != null
                ? $"{p.BancoNome} ({p.BancoNumero ?? ""})".TrimEnd(' ', '(', ')')
                : null;

            var associado = new Associado
            {
                Id = id,
                Nome = p.NOME ?? "",
                Cpf = p.CpfNormalizado,
                MatriculaSindicato = p.MATRICULASINDICATO,
                MatriculaBancaria = p.MATRICULABANCARIA,
                Sexo = p.SEXO,
                EstadoCivil = p.ESTADOCIVIL,
                DataNascimento = p.DATNASCIMENTO,
                Naturalidade = p.NATURALIDADE,
                Endereco = p.ENDERECO,
                Bairro = p.BAIRRO,
                Cidade = NormalizarCidade(p.CIDADE),
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

static async Task<int> NormalizarCidadesAssociadosAsync(AppDbContext db)
{
    var count = 0;
    const int batchSize = 500;
    var offset = 0;

    while (true)
    {
        var batch = await db.Associados
            .OrderBy(a => a.Id)
            .Skip(offset)
            .Take(batchSize)
            .ToListAsync();

        if (batch.Count == 0) break;

        foreach (var associado in batch)
        {
            var atual = associado.Cidade;
            var normalizada = NormalizarCidade(atual);
            if (!string.Equals(atual, normalizada, StringComparison.Ordinal))
            {
                associado.Cidade = normalizada;
                count++;
            }
        }

        await db.SaveChangesAsync();
        offset += batch.Count;
    }

    return count;
}

static List<LegadoPessoaConsolidada> ConsolidarPessoasPorCpf(List<LegadoPessoa> lista, out int ignoradosSemCpf)
{
    ignoradosSemCpf = 0;
    var comCpf = new List<LegadoPessoaComCpf>(lista.Count);
    foreach (var p in lista)
    {
        var cpfNorm = NormalizarCpf(p.CPF);
        if (string.IsNullOrWhiteSpace(cpfNorm) || cpfNorm.Length != 11)
        {
            ignoradosSemCpf++;
            continue;
        }

        comCpf.Add(new LegadoPessoaComCpf(p, cpfNorm));
    }

    var consolidados = new List<LegadoPessoaConsolidada>();
    foreach (var grupo in comCpf.GroupBy(x => x.CpfNormalizado))
    {
        var registros = grupo.Select(g => g.Pessoa).ToList();
        var principal = EscolherPrincipal(registros);
        var enriquecido = EnriquecerPrincipal(principal, registros);
        consolidados.Add(new LegadoPessoaConsolidada(enriquecido, grupo.Key));
    }

    return consolidados;
}

static LegadoPessoa EscolherPrincipal(List<LegadoPessoa> grupo)
{
    return grupo
        .OrderByDescending(p => p.ATIVO ?? false)
        .ThenByDescending(p => p.DTULTIMAATUALIZACAO ?? DateTime.MinValue)
        .ThenByDescending(ScoreCompletude)
        .First();
}

static int ScoreCompletude(LegadoPessoa p)
{
    var score = 0;
    if (!string.IsNullOrWhiteSpace(p.NOME)) score += 2;
    if (!string.IsNullOrWhiteSpace(p.EMAIL)) score += 2;
    if (!string.IsNullOrWhiteSpace(p.CELULAR)) score += 2;
    if (!string.IsNullOrWhiteSpace(p.TELEFONE)) score += 1;
    if (!string.IsNullOrWhiteSpace(p.MATRICULABANCARIA)) score += 2;
    if (!string.IsNullOrWhiteSpace(p.MATRICULASINDICATO)) score += 1;
    if (!string.IsNullOrWhiteSpace(p.ENDERECO)) score += 1;
    if (!string.IsNullOrWhiteSpace(p.BAIRRO)) score += 1;
    if (!string.IsNullOrWhiteSpace(NormalizarCidade(p.CIDADE))) score += 1;
    if (!string.IsNullOrWhiteSpace(p.ESTADO)) score += 1;
    if (!string.IsNullOrWhiteSpace(p.BancoNome)) score += 1;
    if (p.DATNASCIMENTO.HasValue) score += 1;
    return score;
}

static LegadoPessoa EnriquecerPrincipal(LegadoPessoa principal, List<LegadoPessoa> grupo)
{
    var ordenados = grupo
        .OrderByDescending(p => p.ATIVO ?? false)
        .ThenByDescending(p => p.DTULTIMAATUALIZACAO ?? DateTime.MinValue)
        .ToList();

    var p = principal;
    foreach (var s in ordenados)
    {
        p = p with
        {
            NOME = PrimeiroPreenchido(p.NOME, s.NOME),
            MATRICULASINDICATO = PrimeiroPreenchido(p.MATRICULASINDICATO, s.MATRICULASINDICATO),
            MATRICULABANCARIA = PrimeiroPreenchido(p.MATRICULABANCARIA, s.MATRICULABANCARIA),
            SEXO = PrimeiroPreenchido(p.SEXO, s.SEXO),
            ESTADOCIVIL = PrimeiroPreenchido(p.ESTADOCIVIL, s.ESTADOCIVIL),
            DATNASCIMENTO = p.DATNASCIMENTO ?? s.DATNASCIMENTO,
            NATURALIDADE = PrimeiroPreenchido(p.NATURALIDADE, s.NATURALIDADE),
            ENDERECO = PrimeiroPreenchido(p.ENDERECO, s.ENDERECO),
            BAIRRO = PrimeiroPreenchido(p.BAIRRO, s.BAIRRO),
            CIDADE = PrimeiroPreenchido(p.CIDADE, s.CIDADE),
            ESTADO = PrimeiroPreenchido(p.ESTADO, s.ESTADO),
            CODAGENCIA = PrimeiroPreenchido(p.CODAGENCIA, s.CODAGENCIA),
            AGENCIA = PrimeiroPreenchido(p.AGENCIA, s.AGENCIA),
            CONTA = PrimeiroPreenchido(p.CONTA, s.CONTA),
            FUNCAO = PrimeiroPreenchido(p.FUNCAO, s.FUNCAO),
            CTPS = PrimeiroPreenchido(p.CTPS, s.CTPS),
            SERIE = PrimeiroPreenchido(p.SERIE, s.SERIE),
            DATADMISSAO = p.DATADMISSAO ?? s.DATADMISSAO,
            DATFILIACAO = p.DATFILIACAO ?? s.DATFILIACAO,
            DATDESLIGAMENTO = p.DATDESLIGAMENTO ?? s.DATDESLIGAMENTO,
            CELULAR = PrimeiroPreenchido(p.CELULAR, s.CELULAR),
            TELEFONE = PrimeiroPreenchido(p.TELEFONE, s.TELEFONE),
            EMAIL = PrimeiroPreenchido(p.EMAIL, s.EMAIL),
            ATIVO = (p.ATIVO ?? false) || (s.ATIVO ?? false),
            APOSENTADO = (p.APOSENTADO ?? false) || (s.APOSENTADO ?? false),
            DTULTIMAATUALIZACAO = MaxDate(p.DTULTIMAATUALIZACAO, s.DTULTIMAATUALIZACAO),
            CARTEIRINHA = PrimeiroPreenchido(p.CARTEIRINHA, s.CARTEIRINHA),
            BASE = PrimeiroPreenchido(p.BASE, s.BASE),
            MOTIVO = PrimeiroPreenchido(p.MOTIVO, s.MOTIVO),
            BancoNome = PrimeiroPreenchido(p.BancoNome, s.BancoNome),
            BancoNumero = PrimeiroPreenchido(p.BancoNumero, s.BancoNumero)
        };
    }
    p = p with { CIDADE = NormalizarCidade(p.CIDADE) };
    return p;
}

static string? PrimeiroPreenchido(string? atual, string? candidato)
{
    if (!string.IsNullOrWhiteSpace(atual)) return atual;
    return string.IsNullOrWhiteSpace(candidato) ? atual : candidato;
}

static DateTime? MaxDate(DateTime? a, DateTime? b)
{
    if (!a.HasValue) return b;
    if (!b.HasValue) return a;
    return a.Value >= b.Value ? a : b;
}

static string? NormalizarCidade(string? cidade)
{
    if (string.IsNullOrWhiteSpace(cidade)) return null;

    var limpa = Regex.Replace(cidade.Trim(), @"\s+", " ");
    var key = GerarChaveCidade(limpa);
    if (string.IsNullOrWhiteSpace(key)) return null;
    if (CidadeInvalida(key)) return null;

    var canonica = CidadeCanonicaPorAlias(key);
    if (canonica is not null) return canonica;

    return ToTitleCase(limpa);
}

static string GerarChaveCidade(string valor)
{
    var semAcento = RemoverAcentos(valor).ToUpperInvariant();
    semAcento = semAcento.Replace("STRING:", " ");
    var soLetrasEspacos = Regex.Replace(semAcento, @"[^A-Z0-9 ]", " ");
    var normalizado = Regex.Replace(soLetrasEspacos, @"\s+", " ").Trim();
    return normalizado;
}

static string RemoverAcentos(string texto)
{
    var normalized = texto.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder(normalized.Length);
    foreach (var ch in normalized)
    {
        var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
        if (uc != UnicodeCategory.NonSpacingMark)
            sb.Append(ch);
    }
    return sb.ToString().Normalize(NormalizationForm.FormC);
}

static string ToTitleCase(string valor)
{
    var ti = CultureInfo.GetCultureInfo("pt-BR").TextInfo;
    return ti.ToTitleCase(valor.ToLowerInvariant());
}

static bool CidadeInvalida(string key)
{
    if (Regex.IsMatch(key, @"^\d+$")) return true;
    return key is "NNNN" or "CONS";
}

static string? CidadeCanonicaPorAlias(string key)
{
    if (Regex.IsMatch(key, @"^(GOV|GOVERNADOR|GOVERNASOR|GIVERNADOR|GOVERNADOD|GOVAL|GV)\b")
        && key.Contains("VALAD"))
        return "Governador Valadares";

    return key switch
    {
        // Governador Valadares e variações reais do legado
        "GOV VALADARES" => "Governador Valadares",
        "GOV VALADARES MG" => "Governador Valadares",
        "GOV VALDARES" => "Governador Valadares",
        "GOV VALARES" => "Governador Valadares",
        "GOVERNADOR VALADARES" => "Governador Valadares",
        "GOVERNADOR VALADARES MG" => "Governador Valadares",
        "GOVERNADOR VALADARES " => "Governador Valadares",
        "GOVERNADOR VALADSRES" => "Governador Valadares",
        "GOVERNADOR VALADARS" => "Governador Valadares",
        "GOVERNADOR VALADARTES" => "Governador Valadares",
        "GOVERNADOR VALALDARES" => "Governador Valadares",
        "GOVERNADOR CALADARES" => "Governador Valadares",
        "G V" => "Governador Valadares",
        "GV" => "Governador Valadares",
        "GOVAL" => "Governador Valadares",

        // Belo Horizonte
        "BH" => "Belo Horizonte",
        "B H" => "Belo Horizonte",
        "BELO HORIZONTE MG" => "Belo Horizonte",
        "BELOHORIZONTE" => "Belo Horizonte",
        "BH SUL" => "Belo Horizonte",

        // Cidades com sufixo de UF e/ou grafia variante
        "IPATINGA MG" => "Ipatinga",
        "CORONEL FABRICIANO MG" => "Coronel Fabriciano",
        "TIMOTEO MG" => "Timóteo",
        "TEOFILO OTONI" => "Teófilo Otoni",
        "SAO JOAO EVANGELISTA" => "São João Evangelista",
        "SAO JOAO DO MANTENINHA" => "São João do Manteninha",
        "SAO JOAO DO ORIENTE" => "São João do Oriente",
        "SAO JOSE JACURI" => "São José do Jacuri",
        "SAO JOSE DO JACURI" => "São José do Jacuri",
        "SANTANA DO PARAISO" => "Santana do Paraíso",
        "PADRE PARAISO" => "Padre Paraíso",
        "VIRGINOPOLIS" => "Virginópolis",
        "AIMORES" => "Aimorés",
        "MANHUACU" => "Manhuaçu",
        "ITABIRINHA" => "Itabirinha de Mantena",
        "ITABIRINHA DE MANTENA" => "Itabirinha de Mantena",
        "PARIQUITO" => "Periquito",
        "CONS PENA" => "Conselheiro Pena",
        "ENG CALDAS" => "Engenheiro Caldas",
        "TUMIRITINGA MG" => "Tumiritinga",

        // Ruído/valores operacionais - mantidos por transparência de regra
        "UGAC VALE DO ACO" => "Vale do Aço",
        "VALE DO ACO" => "Vale do Aço",
        "GPSA MINAS I" => "Governador Valadares",
        "GPSA MINAS II" => "Governador Valadares",

        _ => null
    };
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

record LegadoPessoaComCpf(LegadoPessoa Pessoa, string CpfNormalizado);
record LegadoPessoaConsolidada(
    string Sguid, string? NOME, string? CPF, string CpfNormalizado, string? MATRICULASINDICATO, string? MATRICULABANCARIA,
    string? SEXO, string? ESTADOCIVIL, DateTime? DATNASCIMENTO, string? NATURALIDADE,
    string? ENDERECO, string? BAIRRO, string? CIDADE, string? ESTADO,
    string? CODAGENCIA, string? AGENCIA, string? CONTA,
    string? FUNCAO, string? CTPS, string? SERIE,
    DateTime? DATADMISSAO, DateTime? DATFILIACAO, DateTime? DATDESLIGAMENTO,
    string? CELULAR, string? TELEFONE, string? EMAIL,
    bool? ATIVO, bool? APOSENTADO, DateTime? DTULTIMAATUALIZACAO,
    string? CARTEIRINHA, string? BASE, string? MOTIVO,
    string? BancoNome, string? BancoNumero)
{
    public LegadoPessoaConsolidada(LegadoPessoa p, string cpfNorm) : this(
        p.Sguid, p.NOME, p.CPF, cpfNorm, p.MATRICULASINDICATO, p.MATRICULABANCARIA,
        p.SEXO, p.ESTADOCIVIL, p.DATNASCIMENTO, p.NATURALIDADE,
        p.ENDERECO, p.BAIRRO, p.CIDADE, p.ESTADO,
        p.CODAGENCIA, p.AGENCIA, p.CONTA,
        p.FUNCAO, p.CTPS, p.SERIE,
        p.DATADMISSAO, p.DATFILIACAO, p.DATDESLIGAMENTO,
        p.CELULAR, p.TELEFONE, p.EMAIL,
        p.ATIVO, p.APOSENTADO, p.DTULTIMAATUALIZACAO,
        p.CARTEIRINHA, p.BASE, p.MOTIVO,
        p.BancoNome, p.BancoNumero) { }
}
