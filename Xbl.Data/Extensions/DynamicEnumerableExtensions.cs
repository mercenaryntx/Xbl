using System.Reflection;
using AutoMapper;

namespace Xbl.Data.Extensions;

internal static class DynamicEnumerableExtensions
{
    public static IEnumerable<T> Map<T>(this IEnumerable<dynamic> source)
    {
        var constructors = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var hasDefaultCtor = constructors.Any(c => c.GetParameters().Length == 0);
        var mapper = new MapperConfiguration(c =>
        {
            c.CreateMap<IDictionary<string, object>, T>().ConvertUsing(hasDefaultCtor ? WithActivator : WithPrimaryConstructor);

        }).CreateMapper();
        return source.Select(r => mapper.Map<T>((IDictionary<string, object>)r));
    }

    private static T WithActivator<T>(IDictionary<string, object> source, T destination, ResolutionContext context)
    {
        destination ??= Activator.CreateInstance<T>();

        foreach (var (key, value) in source)
        {
            var property = destination.GetType().GetProperty(key);
            if (property != null && property.CanWrite)
            {
                var mapped = context.Mapper.Map(value, value.GetType(), property.PropertyType);
                property.SetValue(destination, mapped);
            }
        }

        return destination;
    }

    private static T WithPrimaryConstructor<T>(IDictionary<string, object> source, T destination, ResolutionContext context)
    {
        var constructor = typeof(T).GetConstructors().First();
        var parameters = constructor.GetParameters();
        var args = source.Values.Select((value, i) =>
        {
            var type = parameters[i].ParameterType;
            if (value == null) return type.IsValueType ? Activator.CreateInstance(type) : null;
            return context.Mapper.Map(value, value.GetType(), type);
        }).ToArray();
        return (T)constructor.Invoke(args);
    }
}