using System.ComponentModel;
using System.Windows;
using Prism.Ioc;
using Prism.Regions;
using PrismNavigationTest.Const;

namespace PrismNavigationTest.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindowView : Window
    {
        public LoginWindowView()
        {
            InitializeComponent();
        }

        private void ChangeWindow(object sender, RoutedEventArgs e)
        {
            var regionManager = ContainerLocator.Container.Resolve<IRegionManager>();
            var window = ContainerLocator.Container.Resolve<MainWindowView>();

            #region 必须执行
            regionManager.Regions.Remove(RegionNames.LoginRegion);
            RegionManager.SetRegionManager(window, regionManager);
            RegionManager.UpdateRegions();
            #endregion

            window.Show();
            this.Close();
        }
    }
}
