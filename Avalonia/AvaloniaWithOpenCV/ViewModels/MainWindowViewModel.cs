using AvaloniaWithOpenCV.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using System.IO;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.ViewModels;

internal partial class MainWindowViewModel(IThresholdService thresholdService, IColorSegmentationUI colorSegmentation, IImageDisplay display) : ViewModelBase
{
    [RelayCommand]
    void OpenThresholdWindow()
    {
        thresholdService.Show();
    }

    [RelayCommand]
    static Task OpenSmoothWindow()
    {
        var window = new SmoothWindow() { };
        window.Show();
        return Task.CompletedTask;
    }

    [RelayCommand]
    void OpenColorRangeWindow()
    {
        colorSegmentation.Show();
    }

    [ObservableProperty]
    public partial string FilePath { get; set; }

    async partial void OnFilePathChanged(string value)
    {
        if (!File.Exists(value)) return;

        using Mat mat = Cv2.ImRead(value);
        await display.Display(mat);
    }
}
