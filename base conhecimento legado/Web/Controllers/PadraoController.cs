using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Domain;
using iTextSharp.text;
using Microsoft.Ajax.Utilities;
using Negocios;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Reports;
using Web.InfraEstrutura;
using Web.Models;

namespace Web.Controllers
{
    public class PadraoController : Controller
    {
        private const string CoDrop = "1";
        private const string CoData = "2";
        private const string CoValor = "3";
        private const string CoDropBox = "5";
        private const string CoCheckBox = "6";
        private const string CoFoto = "7";
        private const string CoChave = "9";
        private const string CoDropBoxMulti = "B";
        private const string CoTime = "C";
        private const string Cofile = "D";
        private static readonly IList<ClasseBase> ListaClasseBase = new List<ClasseBase>();
        private static string _tagcampo;
        private static ISession _sessionDefault;
        private static ILoginProvider _loginProvider;
        private static NTelas _nTelas;
        private static NSubMenus _nSubMenus;
        private static readonly IList<ListaModel> ListaModel = new List<ListaModel>();
        private static NCampos _nCampos;
        private static NRelatorios _nRelatorios;
        private static readonly FiltroModel FiltroModel = new FiltroModel();
        private static List<SelectListItem> _lLista;
        private static CTelas _telaEmUso;
        private static readonly List<string> Listaselecao = new List<string>();
        private static NGrupoUsuarios _nGrupoUsuarios;
        private static IResolutionRoot _kernelParam;
        private static bool _newRecord;
        public readonly string SessionId;
        public List<RelatorioAssociados> _listaCampos;

        public PadraoController(IResolutionRoot kernelParam, ILoginProvider loginproviderParam)
        {
            _kernelParam = kernelParam;
            _sessionDefault = kernelParam.Get<ISession>("Default");
            _loginProvider = loginproviderParam;
            _nSubMenus = new NSubMenus(_sessionDefault);
            _nTelas = new NTelas(_sessionDefault);
            _nCampos = new NCampos(_sessionDefault);
            _nGrupoUsuarios = new NGrupoUsuarios(_sessionDefault);
            _nRelatorios = new NRelatorios(_sessionDefault);
            SessionId = System.Web.HttpContext.Current.Session.SessionID;
        }
        [Obsolete("Obsolete")]
        private static void _restart_session()
        {
            _sessionDefault.Close();
            _kernelParam.Release(_kernelParam.Get<ISession>("Default"));
            DependencyResolver.SetResolver(new NinjectDependencyResolver(HttpRuntime.AppDomainAppVirtualPath));
        }
        private static string Limpar(string chave)
        {
            const string lixo = ".-/()";
            return lixo.Aggregate(chave, (current, t) => current.Replace(t.ToString(CultureInfo.InvariantCulture), ""));
        }
        private static dynamic NewNegocio(string nclasse)
        {
            nclasse = "Negocios.N" + nclasse;
            var assem = typeof(NBase<CBase>).Assembly;
            var type = assem.GetType(nclasse);
            var newclassimport = Activator.CreateInstance(type, _sessionDefault);
            return newclassimport;
        }
        private static dynamic NewReport(string nclasse)
        {
            nclasse = "Reports.R" + nclasse;
            var assem = typeof(RBaseReport).Assembly;
            var type = assem.GetType(nclasse);
            var newclassimport = Activator.CreateInstance(type);
            return newclassimport;
        }
        private dynamic NewClass(string nclasse, CTelas tela)
        {
            nclasse = "Domain.C" + nclasse.Trim();
            var assem = typeof(CBase).Assembly;
            var type = assem.GetType(nclasse);
            var newclassimport = Activator.CreateInstance(type);
            newclassimport.GetType().GetProperty("Usuario")?.SetValue(newclassimport, _loginProvider.Usuario, null);
            if (newclassimport.GetType().GetProperty("EMPRESA") != null) newclassimport.GetType().GetProperty("EMPRESA")?.SetValue(newclassimport, _loginProvider.Empresa, null);
            NewDropList(newclassimport, tela);
            foreach (var itens in tela.Campos.Where(l => l.Tipo == CoChave).Where(itens => !ListaModel.Any(l => l.Secao.Equals(SessionId) && l.CampoClasse == itens.Source && l.Classe == tela.Parent)))
            {
                ListaModel.Add(new ListaModel()
                {
                    Secao = SessionId,
                    CampoClasse = itens.Source,
                    Campo = itens.Listar,
                    Classe = tela.Parent,
                    IdCampo = itens.Tipo,
                    CampoSource = itens.CampoSource,
                    Descricao = itens.Descricao.IsEmpty() ? "" : itens.Descricao
                });
            }
            return newclassimport;
        }
        private void NewDropList(dynamic classBase, CTelas telas)
        {
            ViewBag.Telas = telas;
            foreach (var itens in telas.Campos.Where(l => l.Tipo == CoDrop || l.Tipo == CoDropBox || l.Tipo == CoDropBoxMulti))
            {
                switch (itens.Tipo)
                {
                    case CoDrop:
                        var modelbase = classBase.GetType().GetProperty("Drop" + itens.Campo).GetValue(classBase, null);
                        var xconteudo = classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null) == null ? "" : classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null).ToString();
                        if (xconteudo != null)
                        {
                            if (classBase.GetType().GetProperty(itens.Campo).PropertyType.IsEnum)
                            {
                                xconteudo = ((int)classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null)).ToString(CultureInfo.InvariantCulture);
                            }
                            foreach (var item in modelbase)
                            {
                                item.Selected = false;
                                if (item.Value == xconteudo)
                                {
                                    item.Selected = true;
                                }
                            }
                        }
                        ViewData[itens.Campo] = modelbase;
                        break;
                    case CoDropBox:
                        var propertyInfo = classBase.GetType().GetProperty("Drop" + itens.Campo);
                        var conteudo = classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null) == null
                            ? ""
                            : classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null).Sguid;
                        var lista = new List<SelectListItem>();
                        foreach (var registro in NewNegocio(itens.Source).RetornarTodos(_loginProvider.Empresa))
                        {
                            if (classBase.GetType().GetProperty("EMPRESA") != null &&
                                classBase.GetType().GetProperty("EMPRESA").GetValue(classBase, null) ==
                                _loginProvider.Empresa)
                                lista.Add(new SelectListItem
                                {
                                    Text = registro.GetType().GetProperty(itens.Listar).GetValue(registro, null).ToString(),
                                    Value = registro.Sguid,
                                    Selected = conteudo == registro.Sguid
                                });
                            else
                                lista.Add(new SelectListItem
                                {
                                    Text = registro.GetType().GetProperty(itens.Listar).GetValue(registro, null).ToString(),
                                    Value = registro.Sguid,
                                    Selected = conteudo == registro.Sguid
                                });
                        }
                        lista.Sort((x, y) => string.Compare(x.Text, y.Text, StringComparison.Ordinal));
                        propertyInfo.SetValue(classBase, lista, null);
                        ViewData[itens.Campo] = classBase.GetType()
                            .GetProperty("Drop" + itens.Campo)
                            .GetValue(classBase, null);
                        break;
                    case CoDropBoxMulti:
                        var propertyInfoMulti = classBase.GetType().GetProperty("Multi" + itens.Campo);
                        var conteudoMulti = classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null) == null
                            ? ""
                            : classBase.GetType().GetProperty(itens.Campo).GetValue(classBase, null).Sguid;
                        var listaMulti = new List<SelectListItem>();
                        foreach (var registro in NewNegocio(itens.Source).RetornarTodos(_loginProvider.Empresa))
                        {
                            listaMulti.Add(new SelectListItem
                            {
                                Text = registro.GetType().GetProperty(itens.Listar).GetValue(registro, null).ToString(),
                                Value = registro.Sguid,
                                Selected = conteudoMulti == registro.Sguid
                            });
                        }
                        listaMulti.Sort((x, y) => string.Compare(x.Text, y.Text, StringComparison.Ordinal));
                        propertyInfoMulti.SetValue(classBase, listaMulti, null);
                        ViewData[itens.Campo] = propertyInfoMulti.GetValue(classBase, null);
                        break;
                }
            }
        }
        private dynamic SetCampos(string nclasse, CTelas telas, NameValueCollection model, int adic = 0)
        {
            var newclassimport = NewClass(nclasse, telas);
            if (adic == 0 && telas.Lista == 0)
            {
                newclassimport = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase;
            }
            else
            {
                PropertyInfo propertyInfoSguid = newclassimport.GetType().GetProperty("Sguid");
                if (propertyInfoSguid != null && model["Sguid"] != null)
                {
                    var sGuid = model["Sguid"].Substring(0, 36);
                    propertyInfoSguid.SetValue(newclassimport, null, null);
                    propertyInfoSguid.SetValue(newclassimport, sGuid, null);
                }
            }

            foreach (var itens in telas.Campos.Where(l => !l.Campo.ToUpper().Equals("USUARIO") && !l.Campo.Equals("Sguid")))
            {
                PropertyInfo propertyInfo = newclassimport.GetType().GetProperty(itens.Campo);
                switch (itens.Tipo)
                {
                    case CoDropBoxMulti:
                        var boxConteudo = model[itens.Campo];
                        propertyInfo = newclassimport.GetType().GetProperty("MResult" + itens.Campo);
                        var listBoxConteudo = boxConteudo == "" ? new List<string>() : boxConteudo.Split('.').ToList();
                        propertyInfo.SetValue(newclassimport, listBoxConteudo, null);
                        break;
                    case CoDropBox:
                        if (model[itens.Campo] != null)
                        {
                            var limpo = model[itens.Campo].Replace(",", "");
                            propertyInfo.SetValue(newclassimport, NewNegocio(itens.Source).RetornarGuid(limpo), null);
                        }
                        break;
                    case CoChave:
                        propertyInfo.SetValue(newclassimport, ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, null);
                        break;
                    case CoData:
                        try
                        {
                            propertyInfo.SetValue(newclassimport, model[itens.Campo].IsEmpty() ? null : new DateTime?(DateTime.Parse(model[itens.Campo])), null);
                        }
                        catch
                        {
                            propertyInfo.SetValue(newclassimport, DateTime.Now, null);
                        }
                        break;
                    case CoValor:
                        propertyInfo.SetValue(newclassimport, model[itens.Campo].IsEmpty() ? 0 : Convert.ChangeType(model[itens.Campo], propertyInfo.PropertyType), null);
                        break;
                    case CoFoto:
                        propertyInfo = newclassimport.GetType().GetProperty("TempFoto");
                        propertyInfo.SetValue(newclassimport, model["TempFoto"].IsEmpty() ? "" : Convert.ChangeType(model["TempFoto"], propertyInfo.PropertyType), null);
                        break;
                    case CoCheckBox:
                        var resultado = model[itens.Campo].Split(',');
                        var checkconteudo = "0";
                        if (propertyInfo.PropertyType != typeof(bool))
                        {
                            if (resultado[0].ToUpper() == "TRUE") checkconteudo = "1";
                        }
                        else
                        {
                            checkconteudo = resultado[0];
                        }
                        propertyInfo.SetValue(newclassimport, checkconteudo.IsEmpty() ? "" : Convert.ChangeType(checkconteudo, propertyInfo.PropertyType), null);
                        break;
                    case Cofile:
                        var file = model[itens.Campo];
                        break;
                    default:
                        var conteudo = model[itens.Campo];
                        if (conteudo != null)
                        {
                            if (itens.Campo.ToUpper() == "CPF" || itens.Campo.ToUpper() == "CNPJ" || itens.Campo.ToUpper() == "CPFCNPJ" || itens.Campo.ToUpper() == "CNPJCPF" ||
                                itens.Campo.ToUpper() == "TELEFONE" || itens.Campo.ToUpper() == "EMITENTE_FONE" || itens.Campo.ToUpper() == "CARTAO")
                            {
                                conteudo = Limpar(conteudo);
                            }
                            if (itens.Campo.ToUpper() != "TOKEN" && itens.Campo.ToUpper() != "CODATIVACAO" && itens.Campo.ToUpper() != "SENHA" && itens.Campo.ToUpper() != "EMAIL" && itens.Campo.ToUpper() != "SGUID" && !propertyInfo.PropertyType.IsEnum)
                            {
                                conteudo = conteudo.ToUpper();
                            }
                        }
                        if (itens.Campo.ToUpper() == "EMPRESA")
                        {
                            var addempresa = newclassimport.GetType().GetMethod("AddEmpresa");
                            if (addempresa != null) addempresa.Invoke(newclassimport, new object[] { _loginProvider.Empresa });
                        }
                        else
                        {
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                if (!string.IsNullOrWhiteSpace(conteudo))
                                    propertyInfo.SetValue(newclassimport, Enum.Parse(propertyInfo.PropertyType, conteudo));
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(conteudo))
                                    propertyInfo.SetValue(newclassimport, conteudo.IsEmpty() ? "" : Convert.ChangeType(conteudo, propertyInfo.PropertyType), null);
                            }
                        }
                        break;
                }
            }
            return newclassimport;
        }
        [HttpGet]
        public JsonResult ComboFuncion(string campo, string tela, string id)
        {
            tela = tela.Replace("#form", "");
            var dataRet = "";

            if (tela.Equals("Despesas"))
            {

            }

            return Json(dataRet, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult CarregarPagina(string campo, string tela, string id)
        {
            tela = tela.Replace("#form", "");
            var dataRet = "";

            //if (tela.Equals("Pessoas"))
            //{

            //}

            return Json(dataRet, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Importar(string id)
        {
            var tela = _nTelas.RetornarGuid(id);
            return PartialView("_Importar", tela);
        }
        public ContentResult UploadFiles(string id)
        {
            var tela = _nTelas.RetornarGuid(id);
            var r = new List<UploadFilesResult>();

            foreach (var hpf in Request.Files.Cast<string>().Select(file => Request.Files[file]).Where(hpf => hpf == null || hpf.ContentLength != 0))
            {
                var conteudotxt = new StreamReader(hpf.InputStream).ReadToEnd();

                r.Add(new UploadFilesResult()
                {
                    Name = hpf.FileName,
                    Length = hpf.ContentLength,
                    Type = hpf.ContentType,
                    Erro = ""
                });

                switch (tela.Help)
                {
                    case "NotasFiscais":
                        break;
                }
            }
            return Content("{\"name\":\"" + r[0].Name + "\",\"error\":\"" + r[0].Erro + "\"}", "application/json");
        }
        [HttpGet]
        public JsonResult Selecionar(string id, bool marcado)
        {
            var regex = new Regex(@"^[0-9|a-zA-Z'.)(,-: /\\-s]{0,}$");
            if (!regex.IsMatch(id))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            if (marcado)
            {
                Listaselecao.Add(id);
            }
            else
            {
                Listaselecao.Remove(id);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Index(string id)
        {
            var sb = _nSubMenus.RetornarGuid(id);
            ViewBag.NomeTela = sb.NomeTela;
            ViewBag.Carregamento = true;
            return PartialView("_Index", sb.Tela);
        }
        [HttpGet]
        public ActionResult Novo(string id)
        {
            var sb = _nSubMenus.RetornarGuid(id);
            ViewBag.NomeTela = sb.NomeTela;
            ViewBag.Carregamento = false;
            var parent = sb.Tela.First(e => e.SubMenu.Sguid.Equals(sb.Sguid)).Parent;
            if (ListaModel.Any(l => l.Secao.Equals(SessionId) && l.Classe.Equals(parent)))
            {
                var remover = ListaModel.First(l => l.Secao.Equals(SessionId) && l.Classe.Equals(parent));
                if (remover != null) ListaModel.Remove(remover);
            }
            _newRecord = true;
            return PartialView("_Index", sb.Tela);
        }
        [HttpGet]
        public ActionResult Filtros(string id, FiltroModel model, string currentFilter, int source = 0)
        {
            ViewBag.Campo = _tagcampo;
            var viewretorno = "";
            switch (source)
            {
                case 0:
                    viewretorno = "_Consultas";
                    break;
                case 1:
                    viewretorno = "_Pesquisar";
                    break;
                case 2:
                    viewretorno = "_Relatorios";
                    break;
            }
            ModelState.Clear();
            TryValidateModel(model);
            if (!ModelState.IsValid)
            {
                foreach (var erros in ModelState.Values.SelectMany(erro => erro.Errors))
                {
                    ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                }

                ViewBag.Dados = new List<CBase>();
                return PartialView(viewretorno, model);
            }
            var sb = _nSubMenus.RetornarGuid(id).Tela.FirstOrDefault(t => t.Lista == 0);
            ViewBag.Telas = sb;
            model.Ordens = FiltroModel.Ordens;
            if (model.Ordem == null)
            {
                return PartialView(viewretorno, model);
            }
            var dados = model.Ordem.Split('.');
            var negocio = NewNegocio(dados[0]);
            var buscamethod = negocio.GetType().GetMethod(dados[1]);
            if (model.Chavebusca != null)
            {
                if (dados[2] == "mDATA")
                {
                    if (!DateTime.TryParse(model.Chavebusca, out _))
                    {
                        ViewData["EditError"] = "Data inválida!";
                        ViewBag.Dados = new List<CBase>();
                        return PartialView(viewretorno, model);
                    }
                }

                object[] parametros;
                var contador = 0;
                var methodparam = buscamethod.GetParameters();
                if (methodparam != null)
                {
                    foreach (var unused in methodparam)
                    {
                        contador++;
                    }
                }
                if (contador > 1)
                {
                    parametros = new object[]
                        {dados[2] == "mDATA" ? model.Chavebusca : Limpar(model.Chavebusca), _loginProvider.Empresa};
                }
                else
                {
                    parametros = new object[] { dados[2] == "mDATA" ? model.Chavebusca : Limpar(model.Chavebusca) };
                }
                var vdados = buscamethod.Invoke(negocio, parametros);
                if (!vdados.Count.Equals(0))
                {
                    model.Registros = 1;
                }
                model.Registros = 1;
                ViewBag.Dados = vdados;
            }
            else if (sb != null)
            {
                var negociobase = NewNegocio(sb.Help);
                var todos = negociobase.RetornarTodos(_loginProvider.Empresa);
                if (!todos.Count.Equals(0))
                {
                    model.Registros = 1;
                }
                ViewBag.Dados = todos;
            }
            return PartialView(viewretorno, model);
        }
        public ActionResult Consultas(string id)
        {
            var tempListaClasseBase = ListaClasseBase.Where(e => e.Secao.Equals(SessionId)).ToList();
            foreach (var remover in tempListaClasseBase)
            {
                ListaClasseBase.Remove(remover);
            }
            var sb = _nSubMenus.RetornarGuid(id).Tela.FirstOrDefault(t => t.Lista == 0);
            var lcampos = _nCampos.RetornarCamposConsultaveisPorSubmenu(_nTelas.RetornarTelasPorSubmenu(sb.SubMenu));
            ViewBag.Telas = sb;
            FiltroModel.Ordens.Clear();
            foreach (var campos in lcampos)
            {
                FiltroModel.Ordens.Add(new SelectListItem { Text = campos.Descricao, Value = campos.Tela.Help + "." + campos.FuncaoBusca + "." + campos.Mascara });
            }
            ViewBag.Dados = new List<CBase>();
            return PartialView("_Consultas", FiltroModel);
        }
        [HttpGet]
        public ActionResult Delete(string id, string sguid)
        {
            ModelState.Clear();
            var sb = _nSubMenus.RetornarGuid(id);
            var tela = sb.Tela.FirstOrDefault(t => t.Lista == 0);
            if (tela == null) return PartialView("_Index", sb.Tela);
            ViewBag.NomeTela = tela.NomeTela;
            ViewBag.Carregamento = true;
            try
            {
                var negocio = NewNegocio(tela.Help);
                negocio.Excluir(negocio.RetornarGuid(sguid));
            }
            catch (Exception e)
            {
                ViewData["EditError"] = " " + e.Message;
                //_restart_session();
            }
            return PartialView("_Index", sb.Tela);
        }
        public ActionResult Deleta(string id, string nclasse, string idtela)
        {
            var cTelas = _nTelas.RetornarGuid(idtela);
            var newclasse = NewClass(nclasse, cTelas);
            if (ListaModel.Count(l => l.Secao.Equals(SessionId) && l.Classe == cTelas.Parent) > 0)
            {
                ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == cTelas.Parent)?
                    .Lista.Remove(ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == cTelas.Parent)?.
                        Lista.FirstOrDefault(l => l.Sguid == id));
            }
            return PartialView("_Cadastros", newclasse);
        }
        public ActionResult ListaItens(string id)
        {
            var tela = _nTelas.RetornarGuid(id);
            var lista = ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == tela.Parent && l.IdCampo == CoChave);
            if (lista?.Lista != null)
            { lista.Lista = lista.Lista.OrderBy(o => o.GetType().GetProperty(lista.Campo)?.GetValue(o, null)).ToList(); }

            ViewBag.Telas = tela;
            ViewBag.NomeItemTela = tela.Parent;
            ViewBag.Lista = lista;
            ViewBag.Campos = _nCampos.RetornarTodosTela(tela.Sguid);
            return PartialView("_ListaItens", (List<CBase>)lista?.Lista);
        }
        [HttpGet]
        public ActionResult Editar(string id, string sguid)
        {
            var sb = _nSubMenus.RetornarGuid(id);
            ViewBag.NomeTela = sb.NomeTela;
            ViewBag.aEditar = sguid;
            ViewBag.Carregamento = false;
            var parent = sb.Tela.First(e => e.SubMenu.Sguid.Equals(sb.Sguid)).Parent;
            if (ListaModel.Any(l => l.Secao.Equals(SessionId) && l.Classe.Equals(parent)))
            {
                var remover = ListaModel.First(l => l.Secao.Equals(SessionId) && l.Classe.Equals(parent));
                if (remover != null) ListaModel.Remove(remover);
            }
            _newRecord = false;
            return PartialView("_Index", sb.Tela);
        }
        [HttpGet]
        public JsonResult GrupoUsuario(string id)
        {
            var submenuspacote = _nGrupoUsuarios.RetornarIdgrupo(id).FirstOrDefault();
            var addmethod = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase.GetType().GetMethod("AddUsuariosPermissoes");
            var clearmethod = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase.GetType().GetMethod("LimparPermissoes");

            if (clearmethod != null)
            {
                clearmethod.Invoke(ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, null);
            }

            if (submenuspacote != null)
                foreach (var permissao in submenuspacote.GRUPOUSUARIOSPERMISSOES.Select(item =>
                    new CUsuariosPermissoes() { Submenus = item.Submenus, Ativo = item.Ativo }))
                {
                    if (addmethod != null) addmethod.Invoke(ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, new object[] { permissao });
                }

            const string tela = "UsuariosPermissoes";
            var xlista = ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == tela && l.IdCampo == "9")?.Lista;
            dynamic newClass = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase.GetType().GetProperty(tela.ToUpper())?.GetValue(ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, null);

            if (newClass == null) return Json("", JsonRequestBehavior.AllowGet);
            {
                foreach (var item in newClass)
                {
                    if (xlista == null) continue;
                    xlista.Remove(item as CBase);
                    xlista.Add(item as CBase);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Cadastro(string id, string nclasse, string sguid)
        {
            var cTela = _nTelas.RetornarGuid(id);
            var newClass = NewClass(nclasse, cTela);

            if (!sguid.IsNullOrWhiteSpace())
            {
                newClass = NewNegocio(cTela.Help).RetornarGuid(sguid);
                if (newClass != null)
                {
                    NewDropList(newClass, cTela);
                }
            }

            if (cTela.Lista == 0)
            {
                ListaClasseBase.Add(new ClasseBase { Secao = SessionId, CBase = newClass });
                _telaEmUso = cTela;
            }

            if (nclasse == "GrupoUsuarios")
            {
                var submenuspacote = _nSubMenus.RetornarTodos();
                if (newClass != null)
                {
                    var addmethod = newClass.GetType().GetMethod("AddGrupoUsuariosPermissoes");
                    foreach (var permissao in submenuspacote.Select(item => new CGrupoUsuariosPermissoes
                    {
                        Submenus = item,
                        Proprietario = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase as CGrupoUsuarios
                    }))
                    {
                        addmethod.Invoke(newClass, new object[] { permissao });
                    }
                }
            }
            if (nclasse != "GrupoUsuariosPermissoes" && (cTela.Lista != 1 || sguid.IsNullOrWhiteSpace())) { return PartialView("_Cadastros", newClass); }
            var first = cTela.Campos.FirstOrDefault(x => x.Tipo == CoChave);
            var xlista = ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == cTela.Parent && l.IdCampo == first.Tipo).Lista;
            if (ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase != null)
            {
                newClass = ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase.GetType().GetProperty(cTela.Parent.ToUpper())?.GetValue(ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, null);
            }

            if (newClass != null)
            {
                xlista?.Clear();
                foreach (var item in newClass)
                {
                    xlista?.Add(item as CBase);
                }
            }

            newClass = NewClass(nclasse, cTela);
            return PartialView("_Cadastros", newClass);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cadastro(FormCollection model, HttpPostedFileBase ARQUIVO, string id, string nclasse)
        {
            if (ARQUIVO != null)
            {
                var path = "C:\\Temp\\" + ARQUIVO.FileName;
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                ARQUIVO.SaveAs(path);
                model["ARQUIVOANEXO"] = ARQUIVO.FileName.ToLower();
            }

            var telas = _nTelas.RetornarGuid(id);
            var newclassimport = SetCampos(nclasse, telas, model);
            try
            {
                ViewData["EditError"] = "";
                if (telas.Lista == 0)
                {
                    var clearmethod = newclassimport.GetType().GetMethod("LimparListas");
                    if (clearmethod != null)
                    {
                        clearmethod.Invoke(newclassimport, null);
                    }
                    foreach (var listaModel in ListaModel.Where(l => l.Secao.Equals(SessionId) && l.Lista.Count > 0))
                    {
                        var fulltelas = _nTelas.RetornarTodos().FirstOrDefault(l => l.Parent == listaModel.Classe);
                        if (fulltelas == null) continue;
                        var addmethod = newclassimport.GetType().GetMethod("Add" + fulltelas.Parent);
                        if (addmethod == null) continue;
                        foreach (var item in listaModel.Lista)
                        {
                            addmethod.Invoke(newclassimport, new object[] { item });
                        }
                    }
                    var beforepostmethod = newclassimport.GetType().GetMethod("BeforePost");
                    if (beforepostmethod != null)
                    {
                        var beforepostparam = beforepostmethod.GetParameters();
                        if (beforepostparam != null)
                        {
                            var contador = 0;
                            foreach (ParameterInfo param in beforepostparam)
                            {
                                contador++;
                            }
                            if (contador >= 1)
                            {
                                var inparameters = new object[contador];
                                contador = 0;
                                foreach (ParameterInfo param in beforepostparam)
                                {
                                    inparameters[contador++] = NewNegocio(param.Name).RetornarTodos(_loginProvider.Empresa);
                                }
                                beforepostmethod.Invoke(newclassimport, inparameters);
                            }
                            else beforepostmethod.Invoke(newclassimport, null);
                        }
                        else
                        {
                            beforepostmethod.Invoke(newclassimport, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = " " + e.Message;
                return PartialView("_Cadastros", newclassimport);
            }
            try
            {
                NewDropList(newclassimport, telas);
                ModelState.Clear();
                TryValidateModel(newclassimport);
                if (!ModelState.IsValid)
                {
                    foreach (var erros in ModelState.Values.SelectMany(erro => erro.Errors))
                    {
                        ViewData["EditError"] = ViewData["EditError"] + erros.ErrorMessage + Environment.NewLine;
                    }
                    return PartialView("_Cadastros", newclassimport);
                }
                if (telas.Seleciona == 1)
                {
                    var negociobase = NewNegocio(nclasse);
                    foreach (var objetoacertalterado in Listaselecao.Select(sguid => negociobase.RetornarGuid(sguid.Trim())))
                    {
                        foreach (var campoalvo in telas.Campos.Where(c => c.Visivel == 1).Select(campos => newclassimport.GetType().GetProperty(campos.Campo)).Where(proper => proper.GetValue(newclassimport, null) != null))
                        {
                            objetoacertalterado.GetType().GetProperty(campoalvo.Name).SetValue(objetoacertalterado, campoalvo.GetValue(newclassimport, null), null);
                        }
                        negociobase.Gravar(objetoacertalterado, _newRecord);
                    }
                    ViewBag.NomeTela = telas.SubMenu.NomeTela;
                    ViewBag.Dados = NewNegocio(telas.Help).RetornarTodos(_loginProvider.Empresa);
                    ViewBag.Carregamento = true;
                    return PartialView("_Reload", telas.SubMenu);
                }

                if (telas.Lista == 0)
                {
                    NewNegocio(nclasse).Gravar(newclassimport, _newRecord);
                    var afterpostmethod = newclassimport.GetType().GetMethod("AfterPost");
                    if (afterpostmethod != null)
                    {
                        afterpostmethod.Invoke(newclassimport, null);
                    }
                    var tempListaModel = ListaModel.Where(l => l.Secao.Equals(SessionId) && l.Classe.Equals(telas.Parent)).ToList();
                    foreach (var remover in tempListaModel)
                    {
                        ListaModel.Remove(remover);
                    }
                    var tempListaClasseBase = ListaClasseBase.Where(e => e.Secao.Equals(SessionId)).ToList();
                    foreach (var remover in tempListaClasseBase)
                    {
                        ListaClasseBase.Remove(remover);
                    }
                    ViewBag.NomeTela = telas.SubMenu.NomeTela;
                    ViewBag.Dados = NewNegocio(telas.Help).RetornarTodos(_loginProvider.Empresa);
                    ViewBag.Carregamento = true;
                    return PartialView("_Reload", telas.SubMenu);
                }
                var first = telas.Campos.FirstOrDefault(x => x.Tipo == CoChave);
                var campounico = "";
                var cCampos = telas.Campos.FirstOrDefault(x => x.Unico == 1);
                if (cCampos != null) campounico = cCampos.Campo;
                var xlista = ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == telas.Parent && l.IdCampo == first.Tipo).Lista;
                var marcado = false;
                foreach (var itemlista in xlista.Where(l => l.Sguid == (newclassimport as CBase)?.Sguid))
                {
                    foreach (var prop in itemlista.GetType().GetProperties().Where(l => !l.Name.Contains("Temp")))
                    {
                        prop.SetValue(itemlista, newclassimport.GetType().GetProperty(prop.Name).GetValue(newclassimport, null));
                        marcado = true;
                    }
                }
                if (marcado) return PartialView("_Cadastros", NewClass(telas.Help, telas));
                if (campounico != "")
                {
                    var remover = xlista.FirstOrDefault(itens => newclassimport.GetType().GetProperty(campounico).GetValue(newclassimport, null) == itens.GetType().GetProperty(campounico)?.GetValue(itens, null));
                    if (remover != null)
                    {
                        xlista.Remove(remover);
                    }
                }
                xlista.Add(newclassimport as CBase);
                return PartialView("_Cadastros", NewClass(telas.Help, telas));
            }
            catch (Exception e)
            {
                ViewData["EditError"] = " " + e.Message;
            }
            return PartialView("_Cadastros", newclassimport);
        }
        public ActionResult Seleciona(string id, string nclasse, string idtela)
        {
            var cTelas = _nTelas.RetornarGuid(idtela);
            ViewBag.Telas = cTelas;
            var updateclasse = ListaModel.FirstOrDefault(l => l.Secao.Equals(SessionId) && l.Classe == cTelas.Parent)?.Lista.FirstOrDefault(l => l.Sguid == id);
            NewDropList(updateclasse, cTelas);

            return PartialView("_Cadastros", updateclasse);
        }
        [HttpGet]
        public ActionResult Adicionar(string id, string sCampo)
        {
            var campo = _nCampos.RetornarGuid(id);
            var tela = _nTelas.RetornarTelasPorNome(campo.Source);
            var newclassimport = NewClass(campo.Source, tela);

            ViewBag.aCampo = sCampo;
            ViewBag.aTela = tela;

            return PartialView("_Adicionar", newclassimport);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adicionar(FormCollection model, string id, string nclasse, string sCampo)
        {
            var telas = _nTelas.RetornarGuid(id);
            var newclassimport = SetCampos(nclasse, telas, model, 1);

            ViewBag.aTela = telas;
            ViewBag.aCampo = sCampo;

            try
            {
                ModelState.Clear();
                TryValidateModel(newclassimport);

                if (!ModelState.IsValid)
                {
                    foreach (var erros in from itemModelState in ModelState where telas.Campos.Count(l => String.Equals(l.Campo, itemModelState.Key, StringComparison.CurrentCultureIgnoreCase)) == 0 from erros in itemModelState.Value.Errors select erros)
                    {
                        ViewData["EditError"] = ViewData["EditError"] + Environment.NewLine + " * " + erros.ErrorMessage;
                    }
                    return PartialView("_cAdicionar", newclassimport);
                }

                NewNegocio(nclasse).Gravar(newclassimport, _newRecord);
                NewDropList(ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase, _telaEmUso);
                _lLista = (List<SelectListItem>)ViewData[sCampo];

                return PartialView("_Reload1", ListaClasseBase.First(e => e.Secao.Equals(SessionId)).CBase);
            }
            catch (Exception e)
            {
                ViewData["EditError"] = " " + e.Message;
            }
            return PartialView("_cAdicionar", newclassimport);
        }
        public JsonResult GetSelectList()
        {
            var objList = new[] { new { Text = "", Value = "" } }.ToList();
            foreach (var selectListItem in _lLista)
            {
                objList.Add(new { selectListItem.Text, selectListItem.Value });
            }
            return Json(objList, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Pesquisar(string id)
        {
            var campo = _nCampos.RetornarGuid(id);
            var tela = _nTelas.RetornarTelasPorHelp(campo.Source);
            var sb = _nSubMenus.RetornarGuid(tela.SubMenu.Sguid).Tela.FirstOrDefault(t => t.Lista == 0);
            var lcampos = _nCampos.RetornarCamposConsultaveisPorSubmenu(_nTelas.RetornarTelasPorSubmenu(sb?.SubMenu));
            ViewBag.Telas = sb;
            _tagcampo = campo.Campo;
            ViewBag.Campo = _tagcampo;
            FiltroModel.Ordens.Clear();
            foreach (var campos in lcampos)
            {
                FiltroModel.Ordens.Add(new SelectListItem { Text = campos.Descricao, Value = campos.Tela.Help + "." + campos.FuncaoBusca + "." + campos.Mascara });
            }
            ViewBag.Dados = new List<CBase>();
            return PartialView("_Pesquisar", FiltroModel);
        }

        private void CriarLista()
        {
            _listaCampos = new List<RelatorioAssociados>();

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "NOME",
                TipoCampo = "C",
                TituloCampo = "Nome",
                Ordem = 1,
                Width = 90F,
                Totalizar = false,
                Visivel = true,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CPF",
                TipoCampo = "C",
                TituloCampo = "CPF",
                Ordem = 2,
                Width = 30F,
                Totalizar = false,
                Visivel = true,
                Filtro = "",
                Mascara = "mCPF",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "MATRICULABANCARIA",
                TipoCampo = "C",
                TituloCampo = "M.Funcional",
                Ordem = 3,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mSINDICATO",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "MATRICULASINDICATO",
                TipoCampo = "C",
                TituloCampo = "Matr.Sind",
                Ordem = 4,
                Width = 25F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });


            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CELULAR",
                TipoCampo = "C",
                TituloCampo = "Celular",
                Ordem = 5,
                Width = 30F,
                Totalizar = false,
                Visivel = true,
                Filtro = "",
                Mascara = "mTELEFONE",
                Tipo = typeof(string)
            });
            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "BANCO.NOME",
                TipoCampo = "C",
                TituloCampo = "Instituição",
                Ordem = 6,
                Width = 60F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string),
                Lookup = true,
                LookupCampo = "BANCO"
            });
            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_CENTER,
                Currency = false,
                NomeCampo = "DATFILIACAO",
                TipoCampo = "D",
                TituloCampo = "D.filiação",
                Ordem = 7,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mDATA",
                Tipo = typeof(DateTime)
            });
            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CARTEIRINHA",
                TipoCampo = "C",
                TituloCampo = "Carteirinha",
                Ordem = 8,
                Width = 15F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });
            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "ESTADOCIVIL",
                TipoCampo = "C",
                TituloCampo = "Estado civil",
                Ordem = 9,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "ENDERECO",
                TipoCampo = "C",
                TituloCampo = "Endereco",
                Ordem = 10,
                Width = 60F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "BAIRRO",
                TipoCampo = "C",
                TituloCampo = "Bairro",
                Ordem = 11,
                Width = 60F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CIDADE",
                TipoCampo = "C",
                TituloCampo = "Cidade",
                Ordem = 12,
                Width = 60F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "ESTADO",
                TipoCampo = "C",
                TituloCampo = "Estado",
                Ordem = 13,
                Width = 15F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "NATURALIDADE",
                TipoCampo = "C",
                TituloCampo = "Naturalidade",
                Ordem = 14,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "DATNASCIMENTO",
                TipoCampo = "D",
                TituloCampo = "D.Nascimento",
                Ordem = 15,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mDATA",
                Tipo = typeof(DateTime)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CODAGENCIA",
                TipoCampo = "C",
                TituloCampo = "Cod Agência",
                Ordem = 16,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "AGENCIA",
                TipoCampo = "C",
                TituloCampo = "Agência",
                Ordem = 17,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CONTA",
                TipoCampo = "C",
                TituloCampo = "Conta",
                Ordem = 18,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "FUNCAO",
                TipoCampo = "C",
                TituloCampo = "Funcao",
                Ordem = 19,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "DATADMISSAO",
                TipoCampo = "D",
                TituloCampo = "D.Admissão",
                Ordem = 20,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mDATA",
                Tipo = typeof(DateTime)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "CTPS",
                TipoCampo = "C",
                TituloCampo = "CTPS",
                Ordem = 21,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "SERIE",
                TipoCampo = "C",
                TituloCampo = "Série",
                Ordem = 22,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "MOTIVO",
                TipoCampo = "C",
                TituloCampo = "Motivo",
                Ordem = 23,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "DATDESLIGAMENTO",
                TipoCampo = "D",
                TituloCampo = "D.Desligamento",
                Ordem = 24,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mDATA",
                Tipo = typeof(DateTime)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "TELEFONE",
                TipoCampo = "C",
                TituloCampo = "Telefone",
                Ordem = 25,
                Width = 30F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Mascara = "mTELEFONE",
                Tipo = typeof(string)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "EMAIL",
                TipoCampo = "C",
                TituloCampo = "Email",
                Ordem = 26,
                Width = 60F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(string)
            });


            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "ATIVO",
                TipoCampo = "B",
                TituloCampo = "Ativo",
                Ordem = 27,
                Width = 25F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(bool)
            });

            _listaCampos.Add(new RelatorioAssociados
            {
                Alinhamento = Element.ALIGN_LEFT,
                Currency = false,
                NomeCampo = "ASSOCIADO",
                TipoCampo = "B",
                TituloCampo = "Associado",
                Ordem = 28,
                Width = 25F,
                Totalizar = false,
                Visivel = false,
                Filtro = "",
                Tipo = typeof(bool)
            });
        }

        [HttpGet]
        [ValidacaoUsuario]
        public ActionResult Relatorios(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = _nSubMenus.RetornarTodos().Where(e => e.NomeTela.Equals("Relatórios")).FirstOrDefault(e => true)?.Sguid;
            }

            var model = new RelatorioModel();
            var tela = _nSubMenus.RetornarGuid(id).Tela.FirstOrDefault(t => t.Lista == 0);
            ViewBag.Telas = tela;
            var relatoriostela = _nRelatorios.RetornarTodosTela(tela?.Sguid);
            foreach (var relatorio in relatoriostela)
            {
                model.DropRelatorios.Add(new SelectListItem { Text = relatorio.Titulo, Value = relatorio.Sguid });
            }

            if (tela != null && tela.NomeTela.Equals("Associados"))
            {
                CriarLista();

                ViewData["DropASSOCIADO"] = new List<SelectListItem>
                {
                    new SelectListItem{Value = "true",Text = "SIM"},
                    new SelectListItem{Value = "false",Text = "NÃO"},
                    new SelectListItem{Value = "TODOS",Text = "Todos", Selected = true}
                };
                ViewData["DropATIVO"] = new List<SelectListItem>
                {
                    new SelectListItem{Value = "true", Text = "Ativo"},
                    new SelectListItem{Value = "false", Text = "Inativo"},
                    new SelectListItem{Value = "TODOS", Text = "Todos", Selected = true}
                };

                var listabancos = new List<SelectListItem>();

                ViewData["DropBANCO"] = listabancos;

                var nBancos = new NBancos(_sessionDefault);

                foreach (var bco in nBancos.RetornarTodos())
                {
                    listabancos.Add(new SelectListItem
                    {
                        Value = bco.Sguid,
                        Text = bco.NOME
                    });
                }

                return PartialView("Relatorio", _listaCampos);
            }

            return PartialView("_Relatorios", model);
        }

        [HttpPost]
        [ValidacaoUsuario]
        [ValidateAntiForgeryToken]
        public ActionResult Relatorios(FormCollection model)
        {
            var periodo = model["fil_ENIVERSARIO"];
            var motor = new RPessoas();

            CriarLista();
            foreach (var campo in _listaCampos)
            {
                campo.Filtro = model[$"fil_{campo.NomeCampo}"];
                campo.Visivel = Convert.ToBoolean(model[$"chk_{campo.NomeCampo}"]);
            }
            
            var renderedBytes = motor.ListarGeral(_sessionDefault, _listaCampos, periodo).ToArray();
            Response.AppendHeader("Content-Disposition", $"inline; filename=Relatorio.pdf");
            return File(renderedBytes, "application/pdf");
        }

        [HttpGet]
        public ActionResult RelatoriosFiltros(string relatorio, string id)
        {
            var model = new FormCollection();
            if (string.IsNullOrEmpty(relatorio)) return RedirectToAction("Relatorios", "Padrao", new { id });
            var relat = _nRelatorios.RetornarGuid(relatorio);
            var tela = _nSubMenus.RetornarGuid(id).Tela.FirstOrDefault(t => t.Lista == 0);
            ViewData["Telas"] = tela;
            ViewData["RelatoriosFiltros"] = relat.Filtros;
            ViewData["Relatorios"] = relat.Sguid;
            foreach (var filtros in relat.Filtros.Where(e => e.TipoCampo == 5))
            {
                var lista = new List<SelectListItem>();
                foreach (var registro in NewNegocio(filtros.Source).RetornarTodos(_loginProvider.Empresa))
                {
                    lista.Add(new SelectListItem
                    {
                        Text = registro.GetType().GetProperty(filtros.SourceField).GetValue(registro, null).ToString(),
                        Value = registro.Sguid,
                        Selected = false
                    });
                }
                ViewData["Rel" + filtros.NomeCampo] = lista;
            }
            foreach (var filtros in relat.Filtros.Where(e => e.TipoCampo == 6))
            {
                var assem = typeof(CBase).Assembly;
                var registro = Activator.CreateInstance(assem.GetType("Domain.C" + filtros.Source.Trim()));
                ViewData["Rel" + filtros.NomeCampo] = registro.GetType().GetProperty(filtros.SourceField)?.GetValue(registro, null);
            }
            foreach (var filtros in relat.Filtros.Where(e => e.TipoCampo == 8))
            {
                var assem = typeof(CBase).Assembly;
                var registro = Activator.CreateInstance(assem.GetType("Domain.C" + filtros.Source.Trim()));
                ViewData["RelMeses" + filtros.NomeCampo] = new List<SelectListItem>()
                {
                    new SelectListItem(){Text = "Janeiro",Value = "01"},new SelectListItem(){Text = "Fevereiro",Value = "02"},new SelectListItem(){Text = "Março",Value = "03"},
                    new SelectListItem(){Text = "Abril",Value = "04"},new SelectListItem(){Text = "Maio",Value = "05"},new SelectListItem(){Text = "Junho",Value = "06"},
                    new SelectListItem(){Text = "Julho",Value = "07"},new SelectListItem(){Text = "Agosto",Value = "08"},new SelectListItem(){Text = "Setembro",Value = "09"},
                    new SelectListItem(){Text = "Outubro",Value = "10"},new SelectListItem(){Text = "Novembro",Value = "11"},new SelectListItem(){Text = "Dezembro",Value = "12"},
                };
            }
            return PartialView("_RelatoriosFiltros", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RelatoriosFiltros(FormCollection model, string id)
        {
            var cRelatorio = _nRelatorios.RetornarGuid(id);
            var rReport = NewReport(cRelatorio.Tela.Help);
            var metodoRelatorio = rReport.GetType().GetMethod(cRelatorio.ReportName);
            var renderedBytes = ((MemoryStream)metodoRelatorio.Invoke(rReport, new object[] { _sessionDefault, cRelatorio, model })).ToArray();
            Response.AppendHeader("Content-Disposition", $"inline; filename={cRelatorio.Titulo}.pdf");
            return File(renderedBytes, "application/pdf");
        }
    }
}