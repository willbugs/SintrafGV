using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            document.Add(new Paragraph(RemoverAcentos($"Gerado em: {dados.Metadata.DataGeracao:dd/MM/yyyy HH:mm} | Total: {dados.Metadata.TotalRegistros}"))
                .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));

            // Tabela (máximo 6 colunas para caber na página) - excluir listas e dicionários
            var propriedades = ObterPropriedadesSimples<T>().Take(6).ToList();
            if (propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).Take(6).ToList();
            var tabela = new Table(Math.Max(1, propriedades.Count)).UseAllAvailableWidth();

            // Cabeçalhos
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

            // Dados (máximo 100 linhas para performance)
            foreach (var registro in dados.Dados.Take(100))
            {
                foreach (var prop in propriedades)
                {
                    var valor = FormatarValorParaPdf(prop.GetValue(registro));
                    tabela.AddCell(new Cell().Add(new Paragraph(valor)).SetFont(font).SetFontSize(8));
                }
                if (propriedades.Count == 0)
                    tabela.AddCell(new Cell().Add(new Paragraph("-")).SetFont(font).SetFontSize(8));
            }

            document.Add(tabela);
            document.Close();

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.pdf",
                Formato = "pdf",
                Conteudo = memoryStream.ToArray(),
                ContentType = "application/pdf",
                TamanhoBytes = memoryStream.Length,
                DataGeracao = DateTime.Now
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

            // Cabeçalhos - excluir listas e dicionários
            var propriedades = ObterPropriedadesSimples<T>().ToList();
            if (propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();
            var coluna = 1;
            foreach (var prop in propriedades)
            {
                worksheet.Cells[linha, coluna].Value = ObterTituloColuna(prop.Name);
                worksheet.Cells[linha, coluna].Style.Font.Bold = true;
                coluna++;
            }

            // Dados
            linha++;
            foreach (var registro in dados.Dados)
            {
                coluna = 1;
                foreach (var prop in propriedades)
                {
                    var valor = prop.GetValue(registro);
                    if (valor != null)
                    {
                        if (valor is DateTime data)
                        {
                            worksheet.Cells[linha, coluna].Value = data;
                            worksheet.Cells[linha, coluna].Style.Numberformat.Format = "dd/mm/yyyy";
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
                    coluna++;
                }
                linha++;
            }

            // Autofit (Dimension pode ser null se planilha vazia)
            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var conteudo = package.GetAsByteArray();

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.xlsx",
                Formato = "xlsx",
                Conteudo = conteudo,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TamanhoBytes = conteudo.Length,
                DataGeracao = DateTime.Now
            };
        }

        private ExportacaoRelatorioDto GerarCsv<T>(RelatorioResponse<T> dados, string nomeArquivo)
        {
            var csv = new StringBuilder();

            // Cabeçalho informativo
            csv.AppendLine($"# {dados.Metadata.Titulo}");
            csv.AppendLine($"# Gerado em: {dados.Metadata.DataGeracao:dd/MM/yyyy HH:mm}");
            csv.AppendLine($"# Total de registros: {dados.Metadata.TotalRegistros}");
            csv.AppendLine(); // Linha em branco

            // Cabeçalhos das colunas - excluir listas e dicionários
            var propriedades = ObterPropriedadesSimples<T>().ToList();
            if (propriedades.Count == 0)
                propriedades = typeof(T).GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).ToList();
            var cabecalhos = propriedades.Select(p => ObterTituloColuna(p.Name));
            csv.AppendLine(string.Join(";", cabecalhos));

            // Dados
            foreach (var registro in dados.Dados)
            {
                var valores = new List<string>();
                foreach (var prop in propriedades)
                {
                    var valor = prop.GetValue(registro);
                    var valorFormatado = valor switch
                    {
                        null => "",
                        DateTime data => data.ToString("dd/MM/yyyy"),
                        bool boolean => boolean ? "Sim" : "Não",
                        TimeSpan ts => ts.ToString(@"hh\:mm\:ss"),
                        IEnumerable and not string => "-",
                        _ => valor.ToString()
                    };
                    
                    // Escapar caracteres especiais do CSV
                    if (valorFormatado!.Contains("\"") || valorFormatado.Contains(";") || valorFormatado.Contains("\n"))
                    {
                        valorFormatado = "\"" + valorFormatado.Replace("\"", "\"\"") + "\"";
                    }
                    
                    valores.Add(valorFormatado);
                }
                csv.AppendLine(string.Join(";", valores));
            }

            var conteudo = Encoding.UTF8.GetBytes(csv.ToString());

            return new ExportacaoRelatorioDto
            {
                NomeArquivo = $"{nomeArquivo}.csv",
                Formato = "csv",
                Conteudo = conteudo,
                ContentType = "text/csv; charset=utf-8",
                TamanhoBytes = conteudo.Length,
                DataGeracao = DateTime.Now
            };
        }

        private static List<PropertyInfo> ObterPropriedadesSimples<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
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

        private static string FormatarValorParaPdf(object? valor)
        {
            if (valor == null) return "";
            if (valor is IEnumerable and not string) return "-";
            return RemoverAcentos(valor.ToString());
        }

        private static string SanitizarNomePlanilha(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return "Relatorio";
            var invalidos = new[] { '\\', '/', '*', '?', ':', '[', ']' };
            var sanitizado = new string(titulo.Where(c => !invalidos.Contains(c)).ToArray()).Trim();
            return sanitizado.Length > 31 ? sanitizado[..31] : sanitizado;
        }

        private string ObterTituloColuna(string nomePropriedade)
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
                "Ativo" => "Ativo",
                "Aposentado" => "Aposentado",
                "Idade" => "Idade",
                _ => nomePropriedade
            };
        }
    }
}