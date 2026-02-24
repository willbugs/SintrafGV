using Domain;

namespace Negocios
{
    public sealed class Global
    {
        private static Global _instance;
        private static CEmpresa _empresaLogada;
        private static CUsuarios _usuario;
        private static object _loginmodel;
        private static bool _autenticado;
        private static string _subdominio;
        private static bool _atualizar;

        private Global()
        {
        }
        public static Global GetInstance
        {
            get
            {
                if (_instance == null)
                    lock (typeof(Global))
                        if (_instance == null)
                            _instance = new Global();
                return _instance;
            }
        }

        public void SetAtualizar(bool atualizar)
        {
            _atualizar = atualizar;
        }

        public bool GetAtualizar()
        {
            return _atualizar;
        }
        public void SetEmpresaLogada(CEmpresa empresa)
        {
            _empresaLogada = empresa;
        }
        public CEmpresa GetEmpresaLogada()
        {
            return _empresaLogada;
        }
        public void SetUsuario(CUsuarios usuario)
        {
            _usuario = usuario;
        }
        public CUsuarios GetUsuario()
        {
            return _usuario;
        }
        public void SetAutenticado(bool autenticado)
        {
            _autenticado = autenticado;
        }
        public bool GetAuteticado()
        {
            return _autenticado;
        }
        public void SetLoginModel(object loginModel)
        {
            _loginmodel = loginModel;
        }
        public object GetLoginModel()
        {
            return _loginmodel;
        }
        public void SetSubDomain(string subdominio)
        {
            _subdominio = subdominio;
        }
        public string GetSubDomain()
        {
            return _subdominio;
        }
    }
}
