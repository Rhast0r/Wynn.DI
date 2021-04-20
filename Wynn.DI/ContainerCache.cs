using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wynn.DI
{
    internal class ContainerCache
    {
        private Dictionary<Binding, IReadOnlyCollection<Binding>> _bindingToDirectDependantBindings;
        private Dictionary<Type, Binding> _typeToBinding;
        private Dictionary<Type, IEnumerable<FieldInfo>> _typeToFields;

        internal ContainerCache()
        {
            _bindingToDirectDependantBindings = new Dictionary<Binding, IReadOnlyCollection<Binding>>();
            _typeToBinding = new Dictionary<Type, Binding>();

            _typeToFields = new Dictionary<Type, IEnumerable<FieldInfo>>();
        }

        internal Binding GetBinding(Type type)
        {
            return _typeToBinding[type];
        }

        internal IEnumerable<Binding> GetBindings() => _typeToBinding.Values;
        internal IEnumerable<Type> GetBoundTypes() => _typeToBinding.Keys;

        internal void AddBinding(Binding binding)
        {
            _typeToBinding.Add(binding.ServiceType, binding);
        }

        internal bool HasBinding(Type type)
        {
            return _typeToBinding.ContainsKey(type);
        }

        internal IEnumerable<Binding> GetDirectBindings(Binding binding)
        {
            if (!_bindingToDirectDependantBindings.TryGetValue(binding, out var directBindings))
            {
                var bindings = GetDependencies(binding.ImplementationType)
                    .Select(x =>
                    {
                        if (!_typeToBinding.TryGetValue(x, out var targetBinding))
                            throw new InvalidOperationException($"{binding.ServiceType.Name} or a subclass has a Dependency on {x.Name} which is not bound");

                        return targetBinding;
                    })
                    .ToHashSet();

                _bindingToDirectDependantBindings.Add(binding, bindings);
                directBindings = bindings;
            }

            return directBindings;
        }

        internal IEnumerable<FieldInfo> GetFields(Type type)
        {
            if (!_typeToFields.TryGetValue(type, out var enumerable))
            {
                enumerable = BindingHelper.GetInjectableFields(type);
                _typeToFields[type] = enumerable;
            }

            return enumerable;
        }

        internal IReadOnlyCollection<Type> GetDependencies(Type type)
        {
            var dependencies = new HashSet<Type>();
            dependencies.UnionWith(GetFields(type).Select(x => x.FieldType));
            //dependencies.UnionWith(GetProperties(type).Select(x => x.PropertyType));
            return dependencies;
        }
    }
}
