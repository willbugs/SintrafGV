using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;
using NHibernate.Linq;

namespace Negocios
{
    public class NGrupoUsuarios : NBase<CGrupoUsuarios>
    {
        public NGrupoUsuarios(ISession secaoBanco) : base(secaoBanco) { }

        public virtual IList<CGrupoUsuarios> RetornarNomegrupo(string chave)
        {
            return (from t in SecaoBanco.Query<CGrupoUsuarios>() where t.Nomegrupo.ToUpper().Contains(chave.ToUpper()) orderby t.Nomegrupo select t).ToFuture().ToList();
        }
        public virtual IList<CGrupoUsuarios> RetornarIdgrupo(string chave)
        {
            return (from t in SecaoBanco.Query<CGrupoUsuarios>() where t.Sguid.Contains(chave.ToUpper()) orderby t.Nomegrupo select t).ToFuture().ToList();
        }
    }
}
