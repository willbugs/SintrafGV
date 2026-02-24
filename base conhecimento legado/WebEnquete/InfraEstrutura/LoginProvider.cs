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
        void Desautenticar();
        void AtualizaPessoa(CPessoas pessoa);
        bool Autenticado { get; }
        bool Selecionado { get; }
        CPessoas Pessoa { get; }
        LoginModel Login { get; }
        CEmpresa Empresa { get; }

    }
    public class CustonLoginProvider : ILoginProvider
    {
        private readonly NPessoas _pessoasNegocio;
        private readonly NLogEnqueteLogin _logEnqueteLogin;
        private readonly ISession _session;

        public CustonLoginProvider(IResolutionRoot kernel)
        {
            _session = kernel.Get<ISession>("Default");
            _pessoasNegocio = new NPessoas(_session);
            _logEnqueteLogin = new NLogEnqueteLogin(_session);
        }

        public bool Autenticar(LoginModel loginModel, out string msgErro)
        {
            msgErro = string.Empty;

            if (loginModel == null)
            {
                _session.Reconnect();
                return false;
            }

            loginModel.Cpf = loginModel.Cpf.Replace(".", "").Replace("-", "");
            loginModel.Matricula = loginModel.Matricula.Replace(".", "").Replace("-", "");

            var lpessoa = _pessoasNegocio.RetornarCpfcnpjAtivo(loginModel.Cpf.ToUpper());

            if (!lpessoa.Any(e => e.ATIVO))
            {
                msgErro = "Seu cadastro não está ativo!";
                return false;
            }

            var pessoa = lpessoa.FirstOrDefault(e => e.ATIVO);

            if (pessoa == null)
            {
                msgErro = "Cpf não cadastrado!";
                return false;
            }

            if (pessoa.MATRICULABANCARIA != loginModel.Matricula)
            {
                msgErro = "Matrícula inválida!";
                return false;
            }

            if (!pessoa.DATNASCIMENTO.Equals(loginModel.DataNasc))
            {
                msgErro = "Data nascimento inválida!";
                return false;
            }

            //if (pessoa.DATDESLIGAMENTO != null)
            //{
            //    msgErro = $"Afiliado foi desligado {pessoa.DATDESLIGAMENTO.Value:d}!";
            //    return false;
            //}

            var ip = GetVisitorIpAddress();

            HttpContext.Current.Session["autenticacao"] = pessoa;
            HttpContext.Current.Session["autenticacao1"] = new LoginModel()
            {
                Matricula = pessoa.MATRICULASINDICATO,
                Cpf = pessoa.CPF,
                DataNasc = pessoa.DATNASCIMENTO
            };

            try
            {
                var cLogEnqueteLogin = new CLogEnqueteLogin
                {
                    DATA = DateTime.Now,
                    PESSOA = pessoa,
                    CPF = loginModel.Cpf,
                    MATRICULA = loginModel.Matricula,
                    DATANASCIMENTO = loginModel.DataNasc
                };
                _logEnqueteLogin.Gravar(cLogEnqueteLogin);
            }
            catch (Exception e)
            {
                msgErro = e.Message;
                return false;
            }

            return true;
        }
        public void Desautenticar()
        {
            try
            {
                HttpContext.Current.Session.RemoveAll();
                _session.Clear();
            }
            catch (Exception)
            {
                _session.Clear();
            }
        }

        public void AtualizaPessoa(CPessoas pessoa)
        {
            HttpContext.Current.Session["autenticacao"] = pessoa;
        }

        public bool Autenticado => HttpContext.Current.Session["autenticacao"] != null &&
                                   HttpContext.Current.Session["autenticacao"].GetType() == typeof(CPessoas);
        public bool Selecionado => HttpContext.Current.Session["sempresa"] != null &&
                                   HttpContext.Current.Session["sempresa"].GetType() == typeof(CEmpresa);
        public CPessoas Pessoa
        {
            get
            {
                if (Autenticado) return (CPessoas)HttpContext.Current.Session["autenticacao"];
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