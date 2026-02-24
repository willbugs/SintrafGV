using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;

namespace Negocios
{
    public abstract class NBase1<Tabela> where Tabela : CBase1
    {
        protected static ISession SecaoBanco;

        protected NBase1(ISession SecaoBanco1)
        {
            if (SecaoBanco1 != null)
            {
                SecaoBanco = SecaoBanco1;
            }

        }

        public virtual string Inserir(Tabela entidade)
        {
            using (var transacao = SecaoBanco.BeginTransaction())
            {
                try
                {
                    var retorno = SecaoBanco.Save(entidade);
                    transacao.Commit();
                    return retorno.ToString();
                }
                catch (Exception)
                {
                    if (!transacao.WasCommitted)
                        transacao.Rollback();
                    throw;
                }
            }
        }

        public virtual void Alterar(Tabela entidade)
        {

            using (var transacao = SecaoBanco.BeginTransaction())
            {
                try
                {
                    SecaoBanco.Update(entidade);
                    transacao.Commit();
                }
                catch (Exception)
                {
                    if (!transacao.WasCommitted)
                        transacao.Rollback();
                    throw;
                }
            }
        }

        public virtual void Excluir(Tabela entidade)
        {
            using (var transacao = SecaoBanco.BeginTransaction())
            {
                try
                {
                    var objExcluir = SecaoBanco.Get<Tabela>(entidade.Sguid);
                    SecaoBanco.Delete(objExcluir);
                    transacao.Commit();
                }
                catch (Exception)
                {
                    if (!transacao.WasCommitted)
                        transacao.Rollback();
                    throw;
                }
            }
        }

        public virtual Tabela RetornarGuid(string sGuid)
        {
            var retorno = (from t in SecaoBanco.Query<Tabela>() where (t.Sguid == sGuid)  select t).SingleOrDefault();
            return retorno;
        }
        public virtual IList<Tabela> RetornarTodos(CEmpresa empresa = null)
        {
            return (from t in SecaoBanco.Query<Tabela>() select t).AsEnumerable().ToList();
        }
        public virtual IList<Tabela> RetornarTodos()
        {
            return (from t in SecaoBanco.Query<Tabela>() select t).AsEnumerable().ToList();
        }
    }
}
