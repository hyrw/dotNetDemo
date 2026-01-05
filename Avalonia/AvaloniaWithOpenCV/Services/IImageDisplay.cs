using System.Threading.Tasks;
using OpenCvSharp;

namespace AvaloniaWithOpenCV.Services;

public interface IImageDisplay
{
    Task Display(Mat mat);
}
