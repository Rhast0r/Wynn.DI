using System;

namespace Wynn.DI
{
    public interface IFactory<T>
    {
        T Create();
    }

    internal class Factory<T> : IFactory<T>
    {
        [Inject]
        private readonly IContainerResolver _resolver = null;

        public T Create()
        {
            return (T)_resolver.Get(typeof(T));
        }
    }
}
