using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Domain;
using iTextSharp.text;
using Negocios;
using NHibernate;

namespace Reports
{

    public class RProvedor
    {
        public MemoryStream ListagemProvedor(ISession secao, CRelatorios cRelatorio, FormCollection model)
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
                    Width = 75F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_CENTER,
                    Currency = false,
                    NomeCampo = "DATABLOQUEIO",
                    TipoCampo = "D",
                    TituloCampo = "Data bloqueio",
                    Ordem = 5,
                    Width = 30F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "CONTATO",
                    TipoCampo = "C",
                    TituloCampo = "Contato",
                    Ordem = 6,
                    Width = 70F
                },
                new CamposRelatorios
                {
                    Alinhamento = Element.ALIGN_LEFT,
                    Currency = false,
                    NomeCampo = "TELEFONECONTATO",
                    TipoCampo = "C",
                    TituloCampo = "Tel.Contato",
                    Ordem = 7,
                    Width = 30F
                }
            };

            var nProvedor = new NProvedor(secao);
            var dados = nProvedor.RetornarTodos();


            var relatorio = new RBaseReport
            {
                Titulo = "Listagem de provedores",
                SegundoTitulo = "",
                TotalizaLinhas = true,
                Horizontal = true,
                LinhasPagina = 58
            };

            return relatorio.Imprimir(dados, campos);
        }

    }
}
