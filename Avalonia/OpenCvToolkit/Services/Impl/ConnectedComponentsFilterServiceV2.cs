using OpenCvSharp;
using OpenCvToolkit.Services;

namespace OpenCvToolkit;

internal class ConnectedComponentsFilterServiceV2 : IConnectedComponentsFilterService
{
    public Mat Filter(Mat defect) => FilterEdgeConnectedComponents(defect);

    public static Mat FilterEdgeConnectedComponents(Mat binaryImage)
    {
        // 使用2像素缓冲带避免抗锯齿影响
        int edgeBuffer = 2;

        // 四周比原图小2个像素的范围，不再范围内视作接触到边缘
        Rect innerRect = new(edgeBuffer, edgeBuffer, binaryImage.Width - 2 * edgeBuffer, binaryImage.Height - 2 * edgeBuffer);

        using Mat labels = new Mat();
        using Mat stats = new Mat();
        using Mat centroids = new Mat();
        int numLabels = Cv2.ConnectedComponentsWithStats(binaryImage, labels, stats, centroids);

        Mat result = binaryImage.Clone();
        for (int i = 1; i < numLabels; i++)
        {
            int x = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
            int y = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
            int x2 = stats.At<int>(i, (int)ConnectedComponentsTypes.Width) + x;
            int y2 = stats.At<int>(i, (int)ConnectedComponentsTypes.Height) + y;

            if (!innerRect.Contains(new Point(x, y)) || !innerRect.Contains(new Point(x2, y2)))
            {
                // 创建当前连通域的掩模
                using Mat mask = new Mat();
                Cv2.Compare(labels, i, mask, CmpType.EQ);

                // 用随机颜色填充连通域
                result.SetTo(Scalar.Black, mask);
            }
        }

        return result;
    }
}
