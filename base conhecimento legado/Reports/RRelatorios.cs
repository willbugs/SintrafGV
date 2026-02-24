using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Domain;
using iTextSharp.text;
using Negocios;
using NHibernate;

namespace Reports
{
    public class RRelatorios
    {
        public MemoryStream ListagemProvedores(ISession secao, CRelatorios cRelatorio, FormCollection model)
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
                    Width = 120F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "DOCUMENTO",
                    TipoCampo = "C",
                    TituloCampo = "Documento",
                    Ordem = 2,
                    Width = 40F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "TELEFONE",
                    TipoCampo = "C",
                    TituloCampo = "Telefone",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "EMAIL",
                    TipoCampo = "C",
                    TituloCampo = "email",
                    Ordem = 4,
                    Width = 70F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "TotalClientesAtivos",
                    TipoCampo = "C",
                    TituloCampo = "T. Ativos",
                    Ordem = 5,
                    Width = 28F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "TotalClientesInativos",
                    TipoCampo = "C",
                    TituloCampo = "T. Inativos",
                    Ordem = 6,
                    Width = 28F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "TotalPlano1",
                    TipoCampo = "C",
                    TituloCampo = "Plano 1 Tela",
                    Ordem = 7,
                    Width = 28F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "TotalPlano2",
                    TipoCampo = "C",
                    TituloCampo = "Plano 3 Tela2",
                    Ordem = 8,
                    Width = 28F
                },
            };

            var nProvedor = new NProvedor(secao);
            var nClientes = new NPessoas(secao);
            var dados = nProvedor.RetornarTodosAtivos();
            
            foreach (var provedor in dados)
            {
                provedor.TotalClientesAtivos = nClientes.RetornarTotalAtivosProvedor(provedor);
                provedor.TotalClientesInativos = nClientes.RetornarTotalInativosProvedor(provedor);
                provedor.TotalPlano1 = nClientes.RetornarTotalPlano(provedor,1);
                provedor.TotalPlano2 = nClientes.RetornarTotalPlano(provedor,3);
            }

            var relatorio = new RBaseReport
            {
                Titulo = "Listagem de provedores ativos",
                SegundoTitulo = "Clientes ativos/inativos",
                TotalizaLinhas = true,
                Horizontal = true,
                LinhasPagina = 58
            };

            return relatorio.Imprimir(dados, campos);
        } 
        public MemoryStream ListagemClientes(ISession secao, CRelatorios cRelatorio, FormCollection model)
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
                    Width = 85F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CPF",
                    TipoCampo = "C",
                    TituloCampo = "Documento",
                    Ordem = 2,
                    Width = 35F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "TELEFONE",
                    TipoCampo = "C",
                    TituloCampo = "Telefone",
                    Ordem = 3,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "PLANO",
                    TipoCampo = "C",
                    TituloCampo = "Plano",
                    Ordem = 4,
                    Width = 20F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "PROVEDOR.NOME",
                    TipoCampo = "C",
                    TituloCampo = "Provedor",
                    Ordem = 5,
                    Width = 65F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATAATIVACAO",
                    TipoCampo = "D",
                    TituloCampo = "D.Ativação",
                    Ordem = 6,
                    Width = 38F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATABLOQUEIO",
                    TipoCampo = "D",
                    TituloCampo = "D.Bloqueio",
                    Ordem = 7,
                    Width = 40F
                },
            };

          var nClientes = new NPessoas(secao);
          var dados = nClientes.RetornarTodossProvedor();
          foreach (var item in dados)
          {
              item.PLANO = item.PLANO.ToUpper().Replace("STENNA", "Looke");
          }
          var relatorio = new RBaseReport
            {
                Titulo = "Listagem de clientes",
                SegundoTitulo = "Geral",
                TotalizaLinhas = true,
                Horizontal = true,
                LinhasPagina = 58
            };

            return relatorio.Imprimir(dados, campos);
        }
    }
}
