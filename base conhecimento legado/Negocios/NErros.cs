using Domain;
using NHibernate;

namespace Negocios
{
    public class NErros : NBase<CErros>
    {
        public NErros(ISession secaoBanco) : base(secaoBanco) { }
    }
}
