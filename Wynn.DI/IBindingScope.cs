namespace Wynn.DI
{
    public interface IBindingScope : IBindingScopeOnResolve
    {
        void OnInstall();
    }

    public interface IBindingScopeOnResolve
    {
        void OnRequest();
    }
}
