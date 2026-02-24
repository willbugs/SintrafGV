using System;
using System.Collections.Generic;
using System.Web.Mvc;
using NHibernate;
using Ninject;
using Persistencia;

namespace Web.InfraEstrutura
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        public NinjectDependencyResolver(string nBanco = "DATABASE")
        {
            Kernel = new StandardKernel();
            AdicionarBindings(nBanco);
        }

        public IKernel Kernel { get; private set; }

        private void AdicionarBindings(string nBanco)
        {
            Kernel.Bind<ISession>().ToMethod(c => FluentSessionFactory.AbrirSession(nBanco)).InTransientScope().Named("Default");
            Kernel.Bind<ILoginProvider>().To<CustonLoginProvider>();
            Kernel.Inject(this);
        }

        public object GetService(Type serviceType)
        {
            return Kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Kernel.GetAll(serviceType);
        }
    }
}