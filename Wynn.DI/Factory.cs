using System;

namespace Wynn.DI
{
    public interface IFactory
    {
        object BaseCreate();
    }

    public interface IFactory<T> : IFactory
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

        object IFactory.BaseCreate()
        {
            return Create();
        }
    }
}
