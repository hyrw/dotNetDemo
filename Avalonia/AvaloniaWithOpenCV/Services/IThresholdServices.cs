using OpenCvSharp;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services;

internal interface IThresholdServices
{
    Task<Mat> ThresholdAsync(Mat img);
}
