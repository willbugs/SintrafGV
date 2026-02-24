using System.Web;
using System.Web.Mvc;
using Ninject;

namespace Web.InfraEstrutura
{
    public class ValidacaoUsuarioAttribute : AuthorizeAttribute
    {
        [Inject]
        public ILoginProvider Loginprovider { get; set; }

        
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return Loginprovider.Autenticado;
        }
    }
}
