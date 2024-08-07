using ControlDemoApp;
using ControlDemoApp.ViewModels;
using ControlDemoApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Windows.Threading;

if (!Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA))
{
    Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
}

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(container =>
{
    container.AddSingleton<HPBarViewModel>();
    container.AddSingleton<HPBarView>(container =>
    {
        var vm = container.GetRequiredService<HPBarViewModel>();
        return new HPBarView() { DataContext = vm };
    });
    container.AddSingleton<Dispatcher>(Dispatcher.CurrentDispatcher);
    container.AddSingleton<MainWindowViewModel>();
    container.AddSingleton<App>(container =>
    {
        var window = container.GetRequiredService<MainWindow>();
        var lifeTime = container.GetRequiredService<IHostApplicationLifetime>();
        window.Visibility = Visibility.Visible;
        var app = new App()
        {
            MainWindow = window,
        };
        app.Exit += (sender, e)=>
        {
            lifeTime.StopApplication();
        };
        return app;
    });
    container.AddSingleton<MainWindow>(container =>
    {
        var viewModel = container.GetRequiredService<MainWindowViewModel>();
        return new MainWindow() { DataContext = viewModel };
    });
});

using var host = builder.Build();
var app = host.Services.GetRequiredService<App>();
host.Start();
app.Run();
await host.WaitForShutdownAsync();
await host.StopAsync();


//var wpfAppBuilder = WpfApplication<App, MainWindow>.CreateBuilder(args);
//var wpfApp = wpfAppBuilder.Build();
//await wpfApp.RunAsync();

