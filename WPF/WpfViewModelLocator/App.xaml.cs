using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
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

    static IServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<ViewAViewModel>();
        var sp = sc.BuildServiceProvider();
        Ioc.Default.ConfigureServices(sp);
        return sp;
    }

}
