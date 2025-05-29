using AvaloniaWithOpenCV.Views;
using OpenCvSharp;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services.Impl;

public class ThresholdService(Avalonia.Controls.Window owner) : IThresholdService
{
    public async Task<Mat> ThresholdAsync(Mat img)
    {
        ThresholdWindow window = new()
        {
            Img = img.Clone()
        };
        await window.ShowDialog(owner);
        Mat result = window.Mask is null ? Mat.Zeros(MatType.CV_8UC1) : window.Mask;
        window.Close();
        return result;
    }
}
