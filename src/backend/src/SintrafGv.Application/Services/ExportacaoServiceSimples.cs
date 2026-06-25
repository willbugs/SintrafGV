using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using SintrafGv.Application.DTOs;
using SintrafGv.Application;

namespace SintrafGv.Application.Services
{
    public interface IExportacaoService
    {
        Task<ExportacaoRelatorioDto> ExportarPdfAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default);
        Task<ExportacaoRelatorioDto> ExportarExcelAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default);
        Task<ExportacaoRelatorioDto> ExportarCsvAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default);
    }

    public class ExportacaoService : IExportacaoService
    {
        static ExportacaoService()
        {
            // EPPlus 8+: licença obrigatória antes de usar
            ExcelPackage.License.SetNonCommercialOrganization("SintrafGV");
        }

        public async Task<ExportacaoRelatorioDto> ExportarPdfAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => GerarPdf(dados, nomeArquivo), cancellationToken);
        }

        public async Task<ExportacaoRelatorioDto> ExportarExcelAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => GerarExcel(dados, nomeArquivo), cancellationToken);
        }

        public async Task<ExportacaoRelatorioDto> ExportarCsvAsync<T>(RelatorioResponse<T> dados, string nomeArquivo, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => GerarCsv(dados, nomeArquivo), cancellationToken);
        }

        private ExportacaoRelatorioDto GerarPdf<T>(RelatorioResponse<T> dados, string nomeArquivo)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            writer.SetCloseStream(false);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Fontes
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Título (remover acentos - Helvetica não suporta Unicode)
            document.Add(new Paragraph(RemoverAcentos(dados.Metadata.Titulo))
                .SetFont(boldFont).SetFontSize(18).SetTextAlignment(TextAlignment.CENTER));

            // Subtítulo se existir
            if (!string.IsNullOrEmpty(dados.Metadata.Subtitulo))
            {
                document.Add(new Paragraph(RemoverAcentos(dados.Metadata.Subtitulo))
                    .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
            }

            // Info
            document.Add(new Paragraph(RemoverAcentos($"Gerado em: {BrasiliaTime.FormatDateTime(dados.Metadata.DataGeracao)} (horario de Brasilia) | Total: {dados.Metadata.TotalRegistros}"))
                .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));

            // Tabela - colunas exportáveis (exclui [ExportIgnore])
            var colunasParticipacao = ObterColunasParticipacaoVotacao(dados);
            var propriedades = colunasParticipacao == null
                ? ObterPropriedadesSimples<T>().ToList()
                : new List<PropertyInfo>();
            if (colunasParticipacao == null && propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).Take(8).ToList();

            var numColunas = colunasParticipacao?.Count ?? Math.Max(1, propriedades.Count);
            var tabela = new Table(numColunas).UseAllAvailableWidth();

            // Cabeçalhos
            if (colunasParticipacao != null)
            {
                foreach (var col in colunasParticipacao)
                {
                    tabela.AddHeaderCell(new Cell()
                        .Add(new Paragraph(RemoverAcentos(col.Titulo)))
                        .SetFont(boldFont).SetFontSize(10)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(TextAlignment.CENTER));
                }
            }
            else
            {
                foreach (var prop in propriedades)
                {
                    tabela.AddHeaderCell(new Cell()
                        .Add(new Paragraph(RemoverAcentos(ObterTituloColuna(prop.Name))))
                        .SetFont(boldFont).SetFontSize(10)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(TextAlignment.CENTER));
                }
                if (propriedades.Count == 0)
                    tabela.AddHeaderCell(new Cell().Add(new Paragraph("Dados")).SetFont(boldFont).SetFontSize(10).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            }

            // Dados
            foreach (var registro in dados.Dados)
            {
                if (colunasParticipacao != null)
                {
                    foreach (var col in colunasParticipacao)
                    {
                        var valor = FormatarValorParaPdf(col.ObterValor(registro), col.PropertyName);
                        tabela.AddCell(new Cell().Add(new Paragraph(valor)).SetFont(font).SetFontSize(8));
                    }
                }
                else
                {
                    foreach (var prop in propriedades)
                    {
                        var valor = FormatarValorParaPdf(prop.GetValue(registro), prop.Name);
                        tabela.AddCell(new Cell().Add(new Paragraph(valor)).SetFont(font).SetFontSize(8));
                    }
                    if (propriedades.Count == 0)
                        tabela.AddCell(new Cell().Add(new Paragraph("-")).SetFont(font).SetFontSize(8));
                }
            }

            document.Add(tabela);
            AdicionarTotalizadores(document, dados, font, boldFont);
            document.Close();

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.pdf",
                Formato = "pdf",
                Conteudo = memoryStream.ToArray(),
                ContentType = "application/pdf",
                TamanhoBytes = memoryStream.Length,
                DataGeracao = DateTime.UtcNow
            };
        }

        private ExportacaoRelatorioDto GerarExcel<T>(RelatorioResponse<T> dados, string nomeArquivo)
        {
            using var package = new ExcelPackage();
            var nomePlanilha = SanitizarNomePlanilha(dados.Metadata.Titulo);
            var worksheet = package.Workbook.Worksheets.Add(nomePlanilha);

            // Título
            worksheet.Cells[1, 1].Value = dados.Metadata.Titulo;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            var linha = 3;

            var colunasParticipacao = ObterColunasParticipacaoVotacao(dados);
            var propriedades = colunasParticipacao == null
                ? ObterPropriedadesSimples<T>().ToList()
                : new List<PropertyInfo>();
            if (colunasParticipacao == null && propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();

            var coluna = 1;
            if (colunasParticipacao != null)
            {
                foreach (var col in colunasParticipacao)
                {
                    worksheet.Cells[linha, coluna].Value = col.Titulo;
                    worksheet.Cells[linha, coluna].Style.Font.Bold = true;
                    coluna++;
                }
            }
            else
            {
                foreach (var prop in propriedades)
                {
                    worksheet.Cells[linha, coluna].Value = ObterTituloColuna(prop.Name);
                    worksheet.Cells[linha, coluna].Style.Font.Bold = true;
                    coluna++;
                }
            }

            // Dados
            linha++;
            foreach (var registro in dados.Dados)
            {
                coluna = 1;
                if (colunasParticipacao != null)
                {
                    foreach (var col in colunasParticipacao)
                    {
                        EscreverCelulaExcel(worksheet, linha, coluna, col.ObterValor(registro), col.PropertyName);
                        coluna++;
                    }
                }
                else
                {
                    foreach (var prop in propriedades)
                    {
                        EscreverCelulaExcel(worksheet, linha, coluna, prop.GetValue(registro), prop.Name);
                        coluna++;
                    }
                }
                linha++;
            }

            // Autofit (Dimension pode ser null se planilha vazia)
            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            AdicionarTotalizadoresPlanilha(worksheet, dados, ref linha);

            var conteudo = package.GetAsByteArray();

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.xlsx",
                Formato = "xlsx",
                Conteudo = conteudo,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TamanhoBytes = conteudo.Length,
                DataGeracao = DateTime.UtcNow
            };
        }

        private ExportacaoRelatorioDto GerarCsv<T>(RelatorioResponse<T> dados, string nomeArquivo)
        {
            var csv = new StringBuilder();

            // Cabeçalho informativo
            csv.AppendLine($"# {dados.Metadata.Titulo}");
            csv.AppendLine($"# Gerado em: {BrasiliaTime.FormatDateTime(dados.Metadata.DataGeracao)} (horario de Brasilia)");
            csv.AppendLine($"# Total de registros: {dados.Metadata.TotalRegistros}");
            csv.AppendLine(); // Linha em branco

            // Cabeçalhos das colunas - excluir listas e dicionários
            var colunasParticipacao = ObterColunasParticipacaoVotacao(dados);
            var propriedades = colunasParticipacao == null
                ? ObterPropriedadesSimples<T>().ToList()
                : new List<PropertyInfo>();
            if (colunasParticipacao == null && propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();

            if (colunasParticipacao != null)
                csv.AppendLine(string.Join(";", colunasParticipacao.Select(c => EscaparCsv(c.Titulo))));
            else
                csv.AppendLine(string.Join(";", propriedades.Select(p => ObterTituloColuna(p.Name))));

            // Dados
            foreach (var registro in dados.Dados)
            {
                var valores = new List<string>();
                if (colunasParticipacao != null)
                {
                    foreach (var col in colunasParticipacao)
                        valores.Add(EscaparCsv(FormatarValorCsv(col.ObterValor(registro), col.PropertyName)));
                }
                else
                {
                    foreach (var prop in propriedades)
                    {
                        var valor = prop.GetValue(registro);
                        var valorFormatado = valor switch
                        {
                            null => "",
                            DateTime data => BrasiliaTime.FormatForDisplay(data, prop.Name),
                            bool boolean => boolean ? "Sim" : "Não",
                            TimeSpan ts => ts.ToString(@"hh\:mm\:ss"),
                            IEnumerable and not string => "-",
                            _ => valor.ToString()
                        };
                        valores.Add(EscaparCsv(valorFormatado ?? ""));
                    }
                }
                csv.AppendLine(string.Join(";", valores));
            }

            AdicionarTotalizadoresCsv(csv, dados);

            var conteudo = Encoding.UTF8.GetBytes(csv.ToString());

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.csv",
                Formato = "csv",
                Conteudo = conteudo,
                ContentType = "text/csv; charset=utf-8",
                TamanhoBytes = conteudo.Length,
                DataGeracao = DateTime.UtcNow
            };
        }

        private static List<PropertyInfo> ObterPropriedadesSimples<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => p.GetCustomAttribute<ExportIgnoreAttribute>() == null)
                .Where(p =>
                {
                    var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string))
                        return false;
                    if (typeof(IDictionary).IsAssignableFrom(p.PropertyType))
                        return false;
                    return t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(TimeSpan) || t == typeof(Guid);
                })
                .ToList();
        }

        private static string RemoverAcentos(string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string FormatarValorParaPdf(object? valor, string? propertyName = null)
        {
            if (valor == null) return "";
            if (valor is IEnumerable and not string) return "-";
            if (valor is DateTime data)
                return RemoverAcentos(BrasiliaTime.FormatForDisplay(data, propertyName));
            return RemoverAcentos(valor.ToString());
        }

        private static string SanitizarNomePlanilha(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return "Relatorio";
            var invalidos = new[] { '\\', '/', '*', '?', ':', '[', ']' };
            var sanitizado = new string(titulo.Where(c => !invalidos.Contains(c)).ToArray()).Trim();
            return sanitizado.Length > 31 ? sanitizado[..31] : sanitizado;
        }

        private static bool TemTotalizadoresVotacao<T>(RelatorioResponse<T> dados) =>
            dados.Totalizadores != null &&
            (dados.Totalizadores.ContainsKey("resultadoPorOpcao") ||
             dados.Totalizadores.ContainsKey("ResultadoPorOpcao") ||
             dados.Totalizadores.ContainsKey("votosSim") ||
             dados.Totalizadores.ContainsKey("VotosSim") ||
             dados.Totalizadores.ContainsKey("totalAssociados") ||
             dados.Totalizadores.ContainsKey("TotalAssociados"));

        private static Dictionary<string, int> LerResultadoPorOpcao(Dictionary<string, object> tot)
        {
            if (!tot.TryGetValue("resultadoPorOpcao", out var raw) &&
                !tot.TryGetValue("ResultadoPorOpcao", out raw))
                return new Dictionary<string, int>();

            if (raw is Dictionary<string, int> dictInt)
                return dictInt;

            if (raw is JsonElement json && json.ValueKind == JsonValueKind.Object)
            {
                var result = new Dictionary<string, int>();
                foreach (var prop in json.EnumerateObject())
                    result[prop.Name] = prop.Value.ValueKind == JsonValueKind.Number
                        ? prop.Value.GetInt32()
                        : int.TryParse(prop.Value.ToString(), out var n) ? n : 0;
                return result;
            }

            if (raw is IDictionary<string, object> dictObj)
            {
                return dictObj.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value switch
                    {
                        int i => i,
                        long l => (int)l,
                        _ => int.TryParse(kvp.Value?.ToString(), out var n) ? n : 0
                    });
            }

            return new Dictionary<string, int>();
        }

        private static List<string> LerOrdemTotalizadores(Dictionary<string, object> tot, Dictionary<string, int> resultado)
        {
            if (tot.TryGetValue("ordemTotalizadores", out var ordemRaw) ||
                tot.TryGetValue("OrdemTotalizadores", out ordemRaw))
            {
                if (ordemRaw is IEnumerable<string> listaStr)
                    return listaStr.Where(resultado.ContainsKey).ToList();
                if (ordemRaw is JsonElement json && json.ValueKind == JsonValueKind.Array)
                {
                    var ordem = new List<string>();
                    foreach (var item in json.EnumerateArray())
                    {
                        var chave = item.GetString();
                        if (!string.IsNullOrEmpty(chave) && resultado.ContainsKey(chave))
                            ordem.Add(chave);
                    }
                    if (ordem.Count > 0) return ordem;
                }
            }
            return resultado.Keys.ToList();
        }

        private static object LerTotalizadorVotacao(Dictionary<string, object> tot, params string[] chaves)
        {
            foreach (var chave in chaves)
            {
                if (tot.TryGetValue(chave, out var valor) && valor != null)
                    return valor;
            }
            return 0;
        }

        private static void AdicionarTotalizadores<T>(Document document, RelatorioResponse<T> dados, PdfFont font, PdfFont boldFont)
        {
            if (!TemTotalizadoresVotacao(dados))
                return;

            var tot = dados.Totalizadores!;
            document.Add(new Paragraph("\n"));
            var total = LerTotalizadorVotacao(tot, "totalAssociados", "TotalAssociados");
            if (total.Equals(0) && dados.Metadata.TotalRegistros > 0)
                total = dados.Metadata.TotalRegistros;

            var resultado = LerResultadoPorOpcao(tot);
            if (resultado.Count > 0)
            {
                document.Add(new Paragraph(RemoverAcentos($"Total de votantes: {total}"))
                    .SetFont(boldFont).SetFontSize(11));
                foreach (var opcao in LerOrdemTotalizadores(tot, resultado))
                {
                    document.Add(new Paragraph(RemoverAcentos($"{opcao}: {resultado[opcao]}"))
                        .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));
                }
                return;
            }

            var sim = LerTotalizadorVotacao(tot, "votosSim", "VotosSim");
            var nao = LerTotalizadorVotacao(tot, "votosNao", "VotosNao");
            var branco = LerTotalizadorVotacao(tot, "votosBranco", "VotosBranco");
            var resumo = $"Total: {total} | Sim: {sim} | Nao: {nao} | Branco: {branco}";
            document.Add(new Paragraph(RemoverAcentos(resumo))
                .SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.RIGHT));
        }

        private static void AdicionarTotalizadoresPlanilha<T>(ExcelWorksheet worksheet, RelatorioResponse<T> dados, ref int linha)
        {
            if (!TemTotalizadoresVotacao(dados))
                return;

            var tot = dados.Totalizadores!;
            linha += 2;
            worksheet.Cells[linha, 1].Value = "Totalizadores";
            worksheet.Cells[linha, 1].Style.Font.Bold = true;
            linha++;
            worksheet.Cells[linha, 1].Value = "Total de votantes";
            worksheet.Cells[linha, 2].Value = LerTotalizadorVotacao(tot, "totalAssociados", "TotalAssociados");
            linha++;

            var resultado = LerResultadoPorOpcao(tot);
            if (resultado.Count > 0)
            {
                foreach (var opcao in LerOrdemTotalizadores(tot, resultado))
                {
                    worksheet.Cells[linha, 1].Value = opcao;
                    worksheet.Cells[linha, 2].Value = resultado[opcao];
                    linha++;
                }
                return;
            }

            worksheet.Cells[linha, 1].Value = "Votos Sim";
            worksheet.Cells[linha, 2].Value = LerTotalizadorVotacao(tot, "votosSim", "VotosSim");
            linha++;
            worksheet.Cells[linha, 1].Value = "Votos Não";
            worksheet.Cells[linha, 2].Value = LerTotalizadorVotacao(tot, "votosNao", "VotosNao");
            linha++;
            worksheet.Cells[linha, 1].Value = "Votos Branco";
            worksheet.Cells[linha, 2].Value = LerTotalizadorVotacao(tot, "votosBranco", "VotosBranco");
        }

        private static void AdicionarTotalizadoresCsv<T>(StringBuilder csv, RelatorioResponse<T> dados)
        {
            if (!TemTotalizadoresVotacao(dados))
                return;

            var tot = dados.Totalizadores!;
            csv.AppendLine();
            csv.AppendLine("# Totalizadores");
            csv.AppendLine($"# Total de votantes;{LerTotalizadorVotacao(tot, "totalAssociados", "TotalAssociados")}");

            var resultado = LerResultadoPorOpcao(tot);
            if (resultado.Count > 0)
            {
                foreach (var opcao in LerOrdemTotalizadores(tot, resultado))
                    csv.AppendLine($"# {EscaparCsv(opcao)};{resultado[opcao]}");
                return;
            }

            csv.AppendLine($"# Votos Sim;{LerTotalizadorVotacao(tot, "votosSim", "VotosSim")}");
            csv.AppendLine($"# Votos Nao;{LerTotalizadorVotacao(tot, "votosNao", "VotosNao")}");
            csv.AppendLine($"# Votos Branco;{LerTotalizadorVotacao(tot, "votosBranco", "VotosBranco")}");
        }

        private sealed class ColunaExportDef
        {
            public string Titulo { get; init; } = string.Empty;
            public string PropertyName { get; init; } = string.Empty;
            public Func<object, object?> ObterValor { get; init; } = _ => null;
        }

        private static List<ColunaExportDef>? ObterColunasParticipacaoVotacao<T>(RelatorioResponse<T> dados)
        {
            if (typeof(T) != typeof(ParticipacaoVotacaoDto))
                return null;

            var cols = new List<ColunaExportDef>();
            foreach (var prop in ObterPropriedadesSimples<ParticipacaoVotacaoDto>())
            {
                var propInfo = prop;
                cols.Add(new ColunaExportDef
                {
                    Titulo = ObterTituloColuna(propInfo.Name),
                    PropertyName = propInfo.Name,
                    ObterValor = r => propInfo.GetValue((ParticipacaoVotacaoDto)r)
                });
            }

            foreach (var campo in dados.Metadata?.CamposDisponiveis ?? new List<CampoRelatorio>())
            {
                var perguntaId = campo.Nome;
                cols.Add(new ColunaExportDef
                {
                    Titulo = campo.Titulo,
                    ObterValor = r =>
                    {
                        var dto = (ParticipacaoVotacaoDto)r;
                        return dto.Respostas.GetValueOrDefault(perguntaId, "—");
                    }
                });
            }

            return cols;
        }

        private static void EscreverCelulaExcel(ExcelWorksheet worksheet, int linha, int coluna, object? valor, string? propertyName = null)
        {
            if (valor == null) return;
            if (valor is DateTime data)
            {
                worksheet.Cells[linha, coluna].Value = BrasiliaTime.FormatForDisplay(data, propertyName);
            }
            else if (valor is bool boolean)
            {
                worksheet.Cells[linha, coluna].Value = boolean ? "Sim" : "Não";
            }
            else if (valor is TimeSpan ts)
            {
                worksheet.Cells[linha, coluna].Value = ts.ToString(@"hh\:mm\:ss");
            }
            else if (valor is IEnumerable and not string)
            {
                worksheet.Cells[linha, coluna].Value = "-";
            }
            else
            {
                worksheet.Cells[linha, coluna].Value = valor;
            }
        }

        private static string FormatarValorCsv(object? valor, string? propertyName = null) => valor switch
        {
            null => "",
            DateTime data => BrasiliaTime.FormatForDisplay(data, propertyName),
            bool boolean => boolean ? "Sim" : "Não",
            TimeSpan ts => ts.ToString(@"hh\:mm\:ss"),
            IEnumerable and not string => "-",
            _ => valor.ToString() ?? ""
        };

        private static string EscaparCsv(string valorFormatado)
        {
            if (valorFormatado.Contains('"') || valorFormatado.Contains(';') || valorFormatado.Contains('\n'))
                return "\"" + valorFormatado.Replace("\"", "\"\"") + "\"";
            return valorFormatado;
        }

        private static string ObterTituloColuna(string nomePropriedade)
        {
            return nomePropriedade switch
            {
                "Nome" => "Nome",
                "Cpf" => "CPF",
                "MatriculaSindicato" => "Matrícula Sindicato",
                "MatriculaBancaria" => "Matrícula Bancária",
                "Sexo" => "Sexo",
                "EstadoCivil" => "Estado Civil",
                "DataNascimento" => "Data Nascimento",
                "DataAdmissao" => "Data Admissão",
                "DataFiliacao" => "Data Filiação",
                "DataDesligamento" => "Data Desligamento",
                "Celular" => "Celular",
                "Telefone" => "Telefone",
                "Email" => "E-mail",
                "Endereco" => "Endereço",
                "Bairro" => "Bairro",
                "Cidade" => "Cidade",
                "Estado" => "Estado",
                "Funcao" => "Função",
                "NomeBanco" => "Banco",
                "TotalEleicoesDisponiveis" => "Enquetes Disponíveis",
                "TotalVotosRealizados" => "Votos Realizados",
                "PercentualParticipacao" => "Participação (%)",
                "UltimaVotacao" => "Data/Hora Votação",
                "Titulo" => "Título",
                "DataInicio" => "Data Início",
                "DataFim" => "Data Fim",
                "Status" => "Status",
                "TotalVotos" => "Total Votos",
                "Vencedor" => "Opção Mais Votada",
                "Agencia" => "Agência",
                "CidadeAgencia" => "Cidade Agência",
                "CodAgencia" => "Cód. Agência",
                "Conta" => "Conta",
                "Ativo" => "Ativo",
                "Aposentado" => "Aposentado",
                "Idade" => "Idade",
                _ => nomePropriedade
            };
        }
    }
}