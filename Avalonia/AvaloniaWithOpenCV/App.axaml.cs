using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaWithOpenCV.Services;
using AvaloniaWithOpenCV.Services.Impl;
using AvaloniaWithOpenCV.ViewModels;
using AvaloniaWithOpenCV.Views;
using System.Linq;

namespace AvaloniaWithOpenCV
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;

                MainWindow window = new();
                ColorSegmentationWindow colorSegmentationWindow = new();
                ThresholdWindow thresholdWindow = new();
                IImageDisplay display = new ImageDisplay(window.TheImage);
                IThresholdService thresholdService = new ThresholdService(window, thresholdWindow, display);
                IColorSegmentationUI segmentationUI = new ColorSegmentationUI(window, colorSegmentationWindow, display);
                window.DataContext = new MainWindowViewModel(thresholdService, segmentationUI, display);
                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();
        }

        static void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}