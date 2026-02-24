using System.Web.Mvc;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Web.InfraEstrutura;

namespace Web.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly ISession _sessionDefault;
        private readonly ILoginProvider _loginprovider;

        public RelatoriosController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
        {
            if (_sessionDefault == null) _sessionDefault = kernelParam.Get<ISession>("Default");
            if (_loginprovider == null) _loginprovider = loginProviderParam;
        }
        [ValidacaoUsuario]
        public ActionResult Index()
        {
            return View();
        }
    }
}