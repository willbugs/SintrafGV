using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;
using NHibernate.Linq;

namespace Negocios
{
    public class NEmpresa : NBase<CEmpresa>
    {
        public NEmpresa(ISession secaoBanco) : base(secaoBanco) { }

        public virtual IList<CEmpresa> RetornarNOMEREDUZIDO(string sNOMEREDUZIDO)
        {
            return (from e in SecaoBanco.Query<CEmpresa>() where e.NOMEREDUZIDO.Contains(sNOMEREDUZIDO.ToUpper()) select e).AsEnumerable().ToList();
        }
        public virtual IList<CEmpresa> RetornarTodosNOMEREDUZIDO()
        {
            return (from e in SecaoBanco.Query<CEmpresa>() orderby e.NOMEREDUZIDO select e).ToFuture().ToList();
        }
        public virtual IList<CEmpresa> RetornarRazao(string chave)
        {
            return (from e in SecaoBanco.Query<CEmpresa>() where e.EMITENTE_RAZAOSOCIAL.Contains(chave.ToUpper()) orderby e.EMITENTE_RAZAOSOCIAL select e).ToFuture().ToList();
        }
        public virtual IList<CEmpresa> RetornarFantasia(string chave)
        {
            return (from e in SecaoBanco.Query<CEmpresa>() where e.EMITENTE_FANTASIA.Contains(chave.ToUpper()) orderby e.EMITENTE_FANTASIA select e).ToFuture().ToList();
        }
        public virtual IList<CEmpresa> RetornarCpfcnpj(string s)
        {
            return (from e in SecaoBanco.Query<CEmpresa>() where e.EMITENTE_CNPJ == s select e).AsEnumerable().ToList();
        }
    }
}
