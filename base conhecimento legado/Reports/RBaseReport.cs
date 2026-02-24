using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace Reports
{
    public sealed class RBaseReport
    {
        private int _paginacao;
        private int _linha;
        private int _registros;
        private readonly Font _font;
        private readonly LineSeparator _line;
        private Document _doc;
        private float[] _widths;
        private readonly decimal[] _totais;
        private IEnumerable<CamposRelatorios> _camposRelatorioses;
        public string Titulo { private get; set; }
        public string SegundoTitulo { private get; set; }
        public bool TotalizaLinhas { private get; set; }
        public bool Horizontal { private get; set; }
        public int LinhasPagina { private get; set; }
        public int TotalAtivos { get; set; }
        public int TotalInativos { get; set; }
        public int TotalAssociado { get; set; }
        public int TotalNAssociado { get; set; }

        public RBaseReport()
        {
            _totais = new[] { 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m };
            _line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, -2);
            _font = new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL);
        }

        public MemoryStream Imprimir(IEnumerable<CBase> lista, IEnumerable<CamposRelatorios> campos)
        {
            var fs = new MemoryStream();
            var relatorioses = campos.ToList();
            var table = new PdfPTable(6) { HorizontalAlignment = Element.ALIGN_LEFT, DefaultCell = { Border = Rectangle.NO_BORDER } };
            var ncolunas = relatorioses.Count;
            _camposRelatorioses = campos as CamposRelatorios[] ?? relatorioses.OrderBy(e => e.Ordem).ToArray();
            _widths = new float[_camposRelatorioses.Count()];
            foreach (var cp in _camposRelatorioses) { _widths[cp.Ordem - 1] = cp.Width; }
            _doc = new Document();
            PdfWriter.GetInstance(_doc, fs);
            _doc.SetPageSize(Horizontal ? PageSize.A4.Rotate() : PageSize.A4);
            _doc.Open();
            foreach (var item in lista)
            {
                if (_linha >= LinhasPagina)
                {
                    _doc.Add(table);
                    Rodape();
                }
                if (_linha.Equals(0))
                {
                    NovaPagina();
                    table = new PdfPTable(ncolunas)
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        DefaultCell = { Border = Rectangle.NO_BORDER },
                        WidthPercentage = 100,
                        RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                        ExtendLastRow = false,
                        HeaderRows = 1,
                        LockedWidth = true,
                        TotalWidth = _doc.PageSize.Equals(PageSize.A4) ? 500f : 780f
                    };
                    table.SetWidths(_widths);
                    foreach (var campo in _camposRelatorioses)
                    {
                        var alinhamento = campo.TipoCampo.Equals("N") ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
                        if (campo.TipoCampo.Equals("X")) alinhamento = Element.ALIGN_RIGHT;
                        var cell = new PdfPCell(new Phrase(campo.TituloCampo, _font))
                        {
                            Colspan = 1,
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = alinhamento
                        };
                        table.AddCell(cell);
                    }
                }
                foreach (var campo in _camposRelatorioses)
                {
                    var conteudo = item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item)?.ToString();

                    if (campo.TipoCampo.Equals("B"))
                    {
                        conteudo = "NÃO";
                        if ((bool)item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item))
                        {
                            conteudo = "SIM";
                        }
                    }

                    if (campo.TipoCampo.Equals("D") && item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item) != null)
                    {
                        var data = (DateTime)item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item);
                        conteudo = data.ToString("dd/MM/yyyy HH:mm:ss");
                    }
                    if (campo.TipoCampo.Equals("X") && item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item) != null)
                    {
                        conteudo = " ";
                    }
                    if (conteudo != null)
                    {
                        conteudo = campo.Currency ? $"{(decimal)item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item):C}" : conteudo;
                        if (campo.Totalizar)
                        {
                            if (item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item) is decimal)
                            {
                                _totais[campo.Ordem] += (decimal)item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item);
                            }
                            if (item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item) is int)
                            {
                                _totais[campo.Ordem] += (int)item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item);
                            }
                        }
                    }
                    if (conteudo == null && campo.NomeCampo.Contains("."))
                    {
                        var subcampo = campo.NomeCampo.Split('.');
                        var propert = item.GetType().GetProperty(subcampo[0])?.GetValue(item);
                        if (propert != null) conteudo = propert.GetType().GetProperty(subcampo[1])?.GetValue(propert).ToString();
                        if (conteudo != null)
                        {
                            conteudo = campo.Currency ? $"{(decimal)propert.GetType().GetProperty(subcampo[1])?.GetValue(propert):C}" : conteudo;
                            if (campo.Totalizar)
                            {
                                if (propert.GetType().GetProperty(subcampo[1])?.GetValue(propert) is decimal)
                                {
                                    _totais[campo.Ordem] += (decimal)propert.GetType().GetProperty(subcampo[1])?.GetValue(propert);
                                }
                                if (item.GetType().GetProperty(campo.NomeCampo)?.GetValue(item) is int)
                                {
                                    _totais[campo.Ordem] += (int)propert.GetType().GetProperty(subcampo[1])?.GetValue(propert);
                                }
                            }

                        }
                    }
                    var cell = new PdfPCell(new Phrase(conteudo, _font))
                    {
                        Colspan = 1,
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = campo.Alinhamento,
                        VerticalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }
                _linha++;
                _registros++;
            }
            if (_linha <= LinhasPagina)
            {
                _doc.Add(table);
            }
            Rodape(true);
            _doc.Close();
            return fs;
        }
        private void Rodape(bool fim = false)
        {
            var cola = new Chunk(new VerticalPositionMark());
            var table = new PdfPTable(_camposRelatorioses.Count())
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                DefaultCell = { Border = Rectangle.NO_BORDER },
                WidthPercentage = 100,
                RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                ExtendLastRow = false,
                LockedWidth = true,
                TotalWidth = _doc.PageSize.Equals(PageSize.A4) ? 500f : 780f
            };
            table.SetWidths(_widths);
            var cellx = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Colspan = _camposRelatorioses.Count()
            };
            cellx.AddElement(_line);
            table.AddCell(cellx);
            if (_camposRelatorioses.Any(e => e.Totalizar))
            {
                foreach (var campo in _camposRelatorioses)
                {
                    var alinhamento = campo.TipoCampo.Equals("N") ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
                    if (campo.TipoCampo.Equals("X")) alinhamento = Element.ALIGN_RIGHT;
                    var cell = new PdfPCell
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = alinhamento,
                        Colspan = 1
                    };
                    if (campo.Totalizar)
                    {
                        cell.Phrase = campo.Currency ? new Phrase($"{_totais[campo.Ordem]:C}", _font) : new Phrase($"{_totais[campo.Ordem]}", _font);
                    }
                    table.AddCell(cell);
                }
            }
            cellx = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Colspan = _camposRelatorioses.Count()
            };
            var p = new Paragraph("", _font) { Alignment = Element.ALIGN_CENTER };

            if (fim)
            {
                p.Add(new Chunk(cola));
                p.Add(new Phrase($"Total ativos {TotalAtivos:D7}  -  Total Inativos {TotalInativos:D7}  -  Total Associados {TotalAssociado:D7}  -  Não associados {TotalNAssociado:D7}", new Font(Font.FontFamily.HELVETICA, 6, Font.BOLD)));
            }

            if (TotalizaLinhas)
            {
                p.Add(new Chunk(cola));
                p.Add(new Phrase("Nº Registros " + _registros.ToString("######"), new Font(Font.FontFamily.HELVETICA, 6, Font.BOLD)));
            }

            p.Add(new Chunk(cola));
            p.Add(new Phrase("PcMaxInf", new Font(Font.FontFamily.HELVETICA, 6, Font.BOLD)));
            cellx.AddElement(p);
            table.AddCell(cellx);
            _doc.Add(table);
            _linha = 0;
            if (!fim) _doc.NewPage();
        }
        private void NovaPagina()
        {
            _paginacao++;
            var table = new PdfPTable(_camposRelatorioses.Count())
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                DefaultCell = { Border = Rectangle.NO_BORDER },
                WidthPercentage = 100,
                RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                ExtendLastRow = false,
                LockedWidth = true,
                TotalWidth = _doc.PageSize.Equals(PageSize.A4) ? 500f : 780f
            };
            table.SetWidths(_widths);
            var cellx = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Colspan = _camposRelatorioses.Count()
            };
            var p = new Paragraph(Titulo, _font) { Alignment = Element.ALIGN_CENTER };
            var cola = new Chunk(new VerticalPositionMark());
            p.Add(new Chunk(cola));
            p.Add("Página " + _paginacao.ToString("####"));
            cellx.AddElement(_line);
            cellx.AddElement(p);
            p = new Paragraph(SegundoTitulo, _font) { Alignment = Element.ALIGN_RIGHT };
            p.Add(new Chunk(cola));
            p.Add(DateTime.Now.ToString("g"));
            cellx.AddElement(p);
            cellx.AddElement(_line);
            table.AddCell(cellx);
            _doc.Add(table);
            _linha = 0;
        }
    }
}
