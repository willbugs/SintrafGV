using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using Domain;
using Negocios;
using NHibernate;
using Ninject;
using Ninject.Syntax;
using Web.Models;

namespace Web.InfraEstrutura
{
    public interface ILoginProvider
    {
        bool Autenticar(LoginModel loginModel, out string msgErro);
        bool SEmpresa(LoginModel loginModel, out string msgErro);
        void Desautenticar();
        bool Autenticado { get; }
        bool Selecionado { get; }
        CUsuarios Usuario { get; }
        LoginModel Login { get; }
        CEmpresa Empresa { get; }

    }
    public class CustonLoginProvider : ILoginProvider
    {
        private readonly NUsuarios _usuariosNegocio;
        private readonly NEmpresa _empresaNegocio;
        private readonly ISession _session;

        public CustonLoginProvider(IResolutionRoot kernel)
        {
            _session = kernel.Get<ISession>("Default");
            _usuariosNegocio = new NUsuarios(_session);
            _empresaNegocio = new NEmpresa(_session);

        }

        public bool Autenticar(LoginModel loginModel, out string msgErro)
        {
            msgErro = string.Empty;

            if (loginModel == null)
            {
                _session.Reconnect();
                return false;
            }

            var usuario = _usuariosNegocio.RetornarLogin(loginModel.Usuario.ToUpper());

            if (usuario == null)
            {
                msgErro = "Login não cadastrado!";
                return false;
            }

            if (usuario.SENHA != loginModel.Senha)
            {
                msgErro = "Senha inválida!";
                return false;
            }

            var ip = GetVisitorIpAddress();

            HttpContext.Current.Session["autenticacao"] = usuario;
            HttpContext.Current.Session["autenticacao1"] = new LoginModel()
            {
                Senha = usuario.SENHA,
                Usuario = usuario.LOGINUSUARIO
            };

            usuario.Ultimadataconexao = DateTime.Now;
            usuario.Ultimoipconecao = ip;
            usuario.Sessao = HttpContext.Current.Session.SessionID;
            usuario.Logado = 1;

            try
            {
                _usuariosNegocio.Gravar(usuario);
            }
            catch (Exception e)
            {
                msgErro = e.Message;
                return false;
            }

            return true;
        }
        public bool SEmpresa(LoginModel loginModel, out string msgErro)
        {
            msgErro = string.Empty;

            if (loginModel.Empresa == null)
            {
                msgErro = "Empresa não selecionada!";
                return false;
            }

            var empresa = _empresaNegocio.RetornarGuid(loginModel.Empresa);
            var novaempresa = new CEmpresa();

            foreach (var prop in novaempresa.GetType().GetProperties().Where(p => !p.Name.Contains("Temp")))
            {
                novaempresa.GetType().GetProperty(prop.Name)?.SetValue(novaempresa, empresa.GetType().GetProperty(prop.Name)?.GetValue(empresa), null);
            }

            HttpContext.Current.Session["sempresa"] = novaempresa;

            return true;
        }
        public void Desautenticar()
        {
            try
            {
                if (Usuario != null)
                {
                    Usuario.Logado = 0;
                    _usuariosNegocio.Gravar(Usuario, false);
                }
                HttpContext.Current.Session.RemoveAll();
                _session.Clear();
            }
            catch (Exception)
            {
                _session.Clear();
            }
        }
        public bool Autenticado => HttpContext.Current.Session["autenticacao"] != null &&
                                   HttpContext.Current.Session["autenticacao"].GetType() == typeof(CUsuarios);
        public bool Selecionado => HttpContext.Current.Session["sempresa"] != null &&
                                   HttpContext.Current.Session["sempresa"].GetType() == typeof(CEmpresa);
        public CUsuarios Usuario
        {
            get
            {
                if (Autenticado) return (CUsuarios)HttpContext.Current.Session["autenticacao"];
                return null;
            }
        }
        public LoginModel Login
        {
            get
            {
                if (Autenticado) return (LoginModel)HttpContext.Current.Session["autenticacao1"];
                return null;
            }
        }
        public CEmpresa Empresa
        {
            get
            {
                if (Selecionado) return (CEmpresa)HttpContext.Current.Session["sempresa"];
                return null;

            }
        }
        private static string GetVisitorIpAddress(bool getLan = false)
        {
            var visitorIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(visitorIpAddress)) visitorIpAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIpAddress)) visitorIpAddress = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrEmpty(visitorIpAddress) || visitorIpAddress.Trim() == "::1")
            {
                getLan = true;
                visitorIpAddress = string.Empty;
            }

            if (!getLan) return visitorIpAddress;

            if (!string.IsNullOrEmpty(visitorIpAddress)) return visitorIpAddress;

            var stringHostName = Dns.GetHostName();
            var ipHostEntries = Dns.GetHostEntry(stringHostName);
            var arrIpAddress = ipHostEntries.AddressList;

            try
            {
                visitorIpAddress = arrIpAddress.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            }
            catch
            {
                try
                {
                    visitorIpAddress = arrIpAddress[0].ToString();
                }
                catch
                {
                    try
                    {
                        arrIpAddress = Dns.GetHostAddresses(stringHostName);
                        visitorIpAddress = arrIpAddress[0].ToString();
                    }
                    catch
                    {
                        visitorIpAddress = "127.0.0.1";
                    }
                }
            }
            return visitorIpAddress;
        }
    }
}