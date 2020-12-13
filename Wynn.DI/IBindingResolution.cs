namespace Wynn.DI
{
    public interface IBindingResolution : IBindingResolutionAsSingleton
    {
        IBindingScopeOnResolve AsTransient();
    }

    public interface IBindingResolutionAsSingleton
    {
        IBindingScope AsCached();
    }
}
