namespace Domain
{
    public class CPessoasEnquetes : CBase
    {
        public virtual CPessoas PESSOA { get; set; }
        public virtual CEnquetes ENQUETE { get; set; }
        public virtual string PERGUNTA { get; set; }
        public virtual int RESPOSTA01 { get; set; }
        public virtual int RESPOSTA02 { get; set; }
        public virtual System.DateTime? DATA { get; set; }

        public CPessoasEnquetes()
        {
            RESPOSTA01 = 0;
            RESPOSTA02 = 0;
        }
    }
}
