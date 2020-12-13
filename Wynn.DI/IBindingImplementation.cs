using System;

namespace Wynn.DI
{
    public interface IBindingImplementation
    {
        IBindingResolution ToNew();
        IBindingResolution ToNew<TImplementation>() where TImplementation : class;
        IBindingResolution ToNew(Type implementationType);
        IBindingResolutionAsSingleton ToConstant(object value);
    }
}
