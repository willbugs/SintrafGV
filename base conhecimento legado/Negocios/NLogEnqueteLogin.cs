using Domain;
using NHibernate;

namespace Negocios
{
    public class NLogEnqueteLogin : NBase<CLogEnqueteLogin>
    {
        public NLogEnqueteLogin(ISession secaoBanco) : base(secaoBanco) { }
    }
}
