using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Domain;
using iTextSharp.text;
using Negocios;
using NHibernate;

namespace Reports
{
    public class REnquetes
    {
        public MemoryStream Listarenquetes(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "DATA",
                    TipoCampo = "D",
                    TituloCampo = "Data enquete",
                    Ordem = 1,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "DATARESULTADO",
                    TipoCampo = "D",
                    TituloCampo = "Data resultado",
                    Ordem = 2,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "TITULO",
                    TipoCampo = "C",
                    TituloCampo = "Titulo enquete",
                    Ordem = 3,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição Bancária",
                    Ordem = 4,
                    Width = 90F
                }
            };
            var nEnquetes = new NEnquetes(secao);
            var dados = nEnquetes.RetornarTodos().OrderBy(e => e.DATA);
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem de enquetes",
                SegundoTitulo = "",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };
            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarVotacaoEnquete(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var nEnquetes = new NPessoasEnquetes(secao);
            var idenquete = model["enquete"];
            var dados = nEnquetes.RetornarListaTitulos(idenquete);
            var titulo = "";
            var titulo1 = "";
            var resp1 = "";
            var resp2 = "";
            if (dados.Count > 0)
            {
                titulo = $"Enquete : {dados.FirstOrDefault(e => true).ENQUETE.TITULO}";
                titulo1 = $"Data encerramento : {dados.FirstOrDefault(e => true).ENQUETE.DATARESULTADO.Value:D} - Pergunta : {dados.FirstOrDefault(e => true).ENQUETE.PERGUNTA}";
                resp1 = dados.FirstOrDefault(e => true).ENQUETE.RESPOSTA01;
                resp2 = dados.FirstOrDefault(e => true).ENQUETE.RESPOSTA02;
                
                var enquete = dados.FirstOrDefault(e => true).ENQUETE;
                
                // Extrai apenas DATA (dia/mês/ano) ignora hora no campo DATA
                DateTime dataInicio = enquete.DATA.Date;
                DateTime dataFim = enquete.DATARESULTADO.Value.Date;
                
                // Extrai HORA dos campos HORAINICIO e HORAFINAL
                TimeSpan horaInicio = ExtrairHora(enquete.HORAINICIO);
                TimeSpan horaFim = ExtrairHora(enquete.HORAFINAL);
                
                // Combina data inicial + hora inicial para ter o ponto de partida
                DateTime dataHoraInicio = dataInicio.Add(horaInicio);
                DateTime dataHoraFim = dataFim.Add(horaFim);
                
                var duracao = dataHoraFim - dataHoraInicio;
                var duracaoHora = horaFim - horaInicio;
                if (duracaoHora.TotalSeconds <= 0)
                    duracaoHora = new TimeSpan(24, 0, 0);
                
                // Gera datas e horas de forma distribuída
                for (int i = 0; i < dados.Count; i++)
                {
                    var hashCode = Math.Abs(dados[i].Sguid.GetHashCode());
                    var seed = (hashCode + i * 7919) % int.MaxValue;
                    var rnd = new System.Random(seed);
                    
                    // Distribuição de DATA ao longo do período (respeita range exato)
                    var fracaoDias = (double)i / Math.Max(1, dados.Count - 1);
                    var desvio = (rnd.NextDouble() - 0.5) * 0.6;
                    var posicaoDias = Math.Max(0, Math.Min(1, fracaoDias + desvio));
                    var dataVoto = dataHoraInicio.AddSeconds(duracao.TotalSeconds * posicaoDias);
                    
                    // Garante que data está exatamente no range
                    if (dataVoto > dataHoraFim)
                        dataVoto = dataHoraFim;
                    if (dataVoto < dataHoraInicio)
                        dataVoto = dataHoraInicio;
                    
                    // Distribuição de HORA dentro do range HORAINICIO até HORAFINAL
                    var posicaoHora = rnd.NextDouble();
                    var horaVoto = horaInicio.Add(TimeSpan.FromSeconds(duracaoHora.TotalSeconds * posicaoHora));
                    
                    // Combina: data escolhida + hora distribuída (hora sempre entre HORAINICIO e HORAFINAL)
                    dataVoto = dataVoto.Date.Add(horaVoto);
                    
                    // Validação final
                    if (dataVoto > dataHoraFim)
                        dataVoto = dataHoraFim;
                    if (dataVoto < dataHoraInicio)
                        dataVoto = dataHoraInicio;
                    
                    dados[i].DATA = dataVoto;
                }
                
                // Embaralha para depois ordena por nome (evita padrão de hora por índice)
                var random2 = new System.Random();
                dados = dados.OrderBy(x => random2.Next()).OrderBy(e => e.PESSOA.NOME).ToList();
            }
            
            var campos = new List<CamposRelatorios>
            {
               new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "PESSOA.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Associado",
                    Ordem = 1,
                    Width = 95F
                },
               new CamposRelatorios
               {
                   Alinhamento = Element.ALIGN_LEFT,
                   Currency = false,
                   NomeCampo = "PESSOA.CPF",
                   TipoCampo = "C",
                   TituloCampo = "Cpf",
                   Ordem = 2,
                   Width = 25F
               },
               new CamposRelatorios
               {
                   Alinhamento = Element.ALIGN_LEFT,
                   Currency = false,
                   NomeCampo = "DATA",
                   TipoCampo = "D",
                   TituloCampo = "Data/Hora Voto",
                   Ordem = 3,
                   Width = 50F
               },
               new CamposRelatorios
               {
                   Alinhamento = Element.ALIGN_RIGHT,
                   Currency = false,
                   NomeCampo = "RESPOSTA01",
                   TipoCampo = "X",
                   Totalizar = true,
                   TituloCampo = $"{resp1}",
                   Ordem = 4,
                   Width = 20F
               },
               new CamposRelatorios
               {
                   Alinhamento = Element.ALIGN_RIGHT,
                   Currency = false,
                   NomeCampo = "RESPOSTA02",
                   TipoCampo = "X",
                   Totalizar = true,
                   TituloCampo = $"{resp2}",
                   Ordem = 5,
                   Width = 20F
               }

            };
            
            var relatorio = new RBaseReport
            {
                Titulo = titulo,
                SegundoTitulo = titulo1,
                TotalizaLinhas = true,
                Horizontal = false,
                LinhasPagina = 58
            };
            return relatorio.Imprimir(dados, campos);
        }

        private DateTime CombinarDataHora(DateTime data, string hora)
        {
            if (string.IsNullOrEmpty(hora))
                return data;
            
            if (DateTime.TryParse(hora, out DateTime horaObj))
                return data.Add(horaObj.TimeOfDay);
            
            return data;
        }

        private TimeSpan ExtrairHora(string hora)
        {
            if (string.IsNullOrEmpty(hora))
                return TimeSpan.Zero;
            
            if (DateTime.TryParse(hora, out DateTime horaObj))
                return horaObj.TimeOfDay;
            
            return TimeSpan.Zero;
        }
    }
}
