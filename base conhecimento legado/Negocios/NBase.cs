using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Domain;
using NHibernate;

namespace Negocios
{
    public abstract class NBase<Tabela> where Tabela : CBase
    {
        protected static ISession SecaoBanco;

        protected NBase(ISession secaoBanco1)
        {
            if (SecaoBanco == null)
            {
                SecaoBanco = secaoBanco1;
            }
        }

        public virtual void Gravar(Tabela entidade, bool newRecord = false)
        {
            using (var transacao = SecaoBanco.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    SecaoBanco.SaveOrUpdate(entidade);
                    transacao.Commit();
                }
                catch (Exception e)
                {
                    if (!transacao.WasCommitted)
                        transacao.Rollback();

                    if (e.InnerException != null)
                    {
                        throw new Exception(e.InnerException.Message, e);
                    }

                    if (e.Message != null)
                    {
                        throw new Exception(e.Message, e);
                    }

                    throw new Exception("Impossível salvar registro!", e);
                }
            }
        }
        public virtual void Gravar(IEnumerable<Tabela> listaEntidades)
        {
            using (var session = SecaoBanco.SessionFactory.OpenStatelessSession())
            using (var transacao = session.BeginTransaction())
            {
                try
                {
                    foreach (var entidade in listaEntidades)
                    {
                        session.Insert(entidade);
                    }
                    transacao.Commit();
                }
                catch (Exception e)
                {
                    if (!transacao.WasCommitted)
                        transacao.Rollback();

                    if (e.InnerException != null)
                    {
                        throw new Exception(e.InnerException.Message, e);
                    }

                    if (e.Message != null)
                    {
                        throw new Exception(e.Message, e);
                    }

                    throw new Exception("Impossível salvar registro!", e);
                }

            }
        }
        public virtual void Excluir(Tabela entidade)
        {
            _Excluir(entidade);
        }
        public virtual void Excluir(string sGuid)
        {
            _Excluir(SecaoBanco.Get<Tabela>(sGuid));
        }
        public virtual void _Excluir(Tabela entidade)
        {
            using (var transacao = SecaoBanco.BeginTransaction())
            {
                try
                {
                    SecaoBanco.Evict(entidade);
                    SecaoBanco.Delete(entidade);
                    transacao.Commit();
                }
                catch (Exception e)
                {
                    if (!transacao.WasCommitted) transacao.Rollback();
                    throw new Exception("Impossível excluir registro!", e);
                }
            }
        }
        public virtual Tabela RetornarGuid(string sGuid)
        {
            var lobjects = (from t in SecaoBanco.Query<Tabela>() where (t.Sguid == sGuid) select t).AsEnumerable().SingleOrDefault();
            SecaoBanco.Refresh(lobjects);
            return lobjects;
        }
        public virtual IEnumerable<Tabela> RetornarTodos(CEmpresa empresa = null)
        {
            var lobjects = (from t in SecaoBanco.Query<Tabela>() select t).ToList();
            //foreach (var objeto in lobjects) { SecaoBanco.Refresh(objeto); }
            return lobjects;
        }
    }
}
