using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Wynn.DI
{
    public class Container : IContainerResolver
    {
        internal ContainerCache Cache { get; }

        private Dictionary<Binding, object> _bindingToObject;
        private bool _isInstalled;

        public static Container Create() => new Container();

        private Container()
        {
            _bindingToObject = new Dictionary<Binding, object>();
            Cache = new ContainerCache();
            CreateBinding().Bind<IContainerResolver>().ToConstant(this).AsCached().OnResolve();
        }

        private IBindingService CreateBinding()
        {
            ThrowIfInstalled();

            return new Binding(this);
        }

        public void Install()
        {
            ThrowIfInstalled();
            _isInstalled = true;

            EnsureBindingsResolved(Cache.GetBindings().Where(x => x.Scope == BindingScope.OnInstall));
        }

        private void EnsureBindingsResolved(IEnumerable<Binding> bindings)
        {
            var visitedBindings = new HashSet<Binding>();
            foreach (var binding in bindings)
                EnsureBindingResolved(binding, visitedBindings);
        }

        internal object GetObject(Binding binding)
        {
            if (binding.Resolution == BindingResolution.AsTransient)
                throw new InvalidOperationException();

            return _bindingToObject[binding];
        }

        internal bool HasObject(Binding binding)
        {
            if (binding.Resolution == BindingResolution.AsTransient)
                throw new InvalidOperationException();

            return _bindingToObject.ContainsKey(binding);
        }

        internal void AddObject(Binding binding, object obj)
        {
            if (binding.Resolution == BindingResolution.AsTransient)
                throw new InvalidOperationException();

            _bindingToObject.Add(binding, obj);
        }

        private void EnsureBindingResolved(Binding binding, HashSet<Binding> visitedBindings)
        {
            if (binding.Resolution == BindingResolution.AsCached && HasObject(binding))
                return;

            if (visitedBindings == null)
                visitedBindings = new HashSet<Binding>();

            foreach (var currentBinding in Cache.GetDirectBindings(binding))
            {
                if (visitedBindings.Contains(currentBinding))
                    continue;

                visitedBindings.Add(currentBinding);

                EnsureBindingResolved(currentBinding, visitedBindings);
            }

            if (binding.Resolution != BindingResolution.AsTransient)
                EnsureObjectCreated(binding);
        }

        private void EnsureObjectCreated(Binding binding)
        {
            if (binding.Resolution == BindingResolution.AsTransient)
                throw new InvalidOperationException();

            if (!HasObject(binding))
                AddObject(binding, CreateObject(binding));
        }

        private object CreateObject(Binding binding)
        {
            var obj = binding.CreationMethod(this);
            ProcessInject(obj);
            return obj;
        }

        private void ProcessInject(object obj)
        {
            InternalInject(obj);
            (obj as IInitialize)?.Initialize();
        }

        private void CheckCircularDependencies(Binding binding)
        {
            var open = new Queue<Binding>();
            open.Enqueue(binding);

            var totalBindings = new HashSet<Binding>();

            while (open.Count > 0)
            {
                var currentBinding = open.Dequeue();

                if (totalBindings.Contains(currentBinding))
                    continue;

                totalBindings.Add(currentBinding);

                var currentDirectBindings = Cache.GetDirectBindings(currentBinding);
                foreach (var currentDirectBinding in currentDirectBindings)
                {
                    if (binding == currentDirectBinding)
                        throw new InvalidOperationException("circular dependency");

                    open.Enqueue(currentDirectBinding);
                }
            }
        }

        private void ThrowIfInstalled()
        {
            if (_isInstalled)
                throw new InvalidOperationException("for this operation the kernel must have not been installed");
        }

        private void ThrowIfNotInstalled()
        {
            if (!_isInstalled)
                throw new InvalidOperationException("for this operation the kernel must have been installed");
        }

        public void Validate()
        {
            ThrowIfInstalled();

            foreach (var binding in Cache.GetBindings())
            {
                if (Cache.GetDependencies(binding.ImplementationType).Count == 0)
                    continue;
            }

            foreach (var binding in Cache.GetBindings())
                CheckCircularDependencies(binding);

            var visitedBindings = new HashSet<Binding>();
            foreach (var binding in Cache.GetBindings().Where(x => x.Scope == BindingScope.OnInstall))
                EnsureBindingResolved(binding, visitedBindings);

            EnsureBindingsResolved(Cache.GetBindings());
        }

        public T Get<T>() => (T)Get(typeof(T));
        public object Get(Type serviceType)
        {
            ThrowIfNotInstalled();

            var binding = Cache.GetBinding(serviceType);
            EnsureBindingResolved(binding, null);
            if (binding.Resolution == BindingResolution.AsCached)
            {
                return GetObject(binding);
            }
            else if (binding.Resolution == BindingResolution.AsTransient)
            {
                return CreateObject(binding);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Inject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            // Note: The injectee could be unknow to the container and therefor no binding might exist hence we just resolve the bindings it might have
            EnsureBindingsResolved(Cache.GetDependencies(obj.GetType()).Select(x => Cache.GetBinding(x)));

            InternalInject(obj);

            (obj as IInitialize)?.Initialize();
        }

        private void InternalInject(object obj)
        {
            var type = obj.GetType();
            foreach (var fields in Cache.GetFields(type))
            {
                var binding = Cache.GetBinding(fields.FieldType);
                fields.SetValue(obj, GetObject(binding));
            }

            //foreach (var properties in BindingHelper.GetInjectableProperties(objectType))
            //{
            //    var boundObject = GetOrCreateBoundObject(properties.PropertyType);

            //    properties.SetValue(obj, boundObject);
            //}
        }

        internal void InternalAddBinding(Binding binding)
        {
            ThrowIfInstalled();

            Cache.AddBinding(binding);
        }

        public IBindingImplementation Bind<T>()
        {
            return CreateBinding().Bind<T>();
        }

        public IBindingImplementation Bind(Type serviceType)
        {
            return CreateBinding().Bind(serviceType);
        }
    }
}
