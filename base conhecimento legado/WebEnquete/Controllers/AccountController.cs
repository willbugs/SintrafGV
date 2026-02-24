using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Web.InfraEstrutura;
using Web.Models;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginProvider _loginprovider;

        public AccountController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
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
        }
        public ActionResult Login()
        {
            if (_loginprovider.Autenticado && _loginprovider.Selecionado)
            {
                return Redirect(Url.Action("Index", "EIntegrad"));
            }
            return View();
        }

        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Location = OutputCacheLocation.Client, Duration = 0)]
        [HttpPost]
        public ActionResult Login(LoginModel loginModelParam)
        {
            ModelState.Clear();
            TryValidateModel(loginModelParam);

            if (!ModelState.IsValid)
            {
                foreach (var erros in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                }
                return View(loginModelParam);
            }

            if (_loginprovider.Autenticar(loginModelParam, out var msgErro))
            {
                if (_loginprovider.Pessoa == null)
                {
                    _loginprovider.Desautenticar();
                    msgErro = "Usuário não autenticado!";
                    ModelState.AddModelError("", msgErro);
                    ViewData["EditError"] = msgErro;
                    return View("Login");
                }

                if (_loginprovider.Pessoa?.DTULTIMAATUALIZACAO == null)
                {
                    return Redirect(Url.Action("Dados", "MeusDados", new { id = "login" }));
                }

                if (_loginprovider.Pessoa?.DTULTIMAATUALIZACAO != null)
                {
                    var difdata = DateTime.Today.Date - _loginprovider.Pessoa.DTULTIMAATUALIZACAO.Value;
                    if (difdata.Days > 40) return Redirect(Url.Action("Dados", "MeusDados"));
                }
                return Redirect(Url.Action("Index", "EIntegrad"));
            }
            ModelState.AddModelError("", msgErro);
            ViewData["EditError"] = msgErro;
            return View();
        }
    }
}
