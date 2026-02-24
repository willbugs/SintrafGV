using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // Configurar licença do EPPlus uma única vez
            if (ExcelPackage.LicenseContext == LicenseContext.Commercial)
            {
#pragma warning disable CS0618
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
            }
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
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Fontes
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Título
            document.Add(new Paragraph(dados.Metadata.Titulo)
                .SetFont(boldFont).SetFontSize(18).SetTextAlignment(TextAlignment.CENTER));

            // Subtítulo se existir
            if (!string.IsNullOrEmpty(dados.Metadata.Subtitulo))
            {
                document.Add(new Paragraph(dados.Metadata.Subtitulo)
                    .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER));
            }

            // Info
            document.Add(new Paragraph($"Gerado em: {dados.Metadata.DataGeracao:dd/MM/yyyy HH:mm} | Total: {dados.Metadata.TotalRegistros}")
                .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));

            // Tabela (máximo 6 colunas para caber na página)
            var propriedades = typeof(T).GetProperties().Take(6).ToList();
            var tabela = new Table(propriedades.Count).UseAllAvailableWidth();

            // Cabeçalhos
            foreach (var prop in propriedades)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(ObterTituloColuna(prop.Name)))
                    .SetFont(boldFont).SetFontSize(10)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER));
            }

            // Dados (máximo 100 linhas para performance)
            foreach (var registro in dados.Dados.Take(100))
            {
                foreach (var prop in propriedades)
                {
                    var valor = prop.GetValue(registro)?.ToString() ?? "";
                    tabela.AddCell(new Cell()
                        .Add(new Paragraph(valor))
                        .SetFont(font).SetFontSize(8));
                }
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
            var worksheet = package.Workbook.Worksheets.Add(dados.Metadata.Titulo);

            // Título
            worksheet.Cells[1, 1].Value = dados.Metadata.Titulo;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            var linha = 3;

            // Cabeçalhos
            var propriedades = typeof(T).GetProperties().ToList();
            var coluna = 1;
            foreach (var prop in propriedades)
            {
                worksheet.Cells[linha, coluna].Value = ObterTituloColuna(prop.Name);
                worksheet.Cells[linha, coluna].Style.Font.Bold = true;
                worksheet.Cells[linha, coluna].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[linha, coluna].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
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
                        else
                        {
                            worksheet.Cells[linha, coluna].Value = valor;
                        }
                    }
                    coluna++;
                }
                linha++;
            }

            // Autofit
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

            // Cabeçalhos das colunas
            var propriedades = typeof(T).GetProperties().ToList();
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
                        _ => valor.ToString()
                    };
                    
                    // Escapar caracteres especiais do CSV
                    if (valorFormatado.Contains("\"") || valorFormatado.Contains(";") || valorFormatado.Contains("\n"))
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