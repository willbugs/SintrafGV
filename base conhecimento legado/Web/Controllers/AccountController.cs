using System;
using System.Collections.Generic;
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
                if (_loginprovider.Usuario == null)
                {
                    _loginprovider.Desautenticar();
                    msgErro = "Usuário não autenticado!";
                    ModelState.AddModelError("", msgErro);
                    ViewData["EditError"] = msgErro;
                    return View("Login");
                }

                if (_loginprovider.Usuario.USUARIOSEMPRESAS != null && _loginprovider.Usuario != null &&
                    (_loginprovider.Usuario?.USUARIOSEMPRESAS == null && _loginprovider.Usuario.USUARIOSEMPRESAS.Count == 0))
                {
                    msgErro = "Usuário sem empresa definida! Entrar em contato com o administrador!";
                    ModelState.AddModelError("", msgErro);
                    ViewData["EditError"] = msgErro;
                    return View();
                }
                if (_loginprovider.Usuario.USUARIOSEMPRESAS != null && _loginprovider.Usuario.USUARIOSEMPRESAS.Count == 1)
                {
                    loginModelParam.Empresa = _loginprovider.Usuario.USUARIOSEMPRESAS.FirstOrDefault()?.EMPRESA.Sguid;
                    _loginprovider.SEmpresa(loginModelParam, out msgErro);
                }
                if (_loginprovider.Usuario.USUARIOSEMPRESAS != null)
                    ViewData["Empresas"] = _loginprovider.Usuario.USUARIOSEMPRESAS.Select(registro =>
                            new SelectListItem { Text = registro.EMPRESA.NOMEREDUZIDO, Value = registro.EMPRESA.Sguid })
                        .ToList();
                
                return ((List<SelectListItem>)ViewData["Empresas"]).Count.Equals(1)
                    ? (ActionResult)Redirect(Url.Action("Index", "EIntegrad"))
                    : (ActionResult)Redirect(Url.Action("SelecionaEmpresa", "Account"));
            }
            ModelState.AddModelError("", msgErro);
            ViewData["EditError"] = msgErro;
            return View();
        }
        [HttpGet]
        public ActionResult SelecionaEmpresa()
        {
            if (!_loginprovider.Autenticado)
            {
                return Redirect(Url.Action("Login", "Account"));
            }
            var empre = _loginprovider.Usuario.USUARIOSEMPRESAS.Select(registro => new SelectListItem { Text = registro.EMPRESA.NOMEREDUZIDO, Value = registro.EMPRESA.Sguid }).ToList();
            ViewData["Empresas"] = empre;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelecionaEmpresa(LoginModel loginModelParam)
        {
            ModelState.Clear();
            if (_loginprovider.SEmpresa(loginModelParam, out var msgErro))
            {
                return Redirect(Url.Action("Index", "Painel"));
            }
            ModelState.AddModelError("", msgErro);
            ViewData["EditError"] = msgErro;
            ViewData["Empresas"] = _loginprovider.Usuario.USUARIOSEMPRESAS.Select(registro => new SelectListItem { Text = registro.EMPRESA.NOMEREDUZIDO, Value = registro.EMPRESA.Sguid }).ToList();
            ModelState.AddModelError("Empresa", "Empresa Inválida!");
            return View(loginModelParam);
        }
    }
}
