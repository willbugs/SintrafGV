using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Caches.SysCache2;
using NHibernate.Tool.hbm2ddl;
using Domain;

namespace Persistencia
{
    public class FluentSessionFactory
    {
        public static ISessionFactory Sessionfactory;
        private static Banco _banco;
        private static NHibernate.Cfg.Configuration _configFluent;

        private static ISessionFactory CriarSessionFactory(string nCliente, bool criarBanco)
        {

            #region configurando qual banco de dados usar?

            nCliente = nCliente.Replace("/", "").Trim().ToUpper();
            if (nCliente.Equals("")) nCliente = "PCMAX";

            System.Configuration.ConfigurationManager.RefreshSection(nCliente);

            var connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings[nCliente].ConnectionString;

            // adaptacao podre!
            if (System.Environment.MachineName == "WILLBUGS")
            {
                connectionstring = connectionstring.Replace("Data Source=PCMAX;", "Data Source=WILLBUGS;");
            }

            IPersistenceConfigurer configDb = null;
            switch (_banco)
            {
                case Banco.SQLServer:
                    configDb = MsSqlConfiguration.MsSql2012.ConnectionString(connectionstring).IsolationLevel(System.Data.IsolationLevel.ReadCommitted);
                    break;
                case Banco.PostgreSQL:
                    configDb = PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionstring).IsolationLevel(System.Data.IsolationLevel.ReadCommitted);
                    break;
                case Banco.MySQL:
                    configDb = MySQLConfiguration.Standard.ConnectionString(connectionstring).IsolationLevel(System.Data.IsolationLevel.ReadCommitted);
                    break;
                case Banco.FirebirdSql:
                    {
                        var firebirdCfg = new FirebirdConfiguration();
                        configDb = firebirdCfg.ConnectionString(connectionstring).IsolationLevel(System.Data.IsolationLevel.ReadCommitted);
                    }
                    break;
            }
            #endregion

            _configFluent = Fluently.Configure()
                .Database(configDb)
                .Cache(c => c.UseQueryCache().UseSecondLevelCache().ProviderClass<SysCacheProvider>())
                .ExposeConfiguration(cfg => cfg.SetProperty("adonet.batch_size", "2000"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<MUsuarios>())
                //.Mappings(m => m.AutoMappings.Add(AutoMap.AssemblyOf<MUsuarios>().Where(n => n.Name.StartsWith("M"))))
                .BuildConfiguration();

            Sessionfactory = _configFluent.BuildSessionFactory();

            return Sessionfactory;
        }

        public static ISession AbrirSession(string nBanco = "DATABASE", Banco bancoEscolhido = Banco.SQLServer, bool criarBanco = false)
        {
            _banco = bancoEscolhido;
            var session = CriarSessionFactory(nBanco, criarBanco).OpenSession();
            if (criarBanco) new SchemaExport(_configFluent).Execute(true, true, false, session.Connection, null);
            return session;
        }

    }
}
