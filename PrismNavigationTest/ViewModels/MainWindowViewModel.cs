using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using PrismNavigationTest.Const;
using PrismNavigationTest.Views;
using System.Windows.Input;

namespace PrismNavigationTest.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Main Window";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IRegionManager regionManager;

        public ICommand ClickCommand { get; set; }

        public DelegateCommand<string> ViewChangeCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ClickCommand = new DelegateCommand(ExecClickCommand);
            ViewChangeCommand = new DelegateCommand<string>(ExecViewChangeCommand);
        }

        private void ExecClickCommand()
        {
            //throw new NotImplementedException();
        }

        private void ExecViewChangeCommand(string view)
        {
            if (view == "A")
            {
                regionManager.RequestNavigate(RegionNames.MainRegion, nameof(MainSubAView));
            }
            else if (view == "B")
            {
                regionManager.RequestNavigate(RegionNames.MainRegion, nameof(MainSubBView));
            }
        }
    }
}
