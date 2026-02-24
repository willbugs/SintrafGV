using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Web.InfraEstrutura
{
    public class FiltroProviderCustom : FilterAttributeFilterProvider
    {
        [Obsolete("Obsolete")]
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor);
            var dependency = (NinjectDependencyResolver) DependencyResolver.Current;
            var enumerable = filters as Filter[] ?? filters.ToArray();
            if (dependency != null)
            {
                foreach (var filter in enumerable)
                {
                   dependency.Kernel.Inject(filter.Instance);
                }
            }
            return enumerable;
        }
    }
}