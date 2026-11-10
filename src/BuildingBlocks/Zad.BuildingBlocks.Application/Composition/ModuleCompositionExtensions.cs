using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zad.BuildingBlocks.Application.Composition;

public static class ModuleCompositionExtensions
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] installerAssemblies)
    {
        var seen = new HashSet<Assembly>();

        foreach (var assembly in installerAssemblies)
        {
            if (!seen.Add(assembly))
            {
                throw new InvalidOperationException($"Installer assembly '{assembly.FullName}' was passed more than once");
            }

            var installerTypes = assembly.GetTypes()
                .Where(type => typeof(IModuleInstaller).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .ToList();

            if (installerTypes.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Assembly '{assembly.FullName}' must contain exactly one non-abstract IModuleInstaller implementation, but found {installerTypes.Count}");
            }

            var installer = (IModuleInstaller)Activator.CreateInstance(installerTypes[0])!;
            installer.Install(services, configuration);
        }

        return services;
    }
}
