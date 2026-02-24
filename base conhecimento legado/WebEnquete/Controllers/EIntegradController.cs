using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Domain;
using Negocios;
using Newtonsoft.Json;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Web.InfraEstrutura;
using Web.Models;

namespace Web.Controllers
{
    public class EIntegradController : Controller
    {
        private readonly ILoginProvider _loginprovider;
        private readonly NEnquetes _nEnquetes;
        private readonly NResultadoEnquetes _nResultadoEnquetes;
        private readonly NPessoas _nPessoas;
        private readonly NBancos _nBancos;
        private readonly NPessoasEnquetes _nPessoasEnquetes;

        public EIntegradController(IResolutionRoot kernelParam, ILoginProvider loginProviderParam)
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

            if (_nEnquetes == null) _nEnquetes = new NEnquetes(sessionDefault);
            if (_nPessoas == null) _nPessoas = new NPessoas(sessionDefault);
            if (_nBancos == null) _nBancos = new NBancos(sessionDefault);
            if (_nPessoasEnquetes == null) _nPessoasEnquetes = new NPessoasEnquetes(sessionDefault);
            if (_nResultadoEnquetes == null) _nResultadoEnquetes = new NResultadoEnquetes(sessionDefault);
        }
        public ActionResult Sair()
        {
            if (!_loginprovider.Autenticado)
            {
                return RedirectToRoute(new { contoller = "Home", action = "index" });
            }
            _loginprovider.Desautenticar();
            return RedirectToRoute(new { contoller = "Home", action = "index" });
        }
        [ValidacaoUsuario]
        public ActionResult Index()
        {
            var cPessoa = _nPessoas.RetornarGuid(_loginprovider.Pessoa.Sguid);
            var lEnquetes = _nEnquetes.RetornarTodos().Where(e => e.BANCO == null || e.BANCO.Sguid.Equals(cPessoa.BANCO.Sguid)).ToList();
            
            foreach (var enquete in lEnquetes)
            {

                var incio = DateTime.Parse(enquete.DATA.ToString("d") + " " + TimeSpan.Parse(enquete.HORAINICIO).ToString("g"));
                var final = DateTime.Parse(enquete.DATARESULTADO.Value.ToString("d") + " " + TimeSpan.Parse(enquete.HORAFINAL).ToString("g"));

                if (!(DateTime.Now >= incio && DateTime.Now <= final) && !enquete.ATIVO)
                {
                    enquete.ATIVO = false;
                }
                else
                {
                    enquete.ATIVO = true;
                }
            }

            var lEnquetesVotadas = lEnquetes.Select(enquete => new EnquetesVotadas { Pessoa = cPessoa, Enquete = enquete, Ativa = enquete.ATIVO, Associados = enquete.ASSOCIADO, Votado = cPessoa.PESSOASENQUETES.Any(e => e.ENQUETE.Sguid.Equals(enquete.Sguid)) }).ToList();

            return View(lEnquetesVotadas);
        }
        [HttpGet]
        [ValidacaoUsuario]
        public ActionResult Responder(string idEnquete)
        {
            var cEnquete = _nEnquetes.RetornarGuid(idEnquete);
            return PartialView(cEnquete);
        }
        [HttpPost]
        [ValidacaoUsuario]
        public ActionResult Responder(FormCollection model)
        {
            var cEnquete = _nEnquetes.RetornarGuid(model["Sguid"]);
            try
            {
                if (cEnquete == null)
                {
                    return PartialView();
                }

                var cPessoas = _loginprovider.Pessoa;

                var respostaPergunta = int.Parse(model[cEnquete.Sguid]);
                var cPessoasenquetes = new CPessoasEnquetes
                {
                    ENQUETE = cEnquete,
                    PESSOA = cPessoas,
                    PERGUNTA = cEnquete.PERGUNTA
                };
                switch (respostaPergunta)
                {
                    case 1:
                        cPessoasenquetes.RESPOSTA01 = 1;
                        break;
                    case 2:
                        cPessoasenquetes.RESPOSTA02 = 1;
                        break;
                    default:
                        cPessoasenquetes.RESPOSTA01 = 0;
                        cPessoasenquetes.RESPOSTA02 = 0;

                        break;
                }

                cPessoas.PESSOASENQUETES.Add(cPessoasenquetes);

                var votou = _nPessoasEnquetes.RetornarVotoPessoaEnquete(cEnquete, cPessoas);

                if (votou == null)
                {
                    _nPessoasEnquetes.Gravar(cPessoasenquetes);
                }

                return PartialView("_Reload");

            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
                return PartialView(cEnquete);
            }

        }
        [HttpGet]
        [ValidacaoUsuario]
        public ActionResult Resultado(string idEnquete)
        {
            var cEnquete = _nEnquetes.RetornarGuid(idEnquete);
            var lResultado = new List<Resultado>();
            var reultadoEnquete = _nResultadoEnquetes.RetornarResultadoenquete(cEnquete);

            var grafico = new Grafico();
            var percentuais = new List<string>();
            grafico.type = "horizontalBar";
            grafico.options = new Options
            {
                maintainAspectRatio = true,
                legend = new Legend { display = true, position = "top" }
            };
            if (!string.IsNullOrEmpty(cEnquete.RESPOSTA01))
            {
                grafico.data.labels.Add(cEnquete.RESPOSTA01);
                percentuais.Add(reultadoEnquete.RESPOSTA01.ToString());
            }
            if (!string.IsNullOrEmpty(cEnquete.RESPOSTA02))
            {
                grafico.data.labels.Add(cEnquete.RESPOSTA02);
                percentuais.Add(reultadoEnquete.RESPOSTA02.ToString());
            }


            var mim = Math.Min(reultadoEnquete.RESPOSTA01, reultadoEnquete.RESPOSTA02);
            var max = Math.Max(reultadoEnquete.RESPOSTA01, reultadoEnquete.RESPOSTA02);

            percentuais.Add((max + mim).ToString());
            percentuais.Add((mim - Math.Truncate((decimal)(mim / 2))).ToString());

            grafico.data.datasets.Add(new Dataset
            {
                backgroundColor = "#4e73df",
                label = "Resultado",
                borderColor = "#4e73df",
                data = percentuais

            });
            var jsonnovo = JsonConvert.SerializeObject(grafico);
            var resultado = new Resultado
            {
                DataResultado = cEnquete.DATARESULTADO,
                JsonGrafico = jsonnovo,
                Titulo = cEnquete.PERGUNTA
            };

            lResultado.Add(resultado);

            return PartialView(lResultado);
        }
        [AllowAnonymous]
        public ActionResult Download(string id)
        {
            try
            {
                var path = $@"c:\Temp\{id.ToLower()}";
                if (!System.IO.File.Exists(path)) return null;
                var conteudo = System.IO.File.ReadAllBytes(path);
                Response.AppendHeader("Content-Disposition", $"inline; filename={id.ToLower()}");
                return File(conteudo, System.Net.Mime.MediaTypeNames.Application.Octet);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}