using System;

namespace Domain
{
    public class RelatorioAssociados
    {
        public virtual int Ordem { get; set; }
        public virtual string NomeCampo { get; set; }
        public virtual string TituloCampo { get; set; }
        public virtual string TipoCampo { get; set; }
        public virtual bool Currency { get; set; }
        public virtual int Alinhamento { get; set; }
        public virtual bool Totalizar { get; set; }
        public virtual float Width { get; set; }
        public virtual bool Visivel { get; set; }
        public virtual string Filtro { get; set; }
        public virtual string Mascara { get; set; }
        public virtual Type Tipo { get; set; }
        public virtual bool Lookup { get; set; }
        public virtual string LookupCampo { get; set; }

    }
}