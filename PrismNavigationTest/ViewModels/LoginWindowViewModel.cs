using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismNavigationTest.Const;
using PrismNavigationTest.Views;
using System;
using System.Windows.Input;

namespace PrismNavigationTest.ViewModels
{
    public class LoginWindowViewModel : BindableBase
    {
        private string _title = "Login Window";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IRegionManager regionManager;

        public ICommand ClickCommand { get; set; }

        public DelegateCommand<string> ViewChangeCommand { get; set; }  

        public LoginWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ClickCommand = new DelegateCommand(ExecClickCommand);
            ViewChangeCommand = new DelegateCommand<string>(ExecViewChangeCommand);
        }

        private void ExecViewChangeCommand(string view)
        {
            if (view == "A")
            {
                regionManager.RequestNavigate(RegionNames.LoginRegion, nameof(LoginSubAView));
            }else if (view == "B")
            {
                regionManager.RequestNavigate(RegionNames.LoginRegion, nameof(LoginSubBView));
            }
        }

        private void ExecClickCommand()
        {
            //throw new NotImplementedException();
        }
    }
}
