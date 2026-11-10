using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zad.BuildingBlocks.Application.Composition;

public interface IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration);
}
