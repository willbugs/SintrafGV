using Domain;

namespace Web.Models
{
    public class ClasseBase
    {
        public virtual string Secao { get; set; }
        public virtual CBase CBase { get; set; }
    }
}