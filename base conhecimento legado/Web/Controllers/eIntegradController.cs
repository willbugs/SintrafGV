using System.Web.Mvc;
using Ninject.Syntax;
using Web.InfraEstrutura;
using System.Collections.Generic;
using System.Linq;
using Domain;
using NHibernate;
using Negocios;
using Ninject;

namespace Web.Controllers
{
    
    public class EIntegradController : Controller
    {
        private static IList<CMenus> _cMenuses;
        private static ILoginProvider _loginprovider;
        private static ISession _sessionDefault;

        private static void _Sair()
        {
            try
            {
                _loginprovider.Desautenticar();
            }
            finally
            {
                if (_sessionDefault.IsOpen)
                {
                    _sessionDefault.Flush();
                    _sessionDefault.Clear();
                }
            }
        }
        public EIntegradController(IResolutionRoot kernelParam, ILoginProvider loginproviderParam)
        {
            _sessionDefault = kernelParam.Get<ISession>("Default");
            _loginprovider = loginproviderParam;
            if (!_loginprovider.Autenticado && !_loginprovider.Selecionado) return;
            ViewBag.Usuario = _loginprovider.Usuario.NOMEUSUARIO;
            ViewBag.NomeEmpresa = _loginprovider.Empresa.NOMEREDUZIDO;
        }
        public JsonResult Jsair()
        {
            _Sair();
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Sair()
        {
            if (!_loginprovider.Autenticado)
            {
                return RedirectToRoute(new { contoller = "Home", action = "index" });
            }
            _Sair();
            return RedirectToRoute(new { contoller = "Home", action = "index" });
        }
        
        [ValidacaoUsuario]
        public ActionResult Index()
        {
            var nMenus = new NMenus(_sessionDefault);
            var menusdopacote = nMenus.RetornarTodos().OrderBy(f => f.Sequencia).ToList();
            var permissao = _loginprovider.Usuario.USUARIOSPERMISSOES;
            _cMenuses = new List<CMenus>();
            foreach (var cMenuse in menusdopacote.OrderBy(o => o.Sequencia))
            {
                cMenuse.Usuario = _loginprovider.Usuario;

                foreach (var sub in cMenuse.SubMenu)
                {
                    if (permissao.Any(e => e.Atela == "A - " + sub.Descricao))
                    {
                        sub.Acesso = permissao.First(e => e.Atela == "A - " + sub.Descricao).Ativo;
                    }
                    else
                    {
                        sub.Acesso = 0;
                    }
                }

                _cMenuses.Add(cMenuse);
            }
            return View(_cMenuses);
        }
    }
}
