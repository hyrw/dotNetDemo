using OpenCvSharp;
using AvaloniaWithOpenCV.Services;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.ViewModels;

public partial class MainWindowViewModel(IThresholdService thresholdService) : ViewModelBase
{
    [RelayCommand]
    Task OpenThresholdWindow()
    {
        using Mat color = Cv2.ImRead(@"d:/新建文件夹/IMG_0067.JPG");
        return thresholdService.ThresholdAsync(color);
    }

    [RelayCommand]
    Task OpenSmoothWindow()
    {
        var window = new SmoothWindow() {};
        window.Show();
        return Task.CompletedTask;
    }
}
