using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.RichTextBox.Themes;
using System.Windows;
using System.Windows.Controls;
using WpfViewModelLocator.LogConfig;
using WpfViewModelLocator.ViewModel;

namespace WpfViewModelLocator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App() : base()
    {
        InitializeComponent();
        ServiceProvider = ConfigureServices();
    }
    public IServiceProvider ServiceProvider { get; init; }

    static ServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ViewAViewModel>();
        sc.AddLogging(logBuilder =>
        {
            var box = new RichTextBox()
            {
                IsReadOnly = true,
            };
            logBuilder.Services.AddSingleton(box);

            Log.Logger = new LoggerConfiguration()
            .Enrich.WithClassName()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ClassName} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.RichTextBox(box, theme: RichTextBoxConsoleTheme.Colored)
            .CreateLogger();

            logBuilder.AddSerilog(logger: Log.Logger);
        });
        var sp = sc.BuildServiceProvider();
        Ioc.Default.ConfigureServices(sp);
        return sp;
    }

}
