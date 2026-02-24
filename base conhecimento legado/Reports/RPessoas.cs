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
    public class RPessoas
    {
        public MemoryStream ListarGeral(ISession secao, List<RelatorioAssociados> model, string periodo)
        {
            var ordem = 1;
            var campos = (from campo in model
                          where campo.Visivel
                          select new CamposRelatorios
                          {
                              Totalizar = campo.Totalizar,
                              TituloCampo = campo.TituloCampo,
                              NomeCampo = campo.NomeCampo,
                              Alinhamento = campo.Alinhamento,
                              Currency = campo.Currency,
                              Ordem = ordem++,
                              TipoCampo = campo.TipoCampo,
                              Width = campo.Width
                          }).ToList();

            var nPessoas = new NPessoas(secao);
            var dados = nPessoas.RetornarTodos();
            var titulo = "Listagem associado";
            var segundotitulo = "";

            if (!string.IsNullOrWhiteSpace(periodo))
            {
                titulo = "Listagem de aniversariantes";
                segundotitulo = $"Mês {periodo}";
                dados = dados.Where(e => e.DATNASCIMENTO != null && e.DATNASCIMENTO.Value.Month.Equals(int.Parse(periodo))).ToList();
            }

            foreach (var campo in model.Where(campo => campo.Visivel && !string.IsNullOrEmpty(campo.Filtro.ToUpper())))
            {
                if (campo.NomeCampo.Equals("NOME") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.NOME.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CPF") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CPF.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("MATRICULABANCARIA") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.MATRICULABANCARIA.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("MATRICULASINDICATO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.MATRICULASINDICATO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CELULAR") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CELULAR.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CARTEIRINHA") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CARTEIRINHA.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("ESTADOCIVIL") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.ESTADOCIVIL.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("ENDERECO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.ENDERECO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("BAIRRO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.BAIRRO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CIDADE") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CIDADE.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("ESTADO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.ESTADO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("NATURALIDADE") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.NATURALIDADE.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CODAGENCIA") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CODAGENCIA.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("AGENCIA") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.AGENCIA.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CONTA") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CONTA.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("FUNCAO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.FUNCAO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("CTPS") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.CTPS.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("SERIE") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.SERIE.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("MOTIVO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.MOTIVO.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("TELEFONE") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.TELEFONE.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("EMAIL") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.EMAIL.Contains(campo.Filtro.ToUpper())).ToList();
                if (campo.NomeCampo.Equals("DATFILIACAO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.DATFILIACAO.Value.ToString("d").Equals(DateTime.Parse(campo.Filtro.ToUpper()).Date.ToString("d"))).ToList();
                if (campo.NomeCampo.Equals("DATNASCIMENTO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.DATNASCIMENTO.Value.ToString("d").Equals(DateTime.Parse(campo.Filtro.ToUpper()).Date.ToString("d"))).ToList();
                if (campo.NomeCampo.Equals("DATADMISSAO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.DATADMISSAO.Value.ToString("d").Equals(DateTime.Parse(campo.Filtro.ToUpper()).Date.ToString("d"))).ToList();
                if (campo.NomeCampo.Equals("DATDESLIGAMENTO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.DATDESLIGAMENTO.Value.ToString("d").Equals(DateTime.Parse(campo.Filtro.ToUpper()).Date.ToString("d"))).ToList();
                if (campo.NomeCampo.Equals("ATIVO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper()))
                {
                    if (!campo.Filtro.Equals("TODOS"))
                    {
                        dados = dados.Where(e => e.ATIVO.Equals(bool.Parse(campo.Filtro.ToUpper()))).ToList();
                    }
                }
                if (campo.NomeCampo.Equals("ASSOCIADO") && !string.IsNullOrEmpty(campo.Filtro.ToUpper()))
                {
                    if (!campo.Filtro.Equals("TODOS"))
                    {
                        dados = dados.Where(e => e.ASSOCIADO.Equals(bool.Parse(campo.Filtro.ToUpper()))).ToList();
                    }
                }
                if (campo.NomeCampo.Equals("BANCO.NOME") && !string.IsNullOrEmpty(campo.Filtro.ToUpper())) dados = dados.Where(e => e.BANCO != null && e.BANCO.Sguid.Equals(campo.Filtro.ToUpper())).ToList();

            }

            var relatorio = new RBaseReport
            {
                Titulo = titulo,
                SegundoTitulo = segundotitulo,
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38,
                TotalAtivos = dados.Count(e => e.ATIVO.Equals(true)),
                TotalInativos = dados.Count(e => e.ATIVO.Equals(false)),
                TotalAssociado = dados.Count(e => e.ASSOCIADO.Equals(true)),
                TotalNAssociado = dados.Count(e => e.ASSOCIADO.Equals(false))
            };
            return relatorio.Imprimir(dados, campos);
        }
        public MemoryStream ListarAtivos(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATFILIACAO",
                    TipoCampo = "D",
                    TituloCampo = "D.filiação",
                    Ordem = 7,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CARTEIRINHA",
                    TipoCampo = "C",
                    TituloCampo = "Carteirinha",
                    Ordem = 8,
                    Width = 30F
                }
            };
            var nPessoas = new NPessoas(secao);
            var dados = nPessoas.RetornarAtivos();
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associado ativos",
                SegundoTitulo = "",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarAposentados(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATDESLIGAMENTO",
                    TipoCampo = "D",
                    TituloCampo = "D.desligado",
                    Ordem = 7,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MOTIVO",
                    TipoCampo = "C",
                    TituloCampo = "Motivo",
                    Ordem = 8,
                    Width = 35F
                }
            };
            var nPessoas = new NPessoas(secao);
            var dados = nPessoas.Retornaraposentados();
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associados aposentados",
                SegundoTitulo = "",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarOutraBase(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BASE",
                    TipoCampo = "C",
                    TituloCampo = "Outra base",
                    Ordem = 7,
                    Width = 60F
                }
            };
            var nPessoas = new NPessoas(secao);
            var dados = nPessoas.Retornaroutrabase();
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem outra base",
                SegundoTitulo = "",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarDemitidos(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATFILIACAO",
                    TipoCampo = "D",
                    TituloCampo = "D.filiação",
                    Ordem = 7,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATDESLIGAMENTO",
                    TipoCampo = "D",
                    TituloCampo = "D.desligamento",
                    Ordem = 8,
                    Width = 40F
                },
            };
            var nPessoas = new NPessoas(secao);
            var inicio = Convert.ToDateTime(model["inicio"]);
            var final = Convert.ToDateTime(model["final"]);
            var dados = nPessoas.RetornarDeminitosPeriodo(inicio, final);
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associado demitidos",
                SegundoTitulo = $"Período de {inicio:d} até {final:d}",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarSexo(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Instituição",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "SEXO",
                    TipoCampo = "D",
                    TituloCampo = "SEXO",
                    Ordem = 7,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CARTEIRINHA",
                    TipoCampo = "C",
                    TituloCampo = "Carteirinha",
                    Ordem = 8,
                    Width = 30F
                }
            };
            var nPessoas = new NPessoas(secao);
            var sexo = model["sexo"].ToUpper();
            var dados = nPessoas.Retornarsexo(sexo);
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associado por sexo",
                SegundoTitulo = $"Sexo: {sexo}",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarEntidade(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "FUNCAO",
                    TipoCampo = "C",
                    TituloCampo = "Função",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATFILIACAO",
                    TipoCampo = "D",
                    TituloCampo = "D.filiação",
                    Ordem = 7,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATDESLIGAMENTO",
                    TipoCampo = "D",
                    TituloCampo = "D.desligamento",
                    Ordem = 8,
                    Width = 40F
                },
            };
            var nPessoas = new NPessoas(secao);
            var nBancos = new NBancos(secao);
            var banco = nBancos.RetornarGuid(model["banco"]);
            var dados = nPessoas.RetornarInstituicao(banco);
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associado por banco",
                SegundoTitulo = $"Instituição: {banco.NOME}",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }

        public MemoryStream ListarPeriodo(ISession secao, CRelatorios cRelatorio, FormCollection model)
        {
            var campos = new List<CamposRelatorios>
            {
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "NOME",
                    TipoCampo = "C",
                    TituloCampo = "Nome",
                    Ordem = 1,
                    Width = 90F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "CPF",
                    Ordem = 2,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULABANCARIA",
                    TipoCampo = "C",
                    TituloCampo = "M.Funcional",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "MATRICULASINDICATO",
                    TipoCampo = "C",
                    TituloCampo = "M.Sindicato",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CELULAR",
                    TipoCampo = "C",
                    TituloCampo = "Celular",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "BANCO.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Função",
                    Ordem = 6,
                    Width = 60F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATFILIACAO",
                    TipoCampo = "D",
                    TituloCampo = "D.filiação",
                    Ordem = 7,
                    Width = 40F
                }
            };
            var nPessoas = new NPessoas(secao);
            var inicio = Convert.ToDateTime(model["inicio"]);
            var final = Convert.ToDateTime(model["final"]);
            var ativos = Convert.ToBoolean(model["ativo"]);
            if (!ativos)
            {
                campos.Add(
                    new CamposRelatorios
                    {
                        Alinhamento = Element.ALIGN_CENTER,
                        Currency = false,
                        NomeCampo = "DATDESLIGAMENTO",
                        TipoCampo = "D",
                        TituloCampo = "D.desligamento",
                        Ordem = 8,
                        Width = 40F
                    }
                );
            }
            var dados = nPessoas.RetornarPeriodoFiliacao(inicio, final);
            var situacao = ativos ? "Ativos" : "Inativos";
            var relatorio = new RBaseReport
            {
                Titulo = "Listagem associado período filiação",
                SegundoTitulo = $"Período de {inicio:d} até {final:d} - {situacao}",
                TotalizaLinhas = true,
                Horizontal = true, // 38 - off - 58
                LinhasPagina = 38
            };

            return relatorio.Imprimir(dados, campos);
        }
    }
}