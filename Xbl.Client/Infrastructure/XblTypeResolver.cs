using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Infrastructure;

[ExcludeFromCodeCoverage]
public class XblTypeResolver : ITypeResolver
{
    private readonly IServiceProvider _services;

    public XblTypeResolver(IServiceProvider services)
    {
        _services = services;
    }

    public object Resolve(Type type)
    {
        return _services.GetService(type!);
    }
}