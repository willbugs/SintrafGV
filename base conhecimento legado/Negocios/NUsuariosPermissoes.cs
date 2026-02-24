using Domain;
using NHibernate;

namespace Negocios
{
    public class NUsuariosPermissoes: NBase<CUsuariosPermissoes>
    {
        public NUsuariosPermissoes(ISession secaoBanco) : base(secaoBanco) { }
    }
}
