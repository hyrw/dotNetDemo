using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NavigationDemo.ViewModel;
using System.Threading.Channels;

namespace NavigationDemo;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        //var app = new App();
        //app.Run(new MainWindow());

        var hostBuilder = Host.CreateDefaultBuilder(args);
        hostBuilder.ConfigureAppConfiguration((hostBuilderContext ,configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(hostBuilderContext.HostingEnvironment.ContentRootPath);
        });
        hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
        {
            serviceCollection.AddSingleton<Channel<int>>(sp => Channel.CreateUnbounded<int>())
                             .AddSingleton<ChannelWriter<int>>(sp => sp.GetRequiredService<Channel<int>>().Writer)
                             .AddSingleton<ChannelReader<int>>(sp => sp.GetRequiredService<Channel<int>>().Reader)
                             .AddWpfService(hostBuilderContext)
                             .AddHostedService<Worker>()
                             .AddHostedService<WpfHostedService<App, MainWindow>>();
        });
        hostBuilder.Build().Run();
        Console.WriteLine(hostBuilder.ToString());
    }
}
