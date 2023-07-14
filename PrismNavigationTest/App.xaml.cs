using Prism.Ioc;
using PrismNavigationTest.Views;
using System.Windows;

namespace PrismNavigationTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<LoginWindowView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginSubAView>();
            containerRegistry.RegisterForNavigation<LoginSubBView>();

            containerRegistry.RegisterForNavigation<MainSubAView>();
            containerRegistry.RegisterForNavigation<MainSubBView>();
        }
    }
}
