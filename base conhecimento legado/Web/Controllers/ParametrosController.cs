using System;
using System.Linq;
using System.Web.Mvc;
using Domain;
using Negocios;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Web.InfraEstrutura;

namespace Web.Controllers
{
    public class ParametrosController : Controller
    {
        private static ILoginProvider _loginprovider;
        private static NParametros _nParametros;

        public ParametrosController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
        {
            var sessionDefault = kernelParam.Get<ISession>("Default");

            if (!sessionDefault.IsOpen)
            {
                sessionDefault.Flush();
                sessionDefault.Reconnect();
            }

            if (!sessionDefault.IsConnected)
            {
                sessionDefault.Reconnect();
                sessionDefault.Reconnect();
            }

            if (_loginprovider == null)
            {
                _loginprovider = loginProviderParam;
            }

            if (_nParametros == null) _nParametros = new NParametros(sessionDefault);

        }

        [ValidacaoUsuario]
        [HttpGet]
        public ActionResult Index()
        {
            var cParametro = _nParametros.RetornarTodos().FirstOrDefault(e => true) ?? new CParametros();
            return PartialView(cParametro);
        }

        [HttpPost]
        [ValidacaoUsuario]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CParametros model)
        {
            var cParametro = _nParametros.RetornarTodos().FirstOrDefault(e => true) ?? model;
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                foreach (var erros in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                }
                return PartialView(model);
            }

            foreach (var campos in cParametro.GetType().GetProperties())
            {
                if (!campos.Name.Equals("Sguid")) cParametro.GetType().GetProperty(campos.Name)?.SetValue(cParametro, model.GetType().GetProperty(campos.Name)?.GetValue(model));
            }

            try
            {
                _nParametros.Gravar(cParametro);
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message + Environment.NewLine;
                return PartialView(model);
            }
            ViewData["EditError"] = "Dados Atualizados!";
            return PartialView(cParametro);
        }
    }
}