
using AvaloniaWithOpenCV.Services;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.ViewModels;

public partial class MainWindowViewModel(IThresholdService thresholdService) : ViewModelBase
{
    [RelayCommand]
    Task Threshold()
    {
        return thresholdService.ThresholdAsync(null);
    }
}
