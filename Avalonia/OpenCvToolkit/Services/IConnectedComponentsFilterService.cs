using OpenCvSharp;

namespace OpenCvToolkit.Services;

public interface IConnectedComponentsFilterService
{
    public Mat Filter(Mat defect);
}