using System.Collections.Generic;
using NHibernate;
using System.Linq;
using Domain;

namespace Negocios
{
    public class NUsuarios : NBase<CUsuarios>
    {
        public NUsuarios(ISession secaoBanco) : base(secaoBanco) { }

        public virtual CUsuarios RetornarLogin(string loginParam)
        {
            var lobjects = (from e in SecaoBanco.Query<CUsuarios>() where e.LOGINUSUARIO == loginParam select e).FirstOrDefault();
            if (lobjects != null) { SecaoBanco.Refresh(lobjects); }
            return lobjects;
        }
        public virtual CUsuarios RetornarSessionId(string sessionid)
        {
            var lobjects = (from e in SecaoBanco.Query<CUsuarios>() where e.Sessao == sessionid select e).FirstOrDefault(f => true);
            if (lobjects != null) { SecaoBanco.Refresh(lobjects); }
            return lobjects;
        }
        public virtual IList<CUsuarios> RetornarUsuarioNome(string nome)
        {
            var lobjects = (from e in SecaoBanco.Query<CUsuarios>() where e.NOMEUSUARIO.Contains(nome) select e).ToList();
            foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
    }
}
