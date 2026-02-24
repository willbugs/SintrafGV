namespace Domain
{
    public class CResultadoEnquetes : CBase
    {
        public virtual CEnquetes ENQUETE { get; set; }
        public virtual string PERGUNTA { get; set; }
        public virtual int RESPOSTA01 { get; set; }
        public virtual int RESPOSTA02 { get; set; }
    }
}
