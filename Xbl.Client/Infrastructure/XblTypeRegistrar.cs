using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Infrastructure;

[ExcludeFromCodeCoverage]
public class XblTypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public XblTypeRegistrar(IServiceCollection services)
    {
        _services = services;
    }

    public void Register(Type service, Type implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _services.AddSingleton(service, factory);
    }

    public ITypeResolver Build()
    {
        return new XblTypeResolver(_services.BuildServiceProvider());
    }
}