using System.IO;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using WpfPoint = System.Windows.Point;

namespace CaliperTool.Wpf.Services;

/// <summary>
/// 负责图像加载、格式转换，以及图像坐标与画布坐标之间的映射
/// </summary>
public sealed class ImageService : IDisposable
{
    private Mat? _currentMat;
    private bool _disposed;

    // ── 属性 ──────────────────────────────────────────────────

    /// <summary>当前图像的像素宽度</summary>
    public int ImageWidth => _currentMat?.Width ?? 0;

    /// <summary>当前图像的像素高度</summary>
    public int ImageHeight => _currentMat?.Height ?? 0;

    public bool HasImage => _currentMat is not null && !_currentMat.IsDisposed;

    /// <summary>
    /// 返回当前 Mat 的克隆，供外部（CaliperService）进行图像处理。
    /// 调用方负责 Dispose 返回的 Mat。未加载图像时返回 null。
    /// </summary>
    public Mat? GetCurrentMat() => HasImage ? _currentMat!.Clone() : null;

    // ── 图像加载 ──────────────────────────────────────────────

    /// <summary>
    /// 从文件路径加载图像，返回可用于 WPF Image 控件的 BitmapSource
    /// </summary>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">文件无法识别为图像时抛出</exception>
    public BitmapSource LoadImage(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("图像文件不存在。", filePath);

        // 释放旧的 Mat
        _currentMat?.Dispose();

        // IMREAD_COLOR 确保统一为 BGR 3通道
        _currentMat = Cv2.ImRead(filePath, ImreadModes.Color);

        if (_currentMat.Empty())
            throw new InvalidOperationException($"无法读取图像文件：{filePath}");

        return MatToBitmapSource(_currentMat);
    }

    // ── 格式转换 ──────────────────────────────────────────────

    /// <summary>
    /// 将 OpenCV Mat（BGR/BGRA/Gray）转换为 WPF BitmapSource
    /// </summary>
    public static BitmapSource MatToBitmapSource(Mat mat)
    {
        ArgumentNullException.ThrowIfNull(mat);
        if (mat.Empty()) throw new ArgumentException("Mat 为空。");

        // 统一转为 BGR（3通道）
        Mat bgrMat = mat;
        bool needDispose = false;

        if (mat.Channels() == 1)
        {
            bgrMat = new Mat();
            Cv2.CvtColor(mat, bgrMat, ColorConversionCodes.GRAY2BGR);
            needDispose = true;
        }
        else if (mat.Channels() == 4)
        {
            bgrMat = new Mat();
            Cv2.CvtColor(mat, bgrMat, ColorConversionCodes.BGRA2BGR);
            needDispose = true;
        }

        try
        {
            // stride = 宽度 × 每像素字节数（BGR = 3）
            int stride = bgrMat.Width * 3;
            byte[] buffer = new byte[stride * bgrMat.Height];
            System.Runtime.InteropServices.Marshal.Copy(bgrMat.Data, buffer, 0, buffer.Length);

            var bitmap = BitmapSource.Create(
                bgrMat.Width, bgrMat.Height,
                96, 96,
                System.Windows.Media.PixelFormats.Bgr24,
                null,
                buffer,
                stride);

            bitmap.Freeze(); // 跨线程共享需要 Freeze
            return bitmap;
        }
        finally
        {
            if (needDispose) bgrMat.Dispose();
        }
    }

    // ── 坐标变换 ──────────────────────────────────────────────

    /// <summary>
    /// 将画布（控件）坐标映射到图像像素坐标（亚像素精度）
    /// </summary>
    /// <param name="canvasPoint">画布上的点（WPF 逻辑像素）</param>
    /// <param name="renderWidth">图像控件的实际渲染宽度</param>
    /// <param name="renderHeight">图像控件的实际渲染高度</param>
    /// <returns>图像坐标（可含小数，亚像素精度）</returns>
    public WpfPoint CanvasToImageCoordinate(
        WpfPoint canvasPoint,
        double renderWidth,
        double renderHeight)
    {
        if (!HasImage || renderWidth <= 0 || renderHeight <= 0)
            return canvasPoint;

        // WPF Image 控件默认 Stretch=Uniform：等比缩放并居中
        double scaleX = renderWidth / ImageWidth;
        double scaleY = renderHeight / ImageHeight;
        double scale = Math.Min(scaleX, scaleY); // Uniform 缩放取最小比例

        double displayW = ImageWidth * scale;
        double displayH = ImageHeight * scale;

        // 图像在控件中居中的偏移量
        double offsetX = (renderWidth - displayW) / 2.0;
        double offsetY = (renderHeight - displayH) / 2.0;

        // 还原到图像坐标
        double imgX = (canvasPoint.X - offsetX) / scale;
        double imgY = (canvasPoint.Y - offsetY) / scale;

        return new WpfPoint(imgX, imgY);
    }

    /// <summary>
    /// 将图像坐标映射回画布坐标（用于绘制）
    /// </summary>
    public WpfPoint ImageToCanvasCoordinate(
        WpfPoint imagePoint,
        double renderWidth,
        double renderHeight)
    {
        if (!HasImage || renderWidth <= 0 || renderHeight <= 0)
            return imagePoint;

        double scaleX = renderWidth / ImageWidth;
        double scaleY = renderHeight / ImageHeight;
        double scale = Math.Min(scaleX, scaleY);

        double displayW = ImageWidth * scale;
        double displayH = ImageHeight * scale;

        double offsetX = (renderWidth - displayW) / 2.0;
        double offsetY = (renderHeight - displayH) / 2.0;

        double canvasX = imagePoint.X * scale + offsetX;
        double canvasY = imagePoint.Y * scale + offsetY;

        return new WpfPoint(canvasX, canvasY);
    }

    /// <summary>
    /// 判断图像坐标是否在图像范围内
    /// </summary>
    public bool IsInImageBounds(WpfPoint imagePoint) =>
        HasImage &&
        imagePoint.X >= 0 && imagePoint.X < ImageWidth &&
        imagePoint.Y >= 0 && imagePoint.Y < ImageHeight;

    // ── IDisposable ───────────────────────────────────────────

    public void Dispose()
    {
        if (!_disposed)
        {
            _currentMat?.Dispose();
            _disposed = true;
        }
    }
}
