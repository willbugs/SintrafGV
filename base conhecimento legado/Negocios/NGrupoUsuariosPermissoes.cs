using Domain;
using NHibernate;

namespace Negocios
{
    public class NGrupoUsuariosPermissoes : NBase<CGrupoUsuariosPermissoes>
    {
        public NGrupoUsuariosPermissoes(ISession secaoBanco) : base(secaoBanco) { }
    }
}
