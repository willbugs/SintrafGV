using System;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace Teste
{
    public class Relatorios
    {
        private int _paginacao;
        private int _linha;
        private readonly Font _font = new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL);
        private readonly LineSeparator _line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
        private Document _doc;
        private readonly string _path;

        public virtual string Titulo { get; set; }
        public virtual string SegundoTitulo { get; set; }

        public Relatorios(string path)
        {
            _path = path;
        }
        public void Imprimir()
        {
            var fs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None);
            var table = new PdfPTable(6) { HorizontalAlignment = Element.ALIGN_LEFT, DefaultCell = { Border = Rectangle.NO_BORDER } };

            _doc = new Document();
            PdfWriter.GetInstance(_doc, fs);
            _doc.SetPageSize(PageSize.A4);
            _doc.SetMargins(10f, 20f, 10f, 10f);
            _doc.Open();

            for (var i = 0; i < 96; i++)
            {
                if (_linha == 49)
                {
                    _doc.Add(table);
                    Rodape();
                }

                if (_linha.Equals(0))
                {
                    NovaPagina();
                    table = new PdfPTable(6) { HorizontalAlignment = Element.ALIGN_LEFT, DefaultCell = { Border = Rectangle.NO_BORDER } };
                }

                table.AddCell(new Phrase("Col 1 Row " + _linha, _font));
                table.AddCell(new Phrase("Col 2 Row " + _linha, _font));
                table.AddCell(new Phrase("Col 3 Row " + _linha, _font));
                table.AddCell(new Phrase("Col 4 Row " + _linha, _font));
                table.AddCell(new Phrase("Col 5 Row " + _linha, _font));
                table.AddCell(new Phrase("Col 6 Row " + _linha, _font));

                //_doc.Add(new Paragraph(_linha.ToString(), _font) { Alignment = Element.ALIGN_RIGHT });
                _linha++;
            }

            if (_linha <= 49)
            {
                _doc.Add(table);
            }

            Rodape(true);
            _doc.Close();
        }

        private void Rodape(bool fim = false)
        {
            _doc.Add(Chunk.NEWLINE);
            _doc.Add(_line);

            var table = new PdfPTable(1) { HorizontalAlignment = Element.ALIGN_LEFT, DefaultCell = { Border = Rectangle.NO_BORDER } };
            table.AddCell(new Phrase("RODAPE 1", _font));
            table.AddCell(new Phrase("RODAPE 2", _font));
            
            _doc.Add(table);
            _doc.Add(new Paragraph("PcMaxInf", new Font(Font.FontFamily.HELVETICA, 6, Font.BOLD)) { Alignment = Element.ALIGN_RIGHT });
            _linha = 0;
            if (!fim) _doc.NewPage();
        }
        private void NovaPagina()
        {
            _paginacao++;
            var p = new Paragraph(Titulo, _font) { Alignment = Element.ALIGN_CENTER };
            var cola = new Chunk(new VerticalPositionMark());
            var table = new PdfPTable(6) { HorizontalAlignment = Element.ALIGN_LEFT, DefaultCell = { Border = Rectangle.NO_BORDER } };
            p.Add(new Chunk(cola));
            p.Add("Página " + _paginacao.ToString("####"));
            _doc.Add(_line);
            _doc.Add(p);
            p = new Paragraph(SegundoTitulo, _font) { Alignment = Element.ALIGN_RIGHT };
            p.Add(new Chunk(cola));
            p.Add(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            _doc.Add(p);
            _doc.Add(Chunk.NEWLINE);
            _doc.Add(_line);

            table.AddCell(new Phrase("Nome            ", _font));
            table.AddCell(new Phrase("Endereco        ", _font));
            table.AddCell(new Phrase("Valor xwer ", _font));
            table.AddCell(new Phrase("Valor sdsd ", _font));
            table.AddCell(new Phrase("Data Nascimento   ", _font));
            table.AddCell(new Phrase("Data do contrqato ", _font));

            _doc.Add(table);
            _doc.Add(new LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1));
            _linha = 0;
        }
    }
}
