using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NavigationDemo.ViewModel;
using System.IO;
using System.Reflection;

namespace NavigationDemo;

public static class ServiceCollectionEx
{
    public static IServiceCollection AddWpfService(this IServiceCollection services, HostBuilderContext context)
    {
        var dllFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll") ?? [];

        var types = dllFiles
            .Select(Assembly.LoadFile)
            .SelectMany(i => i.GetExportedTypes());


        services.AddSingleton<App>()
                         .AddSingleton<MainWindow>()
                         .AddSingleton<MainWindowViewModel>();

        return services;
    }
}
