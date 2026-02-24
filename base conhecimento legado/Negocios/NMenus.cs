using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Domain;
using NHibernate.Linq;

namespace Negocios
{
    public class NMenus : NBase1<CMenus>
    {
        public NMenus(ISession secaoBanco) : base(secaoBanco) { }
        public virtual IList<CMenus> RetornarTodosMenusUsuario(string id)
        {
            return (from t in SecaoBanco.Query<CMenus>() where t.Usuario.Sguid == id orderby t.Sequencia select t).ToFuture().ToList();
        }

        public override IList<CMenus> RetornarTodos()
        {
            return (from t in SecaoBanco.Query<CMenus>() orderby t.Sequencia select t).ToFuture().ToList();
        }
    
    }
    public class NSubMenus : NBase1<CSubMenus>
    {
        public NSubMenus(ISession secaoBanco) : base(secaoBanco) { }

    }
    public class NTelas : NBase1<CTelas>
    {
        public NTelas(ISession secaoBanco) : base(secaoBanco) { }

        public virtual IList<CTelas> RetornarTelasPorSubmenu(CSubMenus submenu)
        {
            return
                (from t in SecaoBanco.Query<CTelas>() where t.SubMenu == submenu orderby t.Lista select t).ToFuture()
                    .ToList();
        }

        public virtual CTelas RetornarTelasPorNome(string nome)
        {
            return
                (from t in SecaoBanco.Query<CTelas>() where t.Help == nome orderby t.Help select t).FirstOrDefault();

        }

        public virtual CTelas RetornarTelasPorHelp(string help)
        {
            return
                (from t in SecaoBanco.Query<CTelas>() where t.Help == help orderby t.Help select t).FirstOrDefault();

        }

    }
    public class NCampos : NBase1<CCampos>
    {
        public NCampos(ISession secaoBanco) : base(secaoBanco) { }

        public virtual IList<CCampos> RetornarTodosTela(string idTela)
        {
            return (from t in SecaoBanco.Query<CCampos>() where t.Tela.Sguid == idTela select t).ToFuture().ToList();
        }

        public virtual IList<CCampos> RetornarCamposConsultaveisPorSubmenu(IList<CTelas> listatelas)
        {
            var listacampos = new List<CCampos>();
            foreach (var listatela in listatelas)
            {
                listacampos.AddRange(listatela.Campos.Where(t => t.Chave == 1).ToList());
            }
            return listacampos;
        }

    }

    public class NRelatorios : NBase1<CRelatorios>
    {
        public NRelatorios(ISession secaoBanco) : base(secaoBanco) { }
        public virtual IList<CRelatorios> RetornarTodosTela(string idTela)
        {
            return (from t in SecaoBanco.Query<CRelatorios>() where t.Tela.Sguid == idTela select t).ToFuture().ToList();
        }

    }
    public class NRelatoriosDataSet : NBase1<CRelatoriosDataSet>
    {
        public NRelatoriosDataSet(ISession secaoBanco) : base(secaoBanco) { }

    }
    public class NRelatoriosFiltros: NBase1<CRelatoriosFiltros>
    {
        public NRelatoriosFiltros(ISession secaoBanco) : base(secaoBanco) { }

    }

}
