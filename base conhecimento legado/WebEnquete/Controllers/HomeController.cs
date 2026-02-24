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
    public class HomeController : Controller
    {
        private readonly NPessoas _nPessoas;
        private readonly NBancos _nBancos;
        private readonly NParametros _nParametros;
        private readonly List<SelectListItem> _dropBancos;

        public HomeController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
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

            if (_nParametros == null) _nParametros = new NParametros(sessionDefault);
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
        public ActionResult Existente(string matricula)
        {
            matricula = matricula.Replace(".", "").Replace("-", "");

            if (string.IsNullOrEmpty(matricula))
            {
                return new JsonResult{Data = "",JsonRequestBehavior = JsonRequestBehavior.DenyGet};
            }

            var cPessoas = _nPessoas.RetornarMatriculaBancaria(matricula);

            if (cPessoas == null)
            {
                return new JsonResult{Data = "",JsonRequestBehavior = JsonRequestBehavior.DenyGet};
            }

            return new JsonResult{Data = "",JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpGet]
        public ActionResult Index(string id = "0")
        {
            ViewData["novo"] = id;
            var cPessoa = new CPessoas { DropBANCO = _dropBancos };
            return View(cPessoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cadastro(CPessoas model)
        {
            try
            {
                ViewData["novo"] = "0";

                model.DropBANCO = _dropBancos;

                if (!model.DEACORDO)
                {
                    ViewData["EditError"] = "Favor confirmar os termos de uso.";
                    return PartialView("Index", model);
                }

                if (model.BANCO == null)
                {
                    ViewData["EditError"] = "Instituição Bancária, inválida.";
                    return PartialView("Index", model);
                }

                model.BANCO = _nBancos.RetornarGuid(model.BANCO.Sguid);
                model.MATRICULASINDICATO = model.Sguid;
                model.CPF = model.CPF.Replace(".", "").Replace("-", "");
                model.DATFILIACAO = DateTime.Today;
                model.ATIVO = true;
                model.MATRICULABANCARIA = model.MATRICULABANCARIA.Replace(".", "").Replace("-", "");
                model.NOME = model.NOME.ToUpper();
                //model.ASSOCIADO = true;

                ModelState.Clear();
                TryValidateModel(model);

                if (!ModelState.IsValid)
                {
                    foreach (var erros in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                    }
                    return PartialView("Index", model);
                }

                if (_nPessoas.RetornarCpfcnpj(model.CPF) != null)
                {
                    ViewData["EditError"] = "CPF já cadastrado";
                    return PartialView("Index", model);
                }
                if (_nPessoas.RetornarMatriculaBancaria(model.MATRICULABANCARIA) != null)
                {
                    ViewData["EditError"] = "Matrícula funcional já cadastrado";
                    return PartialView("Index", model);
                }

                model.CPF = model.CPF.Replace(".", "").Replace("-", "").Replace("/", "");
                model.CELULAR = model.CELULAR.Replace("-", "").Replace("-", "").Replace("(", "").Replace(")", "");
                model.MATRICULABANCARIA = model.MATRICULABANCARIA.Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("/", "");

                _nPessoas.Gravar(model, true);

                var cParametros = _nParametros.RetornarTodos().FirstOrDefault(e => true);
                if (cParametros != null && !string.IsNullOrEmpty(cParametros.SMTP))
                {
                    var body = $"Nome associado: {model.NOME}" + Environment.NewLine;
                    body += $"Cpf           : {model.CPF}" + Environment.NewLine;
                    body += $"D. nascimento : {model.DATNASCIMENTO.Value:d}" + Environment.NewLine;
                    body += $"Data cadastro : {model.DATFILIACAO.Value:d}" + Environment.NewLine;
                    var motor = new Email(cParametros.SMTP, cParametros.LOGIN, cParametros.SENHA, cParametros.PORTA, true);
                    motor.Enviar(cParametros.EMAILATIVACOES, "Cadastro no site de votação", body, "", "", null);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
                return PartialView("Index", model);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}