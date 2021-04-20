using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wynn.DI
{
    public static class BindingHelper
    {
        internal static IEnumerable<FieldInfo> GetInjectableFields(Type type)
        {
            foreach (var field in GetAllFields(type).Where(x => x.IsDefined(typeof(InjectAttribute), true)))
            {
                //if (!field.IsInitOnly)
                //    throw new InvalidOperationException($"{type.Name}.{field.Name} must be readonly");
                //if (field.IsPublic)
                //    throw new InvalidOperationException($"{type.Name}.{field.Name} must not be public ");

                yield return field;
            }

            //foreach (var property in type.GetAllProperties().Where(x => x.IsDefined(typeof(InjectAttribute), true) && !x.CanWrite))
            //{
            //    if (!property.CanWrite)
            //    {
            //        var expectedBackingfieldName = $"<{property.Name}>k__BackingField";

            //        var fieldInfo = property.DeclaringType
            //            .GetDeclaredFields()
            //            .Where(x => x.Name == expectedBackingfieldName)
            //            .Single();

            //        yield return fieldInfo;
            //    }
            //}
        }

        //internal static IEnumerable<PropertyInfo> GetInjectableProperties(Type type)
        //{
        //    return type.GetAllProperties().Where(x => x.IsDefined(typeof(InjectAttribute), true) && x.CanWrite);
        //}

        internal static Type CreateIFactoryType(Type type)
        {
            return typeof(IFactory<>).MakeGenericType(new[] { type });
        }

        internal static Type CreateFactoryType(Type type)
        {
            return typeof(Factory<>).MakeGenericType(new[] { type });
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            var currentType = type;

            while (currentType != null)
            {
                foreach (var field in GetDeclaredFields(currentType))
                {
                    yield return field;
                }

                currentType = currentType.BaseType;
            }
        }

        private static IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        //internal static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        //{
        //    var currentType = type;

        //    while (currentType != null)
        //    {
        //        foreach (var property in GetDeclaredProperties(currentType))
        //        {
        //            yield return property;
        //        }

        //        currentType = currentType.BaseType;
        //    }
        //}

        //private static IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        //{
        //    return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        //}
    }
}
