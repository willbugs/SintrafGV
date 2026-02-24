using System.Linq;
using Domain;
using NHibernate;

namespace Negocios
{
    public class NResultadoEnquetes : NBase<CResultadoEnquetes>
    {
        public NResultadoEnquetes(ISession secaoBanco) : base(secaoBanco) { }
        public virtual CResultadoEnquetes RetornarResultadoenquete(CEnquetes enquete)
        {
            var lobjects = (from e in SecaoBanco.Query<CResultadoEnquetes>() where e.ENQUETE.Sguid.Equals(enquete.Sguid) orderby e.PERGUNTA select e).FirstOrDefault(e => true);
            if (lobjects != null) { SecaoBanco.Refresh(lobjects); }
            return lobjects;
        }
    }
}
