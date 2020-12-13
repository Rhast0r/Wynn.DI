using System;

namespace Wynn.DI
{
    public interface IBindingService
    {
        IBindingImplementation Bind(Type serviceType);
        IBindingImplementation Bind<TService>();
    }
}
