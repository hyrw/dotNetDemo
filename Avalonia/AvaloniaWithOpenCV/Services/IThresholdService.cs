using OpenCvSharp;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services;

public interface IThresholdService
{
    Task<Mat> ThresholdAsync(Mat img);
}
