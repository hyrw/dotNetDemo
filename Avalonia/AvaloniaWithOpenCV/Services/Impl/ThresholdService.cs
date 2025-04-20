using AvaloniaWithOpenCV.Views;
using OpenCvSharp;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services.Impl;

public class ThresholdService(Avalonia.Controls.Window owner) : IThresholdService
{
    public async Task<Mat> ThresholdAsync(Mat img)
    {
        ThresholdWindow window = new ThresholdWindow();
        window.Img = img.Clone();
        await window.ShowDialog(owner);
        Mat? result = window.Mask;
        window.Close();
        return result ?? Mat.Zeros(img.Size(), MatType.CV_8UC1);
    }
}
