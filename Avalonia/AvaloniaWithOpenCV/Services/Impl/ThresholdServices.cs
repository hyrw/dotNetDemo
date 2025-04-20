using AvaloniaWithOpenCV.Views;
using OpenCvSharp;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services.Impl;

internal class ThresholdServices(ThresholdWindow window) : IThresholdServices
{
    public async Task<Mat> ThresholdAsync(Mat img)
    {
        window.Img = img;
        await window.ShowDialog(window);
        Mat? result = window.Mask;
        window.Close();
        return result ?? Mat.Zeros(img.Size(), MatType.CV_8UC1);
    }
}
