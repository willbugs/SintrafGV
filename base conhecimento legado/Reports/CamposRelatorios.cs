namespace Reports
{
    public class CamposRelatorios
    {
        public virtual int Ordem { get; set; }
        public virtual string NomeCampo { get; set; }
        public virtual string TituloCampo { get; set; }
        public virtual string TipoCampo { get; set; }
        public virtual bool Currency { get; set; }
        public virtual int Alinhamento { get; set; }
        public virtual bool Totalizar { get; set; }
        public virtual float Width { get; set; }
    }
}
