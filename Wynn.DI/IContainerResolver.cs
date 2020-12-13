using System;

namespace Wynn.DI
{
    public interface IContainerResolver
    {
        T Get<T>();
        object Get(Type type);
        void Inject(object obj);
    }
}
