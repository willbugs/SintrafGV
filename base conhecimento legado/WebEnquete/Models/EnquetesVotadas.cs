using Domain;

namespace Web.Models
{
    public class EnquetesVotadas
    {
        public virtual CEnquetes Enquete { get; set; }
        public virtual bool Votado { get; set; }
        public virtual bool Ativa { get; set; }
        public virtual bool Associados { get; set; }
        public virtual CPessoas  Pessoa { get; set; }
    }
}