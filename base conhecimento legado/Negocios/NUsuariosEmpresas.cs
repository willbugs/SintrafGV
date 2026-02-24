using NHibernate;
using Domain;

namespace Negocios
{
    public class NUsuariosEmpresas : NBase<CUsuariosEmpresas>
    {
        public NUsuariosEmpresas(ISession secaoBanco) : base(secaoBanco) { }
    }
}
