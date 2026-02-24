using System;
using System.Collections.Generic;
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
    public class MeusDadosController : Controller
    {
        private readonly ILoginProvider _loginProviderParam;
        private readonly NPessoas _nPessoas;
        private readonly NBancos _nBancos;
        private readonly List<SelectListItem> _dropBancos;

        public MeusDadosController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
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

            if (_loginProviderParam == null) _loginProviderParam = loginProviderParam;

            if (_nPessoas == null) _nPessoas = new NPessoas(sessionDefault);
            if (_nBancos == null)
            {
                _nBancos = new NBancos(sessionDefault);
                var lBancos = _nBancos.RetornarTodos().OrderBy(e => e.NOME).ToList();
                _dropBancos = new List<SelectListItem>();
                foreach (var banco in lBancos)
                {
                    _dropBancos.Add(new SelectListItem
                    {
                        Value = banco.Sguid,
                        Text = banco.NOME
                    });
                }
            }
        }

        [HttpGet]
        [ValidacaoUsuario]
        public ActionResult Dados(string id = "")
        {
            if (!string.IsNullOrEmpty(id))
            {
                Global.GetInstance.SetAtualizar(true);
            }
            var cPessoa = _loginProviderParam.Pessoa;
            cPessoa.DropBANCO = _dropBancos;
            return View(cPessoa);
        }

        [HttpPost]
        [ValidacaoUsuario]
        [ValidateAntiForgeryToken]
        public ActionResult Dados(CPessoas model)
        {
            try
            {
                model.DropBANCO = _dropBancos;
                ModelState.Clear();
                TryValidateModel(model);

                if (!ModelState.IsValid)
                {
                    foreach (var erros in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                    }
                    return PartialView("Dados", model);
                }

                var cPessoa = _nPessoas.RetornarGuid(model.Sguid);
                model.DTULTIMAATUALIZACAO = DateTime.Today;
                model.CPF = model.CPF.Replace(".", "").Replace("-", "").Replace("/", "");
                model.CELULAR = model.CELULAR.Replace("-", "").Replace("-", "").Replace("(", "").Replace(")", "");
                model.MATRICULABANCARIA = model.MATRICULABANCARIA.Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("/", "");

                if (cPessoa != null)
                {
                    foreach (var campos in cPessoa.GetType().GetProperties())
                    {
                        if (!campos.Name.Equals("Sguid") 
                            && !campos.Name.Equals("PESSOASENQUETES") 
                            && !campos.Name.Equals("LOGENQUETELOGIN")
                            && !campos.Name.Equals("ATIVO")
                            && !campos.Name.Equals("ASSOCIADO")
                            && !campos.Name.Contains("Temp")
                            ) cPessoa.GetType().GetProperty(campos.Name)?.SetValue(cPessoa, model.GetType().GetProperty(campos.Name)?.GetValue(model));
                    }
                }
                else
                {
                    cPessoa = model;
                }

                _nPessoas.Gravar(cPessoa);

            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
                return PartialView(model);
            }

            if (Global.GetInstance.GetAtualizar())
            {
                Global.GetInstance.SetAtualizar(false);
                return Redirect(Url.Action("Index", "EIntegrad"));
            }


            return PartialView(model);
        }
    }
}