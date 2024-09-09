using System.Windows;
using Prism.Ioc;
using Prism.Navigation.Regions;
using PrismNavigationTest.Const;

namespace PrismNavigationTest.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            //var containerRegistry = ContainerLocator.Container.Resolve<IContainerRegistry>();
            //containerRegistry.RegisterForNavigation<MainSubAView>();
            //containerRegistry.RegisterForNavigation<MainSubBView>();
        }
        private void ChangeWindow(object sender, RoutedEventArgs e)
        {
            var regionManager = ContainerLocator.Container.Resolve<IRegionManager>();
            var window = ContainerLocator.Container.Resolve<LoginWindowView>();

            #region 必须执行
            regionManager.Regions.Remove(RegionNames.MainRegion);
            RegionManager.SetRegionManager(window, regionManager);
            RegionManager.UpdateRegions();
            #endregion

            window.Show();
            this.Close();
        }
    }
}
