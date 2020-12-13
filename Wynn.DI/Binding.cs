using System;
using System.Collections.Generic;
using System.Linq;

namespace Wynn.DI
{
    internal class Binding : IBindingService, IBindingImplementation, IBindingResolution, IBindingScope
    {
        internal Type ServiceType { get; private set; }
        internal Type ImplementationType { get; private set; }
        internal Func<IContainerResolver, object> CreationMethod { get; private set; }
        internal BindingScope Scope { get; private set; }
        internal BindingResolution Resolution { get; private set; }

        private Container _container;

        public Binding(Container container)
        {
            _container = container;
        }

        private void Complete()
        {
            if (ServiceType == null)
                throw new InvalidOperationException();

            if (ImplementationType == null)
                throw new InvalidOperationException();

            if (CreationMethod == null)
                throw new InvalidOperationException();

            if (Scope == BindingScope.None)
                throw new InvalidOperationException();

            if (Resolution == BindingResolution.None)
                throw new InvalidOperationException();

            _container.InternalAddBinding(this);
        }

        public IBindingImplementation Bind(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            ServiceType = serviceType;

            return this;
        }

        public IBindingImplementation Bind<TService>()
        {
            return Bind(typeof(TService));
        }

        public IBindingResolution ToNew()
        {
            return ToNew(ServiceType);
        }

        public IBindingResolution ToNew<TService>() where TService : class
        {
            return ToNew(typeof(TService));
        }

        public IBindingResolution ToNew(Type implementationType)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            ValidateImplementationType(implementationType);
            ImplementationType = implementationType; 

            CreationMethod = (resolver) =>
            {
                var obj = Activator.CreateInstance(implementationType, true);
                return obj;
            };

            return this;
        }

        private void ValidateImplementationType(Type implementationType)
        {
            if (!implementationType.IsClass)
                throw new ArgumentException();

            if (implementationType.IsAbstract)
                throw new ArgumentException();

            if (implementationType.IsSubclassOf(ServiceType))
                throw new ArgumentException();
        }

        public IBindingResolutionAsSingleton ToConstant(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var implementationType = value.GetType(); 
            ValidateImplementationType(implementationType);
            ImplementationType = implementationType;

            CreationMethod = (resolver) => value;

            AsCached();

            return this;
        }

        public IBindingScope AsCached()
        {
            Resolution = BindingResolution.AsCached;
            return this;
        }

        public IBindingScopeOnResolve AsTransient()
        {
            Resolution = BindingResolution.AsTransient;
            _container.Bind(BindingHelper.CreateIFactoryType(ServiceType)).ToNew(BindingHelper.CreateFactoryType(ImplementationType)).AsCached().OnResolve();

            return this;
        }

        public void OnResolve()
        {
            Scope = BindingScope.OnResolve;

            Complete();
        }

        public void OnInstall()
        {
            Scope = BindingScope.OnInstall;

            Complete();
        }

        public override string ToString()
        {
            return $"({ServiceType.Name})";
        }
    }
}
