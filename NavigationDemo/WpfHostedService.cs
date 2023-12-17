using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace NavigationDemo;

public class WpfHostedService<TApplication, TWindow> : IHostedService
    where TApplication : Application
    where TWindow : Window
{

    public WpfHostedService(TApplication application,
                            TWindow window,
                            ILogger<WpfHostedService<TApplication, TWindow>> logger,
                            IHostApplicationLifetime hostApplicationLifetime)
    {
        this.application = application;
        this.window = window;
        this.logger = logger;
        this.hostApplicationLifetime = hostApplicationLifetime;
    }

    private readonly TApplication application;
    private readonly TWindow window;
    private readonly ILogger<WpfHostedService<TApplication, TWindow>> logger;
    private readonly IHostApplicationLifetime hostApplicationLifetime;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("WpfStartAsync... {Now}", DateTime.Now);
        hostApplicationLifetime.ApplicationStopping.Register(application.Shutdown);
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        application.Run(window);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        application.Shutdown();
        return Task.CompletedTask;
    }
}
