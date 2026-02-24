using Domain;
using NHibernate;

namespace Negocios
{
    public class NParametros : NBase<CParametros>
    {
        public NParametros(ISession secaoBanco) : base(secaoBanco) { }
        
    }
}
