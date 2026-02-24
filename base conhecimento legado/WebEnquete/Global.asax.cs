using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NHibernate;
using Ninject;
using Web.InfraEstrutura;

namespace Web
{
    public class MvcApplication : HttpApplication
    {
        [Obsolete("Obsolete")] private IKernel Kernel { get; set; }
        private ISession _session;

        [Obsolete("Obsolete")]
        protected void Application_Start()
        {
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            DependencyResolver.SetResolver(new NinjectDependencyResolver(virtualPath));
            FilterProviders.Providers.Clear();
            FilterProviders.Providers.Add(new FiltroProviderCustom());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_Error()
        //{
        //    InjetarDependencia();
        //    var httpContext = HttpContext.Current;
        //    var exception = Server.GetLastError();
        //    var secao = httpContext.Session.SessionID;
        //    var mensagem = exception.Message;
        //    var source = exception.Source;
        //    var stackTrace = exception.StackTrace.Replace("\r\n", "").Substring(0, 255);
        //    var ip = Request.ServerVariables["REMOTE_ADDR"];
        //    var nErros = new NErros(_session);
        //    var nUsuarios = new NUsuarios(_session);

        //    Server.ClearError();

        //    var cErros = new CErros
        //    {
        //        Mensagem = mensagem,
        //        Source = source,
        //        StackTrace = stackTrace,
        //        IpServidor = ip
        //    };

        //    try
        //    {
        //        var cUsusario = nUsuarios.RetornarSessionId(secao);

        //        if (cUsusario != null)
        //        {
        //            cUsusario.Logado = 0;
        //            nUsuarios.Gravar(cUsusario);
        //        }

        //        nErros.Gravar(cErros);
        //    }
        //    catch
        //    {

        //    }
        //    finally
        //    {
        //        httpContext.Response.Redirect($"/Error/Index/");
        //    }
        //}
        //private void InjetarDependencia()
        //{
        //    Kernel = new StandardKernel();
        //    Kernel.Bind<ISession>().ToMethod(c => FluentSessionFactory.AbrirSession("WILLBUGS")).InSingletonScope().Named("Default");
        //    Kernel.Inject(this);
        //    _session = Kernel.Get<ISession>("Default");
        //}
    }
}

//Application_Init: Dispara quando o aplicativo é inicializado pela primeira vez.
//Application_Start: Dispara a primeira vez que um aplicativo é iniciado.
//Session_Start: Dispara a primeira vez quando a sessão de um usuário é iniciada.
//Application_BeginRequest: Dispara toda vez que uma nova solicitação chega.
//Application_EndRequest: Dispara quando o pedido termina.
//Application_AuthenticateRequest: Indica que uma solicitação está pronta para ser autenticada.
//Application_Error: Dispara quando ocorre um erro não manipulado no aplicativo.
//Session_End: Dispara sempre que uma única sessão do usuário termina ou atinge o tempo limite.
//Application_
