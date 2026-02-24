using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;

namespace Negocios
{
    public class NEnquetes : NBase<CEnquetes>
    {
        public NEnquetes(ISession secaoBanco) : base(secaoBanco) { }
        public virtual IList<CEnquetes> RetornarListaTitulos(string s)
        {
            var lobjects = (from e in SecaoBanco.Query<CEnquetes>() where e.TITULO.Contains(s) select e).ToList();
            //foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
        public virtual IList<CEnquetes> RetornarEnquetesBanco(string banco)
        {
            var lobjects = (from e in SecaoBanco.Query<CEnquetes>() where e.BANCO.Sguid.Equals(banco) select e).ToList();
            //foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
        public virtual IList<CEnquetes> RetornarEnquetesSemBanco()
        {
            var lobjects = (from e in SecaoBanco.Query<CEnquetes>() where e.BANCO == null select e).ToList();
            //foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
        public virtual IEnumerable<CEnquetes> RetornarTodasEnquetes()
        {
            var lobjects = (from e in SecaoBanco.Query<CEnquetes>() orderby e.DATA descending, e.TITULO select e).ToList();
            //foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
        public override IEnumerable<CEnquetes> RetornarTodos(CEmpresa empresa = null)
        {
            var lobjects = (from t in SecaoBanco.Query<CEnquetes>() orderby t.DATA descending, t.TITULO select t).AsEnumerable().ToList();

            //foreach (var objeto in lobjects)
            //{
            //    SecaoBanco.Refresh(objeto);
            //}
            return lobjects;
        }
    }
}
