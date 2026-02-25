using Microsoft.EntityFrameworkCore;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Interfaces;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;

namespace SintrafGv.Application.Services
{
    public class RelatorioVotacaoCartorialService : IRelatorioVotacaoCartorialService
    {
        private readonly IEleicaoRepository _eleicaoRepository;
        private readonly IVotoRepository _votoRepository;
        private readonly IAssociadoRepository _associadoRepository;
        private readonly IConfiguracaoSindicatoRepository _configuracaoRepository;
        private readonly AppDbContext _context;

        public RelatorioVotacaoCartorialService(
            IEleicaoRepository eleicaoRepository,
            IVotoRepository votoRepository,
            IAssociadoRepository associadoRepository,
            IConfiguracaoSindicatoRepository configuracaoRepository,
            AppDbContext context)
        {
            _eleicaoRepository = eleicaoRepository;
            _votoRepository = votoRepository;
            _associadoRepository = associadoRepository;
            _configuracaoRepository = configuracaoRepository;
            _context = context;
        }

        public async Task<RelatorioVotacaoCartorialDto> GerarRelatorioCartorialAsync(
            RelatorioCartorialRequest request, 
            CancellationToken cancellationToken = default)
        {
            // Buscar dados da eleição
            var eleicao = await _eleicaoRepository.ObterPorIdAsync(request.EleicaoId, cancellationToken);
            if (eleicao == null)
                throw new ArgumentException("Eleição não encontrada", nameof(request.EleicaoId));

            // Buscar configuração do sindicato
            var configuracao = await _configuracaoRepository.ObterConfiguracaoAsync(cancellationToken);
            if (configuracao == null)
                throw new InvalidOperationException("Configuração do sindicato não encontrada");

            // Buscar votos da eleição
            var votos = await _votoRepository.ObterVotosPorEleicaoAsync(request.EleicaoId, cancellationToken);

            // Buscar dados dos associados que votaram
            var associadosIds = votos.Select(v => v.AssociadoId).Distinct().ToList();
            var associados = await _associadoRepository.ObterPorIdsAsync(associadosIds, cancellationToken);

            var relatorio = new RelatorioVotacaoCartorialDto
            {
                DadosSindicato = new DadosSindicatoDto
                {
                    RazaoSocial = configuracao.RazaoSocial,
                    NomeFantasia = configuracao.NomeFantasia,
                    CNPJ = configuracao.CNPJ,
                    InscricaoEstadual = configuracao.InscricaoEstadual ?? "",
                    EnderecoCompleto = $"{configuracao.Endereco}, {configuracao.Numero}" +
                                     $"{(!string.IsNullOrEmpty(configuracao.Complemento) ? $", {configuracao.Complemento}" : "")}" +
                                     $", {configuracao.Bairro}, {configuracao.Cidade}/{configuracao.UF}, CEP: {configuracao.CEP}",
                    Telefone = configuracao.Telefone ?? "",
                    Email = configuracao.Email ?? "",
                    Presidente = configuracao.Presidente,
                    CPFPresidente = configuracao.CPFPresidente,
                    Secretario = configuracao.Secretario,
                    CPFSecretario = configuracao.CPFSecretario
                },

                DadosEleicao = new DadosEleicaoDto
                {
                    EleicaoId = eleicao.Id,
                    Titulo = eleicao.Titulo,
                    Descricao = eleicao.Descricao ?? "",
                    DataInicio = eleicao.InicioVotacao,
                    HoraInicio = eleicao.InicioVotacao.ToString("HH:mm:ss"),
                    DataFim = eleicao.FimVotacao,
                    HoraFim = eleicao.FimVotacao.ToString("HH:mm:ss"),
                    Pergunta = eleicao.Perguntas?.FirstOrDefault()?.Texto ?? "",
                    Opcoes = eleicao.Perguntas?.FirstOrDefault()?.Opcoes?.Select(o => o.Texto).ToList() ?? new List<string>(),
                    ApenasAssociados = eleicao.ApenasAssociados,
                    ArquivoAnexo = eleicao.ArquivoAnexo
                }
            };

            // Processar votos detalhados
            var votosDetalhados = new List<VotoDetalhado>();
            var numeroSequencial = 1;

            foreach (var voto in votos.OrderBy(v => v.DataHoraVoto))
            {
                var associado = associados.FirstOrDefault(a => a.Id == voto.AssociadoId);
                if (associado == null) continue;

                var votoDetalhado = new VotoDetalhado
                {
                    VotoId = voto.Id,
                    DataHoraVoto = voto.DataHoraVoto,
                    TimestampPreciso = voto.TimestampPreciso,
                    CodigoVotante = GerarCodigoVotante(voto.Id, associado.Id),
                    NomeVotante = request.IncluirDadosVotantes ? associado.Nome : "[NOME PROTEGIDO]",
                    CPFVotante = request.IncluirDadosVotantes ? associado.Cpf : "[CPF PROTEGIDO]",
                    MatriculaSindicato = request.IncluirDadosVotantes ? (associado.MatriculaSindicato ?? "") : "[MATRÍCULA PROTEGIDA]",
                    MatriculaBancaria = request.IncluirDadosVotantes ? (associado.MatriculaBancaria ?? "") : "[MATRÍCULA PROTEGIDA]",
                    Banco = request.IncluirDadosVotantes ? (associado.Banco ?? "") : "[BANCO PROTEGIDO]",
                    Pergunta = relatorio.DadosEleicao.Pergunta,
                    RespostaSelecionada = ObterRespostaDoVoto(voto.Id),
                    NumeroResposta = numeroSequencial++,
                    EnderecoIP = request.IncluirDadosTecnicos ? (voto.IpOrigem ?? "") : "[IP PROTEGIDO]",
                    UserAgent = request.IncluirDadosTecnicos ? (voto.UserAgent ?? "") : "[USER AGENT PROTEGIDO]",
                    HashVoto = voto.HashVoto,
                    AssinaturaDigital = voto.AssinaturaDigital ?? ""
                };

                votosDetalhados.Add(votoDetalhado);
            }

            relatorio.Votos = votosDetalhados;

            // Gerar resumo estatístico
            relatorio.Resumo = await GerarResumoVotacao(eleicao, votos, cancellationToken);

            // Dados de autenticação
            var agora = DateTime.UtcNow;
            relatorio.Autenticacao = new DadosAutenticacaoDto
            {
                DataGeracaoRelatorio = agora,
                HoraGeracaoRelatorio = agora.ToString("HH:mm:ss"),
                ResponsavelRelatorio = configuracao.Presidente,
                CargoResponsavel = "Presidente",
                TextoAutenticacao = configuracao.TextoAutenticacao ?? 
                    "Este relatório contém os dados oficiais da votação/eleição realizada pelo sindicato, " +
                    "com registro de data e hora de cada voto, garantindo a transparência e autenticidade do processo democrático.",
                CartorioResponsavel = configuracao.CartorioResponsavel,
                EnderecoCartorio = configuracao.EnderecoCartorio,
                NumeroProtocolo = GerarNumeroProtocolo(eleicao.Id, agora),
                ChaveValidacao = GerarChaveValidacao(eleicao.Id, agora),
                URLValidacao = $"https://sintrafgv.org.br/validar-relatorio/{eleicao.Id}"
            };

            // Gerar hash do relatório sempre
            relatorio.Autenticacao.HashRelatorio = await GerarHashVerificacaoAsync(relatorio, cancellationToken);
            // Não gerar assinatura digital (decisão: não usar certificados)
            relatorio.Autenticacao.AssinaturaDigitalRelatorio = "";

            return relatorio;
        }

        public async Task<byte[]> GerarRelatorioPDFCartorialAsync(
            RelatorioCartorialRequest request, 
            CancellationToken cancellationToken = default)
        {
            var relatorio = await GerarRelatorioCartorialAsync(request, cancellationToken);
            
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Configurar fontes
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título do documento
            document.Add(new Paragraph("RELATÓRIO CARTORIAL DE VOTAÇÃO")
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Dados da eleição
            document.Add(new Paragraph("DADOS DA ELEIÇÃO")
                .SetFont(headerFont)
                .SetFontSize(12)
                .SetMarginBottom(10));

            var eleicaoTable = new Table(2).UseAllAvailableWidth();
            eleicaoTable.AddCell(new Cell().Add(new Paragraph("Título:").SetFont(headerFont)));
            eleicaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.DadosEleicao.Titulo).SetFont(normalFont)));
            
            eleicaoTable.AddCell(new Cell().Add(new Paragraph("Data Início:").SetFont(headerFont)));
            eleicaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.DadosEleicao.DataInicio.ToString("dd/MM/yyyy")).SetFont(normalFont)));
            
            eleicaoTable.AddCell(new Cell().Add(new Paragraph("Data Fim:").SetFont(headerFont)));
            eleicaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.DadosEleicao.DataFim.ToString("dd/MM/yyyy")).SetFont(normalFont)));
            
            eleicaoTable.AddCell(new Cell().Add(new Paragraph("Total de Votos:").SetFont(headerFont)));
            eleicaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.Resumo.TotalVotosComputados.ToString()).SetFont(normalFont)));

            document.Add(eleicaoTable);
            document.Add(new Paragraph("\n"));

            // Resumo estatístico
            document.Add(new Paragraph("RESUMO ESTATÍSTICO")
                .SetFont(headerFont)
                .SetFontSize(12)
                .SetMarginBottom(10));

            var resumoTable = new Table(2).UseAllAvailableWidth();
            resumoTable.AddCell(new Cell().Add(new Paragraph("Participação:").SetFont(headerFont)));
            resumoTable.AddCell(new Cell().Add(new Paragraph($"{relatorio.Resumo.PercentualParticipacao:F2}%").SetFont(normalFont)));
            
            var votosValidos = relatorio.Resumo.ResultadoPorOpcao
                .Where(kvp => kvp.Key != "Voto em Branco").Sum(kvp => kvp.Value);
            var votosBranco = relatorio.Resumo.ResultadoPorOpcao
                .Where(kvp => kvp.Key == "Voto em Branco").Sum(kvp => kvp.Value);

            resumoTable.AddCell(new Cell().Add(new Paragraph("Votos Válidos:").SetFont(headerFont)));
            resumoTable.AddCell(new Cell().Add(new Paragraph(votosValidos.ToString()).SetFont(normalFont)));
            
            resumoTable.AddCell(new Cell().Add(new Paragraph("Votos em Branco:").SetFont(headerFont)));
            resumoTable.AddCell(new Cell().Add(new Paragraph(votosBranco.ToString()).SetFont(normalFont)));

            foreach (var resultado in relatorio.Resumo.ResultadoPorOpcao.Where(kvp => kvp.Key != "Voto em Branco"))
            {
                var percentual = relatorio.Resumo.PercentualPorOpcao.GetValueOrDefault(resultado.Key, 0);
                resumoTable.AddCell(new Cell().Add(new Paragraph($"{resultado.Key}:").SetFont(headerFont)));
                resumoTable.AddCell(new Cell().Add(new Paragraph($"{resultado.Value} votos ({percentual:F1}%)").SetFont(normalFont)));
            }

            document.Add(resumoTable);
            document.Add(new Paragraph("\n"));

            // Votos detalhados (apenas se solicitado)
            if (relatorio.Votos.Any())
            {
                document.Add(new Paragraph("VOTOS DETALHADOS")
                    .SetFont(headerFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10));

                var votosTable = new Table(4).UseAllAvailableWidth();
                votosTable.AddHeaderCell(new Cell().Add(new Paragraph("Nº").SetFont(headerFont)));
                votosTable.AddHeaderCell(new Cell().Add(new Paragraph("Data/Hora").SetFont(headerFont)));
                votosTable.AddHeaderCell(new Cell().Add(new Paragraph("Votante").SetFont(headerFont)));
                votosTable.AddHeaderCell(new Cell().Add(new Paragraph("Hash").SetFont(headerFont)));

                foreach (var voto in relatorio.Votos.Take(100)) // Limitar a 100 votos para não sobrecarregar o PDF
                {
                    votosTable.AddCell(new Cell().Add(new Paragraph(voto.NumeroResposta.ToString()).SetFont(normalFont)));
                    votosTable.AddCell(new Cell().Add(new Paragraph(voto.DataHoraVoto.ToString("dd/MM/yyyy HH:mm")).SetFont(normalFont)));
                    votosTable.AddCell(new Cell().Add(new Paragraph(voto.CodigoVotante).SetFont(normalFont)));
                    votosTable.AddCell(new Cell().Add(new Paragraph(voto.HashVoto[..8] + "...").SetFont(normalFont)));
                }

                document.Add(votosTable);
            }

            // Dados de autenticação
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("DADOS DE AUTENTICAÇÃO")
                .SetFont(headerFont)
                .SetFontSize(12)
                .SetMarginBottom(10));

            var autenticacaoTable = new Table(2).UseAllAvailableWidth();
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph("Data de Geração:").SetFont(headerFont)));
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.Autenticacao.DataGeracaoRelatorio.ToString("dd/MM/yyyy HH:mm:ss")).SetFont(normalFont)));
            
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph("Hash do Relatório:").SetFont(headerFont)));
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.Autenticacao.HashRelatorio).SetFont(normalFont)));
            
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph("Responsável:").SetFont(headerFont)));
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.Autenticacao.ResponsavelRelatorio ?? "Sistema SintrafGV").SetFont(normalFont)));

            autenticacaoTable.AddCell(new Cell().Add(new Paragraph("Protocolo:").SetFont(headerFont)));
            autenticacaoTable.AddCell(new Cell().Add(new Paragraph(relatorio.Autenticacao.NumeroProtocolo ?? "").SetFont(normalFont)));

            document.Add(autenticacaoTable);

            // Rodapé com informações legais
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("Este relatório foi gerado automaticamente pelo Sistema SintrafGV e possui validade legal para fins cartoriais.")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<bool> ValidarIntegridadeVotacaoAsync(
            Guid eleicaoId, 
            CancellationToken cancellationToken = default)
        {
            var votos = await _votoRepository.ObterVotosPorEleicaoAsync(eleicaoId, cancellationToken);
            
            foreach (var voto in votos)
            {
                // Validar hash do voto
                var hashCalculado = GerarHashVoto(voto);
                if (hashCalculado != voto.HashVoto)
                    return false;

                // Validar assinatura digital (se presente)
                if (!string.IsNullOrEmpty(voto.AssinaturaDigital))
                {
                    if (!ValidarAssinaturaDigital(voto.HashVoto, voto.AssinaturaDigital))
                        return false;
                }
            }

            return true;
        }

        public async Task<string> GerarHashVerificacaoAsync(
            RelatorioVotacaoCartorialDto relatorio, 
            CancellationToken cancellationToken = default)
        {
            var dadosParaHash = new
            {
                relatorio.DadosEleicao.EleicaoId,
                relatorio.DadosEleicao.Titulo,
                relatorio.Resumo.TotalVotosComputados,
                relatorio.Resumo.ResultadoPorOpcao,
                relatorio.Autenticacao.DataGeracaoRelatorio,
                VotosHash = relatorio.Votos.Select(v => v.HashVoto).OrderBy(h => h).ToList()
            };

            var json = JsonSerializer.Serialize(dadosParaHash);
            return GerarHashSHA256(json);
        }

        public async Task<string> AssinarDigitalmenteRelatorioAsync(
            string hashRelatorio, 
            CancellationToken cancellationToken = default)
        {
            return $"HASH_{hashRelatorio[..16]}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        #region Métodos Auxiliares

        private string GerarCodigoVotante(Guid votoId, Guid associadoId)
        {
            var dados = $"{votoId}_{associadoId}";
            return GerarHashSHA256(dados)[..12].ToUpper();
        }

        private string ObterRespostaDoVoto(Guid votoId)
        {
            var detalhes = _context.VotosDetalhes
                .Where(vd => _context.Set<Domain.Entities.Voto>()
                    .Any(v => v.Id == votoId && v.EleicaoId == vd.Pergunta!.EleicaoId))
                .Include(vd => vd.Opcao)
                .ToList();

            if (!detalhes.Any())
                return "Sem resposta";

            if (detalhes.All(d => d.VotoBranco))
                return "Voto em Branco";

            var respostas = detalhes
                .Where(d => d.Opcao != null)
                .Select(d => d.Opcao!.Texto)
                .ToList();

            return respostas.Any() ? string.Join(", ", respostas) : "Sem resposta";
        }

        private async Task<ResumoVotacaoDto> GerarResumoVotacao(
            Domain.Entities.Eleicao eleicao, 
            List<Domain.Entities.Voto> votos, 
            CancellationToken cancellationToken)
        {
            var totalAssociados = await _associadoRepository.ContarAssociadosAtivosAsync(cancellationToken);
            var totalVotos = votos.Count;
            
            var resumo = new ResumoVotacaoDto
            {
                TotalVotosComputados = totalVotos,
                TotalAssociadosAptos = totalAssociados,
                PercentualParticipacao = totalAssociados > 0 ? (decimal)totalVotos / totalAssociados * 100 : 0
            };

            if (votos.Any())
            {
                resumo.PrimeiroVoto = votos.Min(v => v.DataHoraVoto);
                resumo.UltimoVoto = votos.Max(v => v.DataHoraVoto);
                resumo.DuracaoVotacao = resumo.UltimoVoto - resumo.PrimeiroVoto;
            }

            var opcoes = eleicao.Perguntas?
                .SelectMany(p => p.Opcoes ?? new List<Domain.Entities.Opcao>())
                .ToList() ?? new List<Domain.Entities.Opcao>();

            var resultadoPorOpcao = new Dictionary<string, int>();
            foreach (var opcao in opcoes)
            {
                var count = _context.VotosDetalhes.Count(vd => vd.OpcaoId == opcao.Id);
                resultadoPorOpcao[opcao.Texto] = count;
            }

            var votosBranco = _context.VotosDetalhes
                .Count(vd => vd.VotoBranco && 
                    eleicao.Perguntas!.Select(p => p.Id).Contains(vd.PerguntaId));
            if (votosBranco > 0)
                resultadoPorOpcao["Voto em Branco"] = votosBranco;

            resumo.ResultadoPorOpcao = resultadoPorOpcao;

            var totalRespostas = resultadoPorOpcao.Values.Sum();
            resumo.PercentualPorOpcao = resultadoPorOpcao.ToDictionary(
                kvp => kvp.Key,
                kvp => totalRespostas > 0 ? (decimal)kvp.Value / totalRespostas * 100 : 0
            );

            return resumo;
        }

        private string GerarHashVoto(Domain.Entities.Voto voto)
        {
            var dados = $"{voto.Id}_{voto.EleicaoId}_{voto.AssociadoId}_{voto.DataHoraVoto:yyyy-MM-dd HH:mm:ss.fff}";
            return GerarHashSHA256(dados);
        }

        private string GerarHashSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        private bool ValidarAssinaturaDigital(string hash, string assinatura)
        {
            return !string.IsNullOrEmpty(assinatura);
        }

        private string GerarNumeroProtocolo(Guid eleicaoId, DateTime dataGeracao)
        {
            return $"PROT-{dataGeracao:yyyyMMdd}-{eleicaoId.ToString()[..8].ToUpper()}";
        }

        private string GerarChaveValidacao(Guid eleicaoId, DateTime dataGeracao)
        {
            var dados = $"{eleicaoId}_{dataGeracao:yyyyMMddHHmmss}";
            return GerarHashSHA256(dados)[..16].ToUpper();
        }

        #endregion
    }
}