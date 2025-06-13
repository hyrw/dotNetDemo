using OpenCvSharp;
using OpenCvToolkit.Extensions;
using OpenCvToolkit.Services;
using System;

namespace OpenCvToolkit.Services.Impl;

internal class ConnectedComponentsFilterService : IConnectedComponentsFilterService
{
    public Mat Filter(Mat defect) => FilterEdgeConnectedComponents(defect);

    public static Mat FilterEdgeConnectedComponents(Mat binaryImage)
    {
        // 确保输入是单通道二值图像
        if (binaryImage.Channels() != 1)
            throw new ArgumentException("Input must be a single-channel binary image");

        int width = binaryImage.Width;
        int height = binaryImage.Height;

        // 1. 查找所有轮廓及其层次结构
        Cv2.FindContours(binaryImage, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

        // 2. 创建输出图像（黑色背景）
        Mat result = binaryImage.Clone();

        // 3. 最外层轮廓接触边缘，则连同内部轮廓一起填黑
        for (int i = 0; i < contours.Length; i++)
        {
            // 只处理最外层轮廓（没有父轮廓）
            if (!hierarchy[i].HasParent()) continue;

            // 检查最外层轮廓是否接触边缘
            if (IsContourTouchingEdge(contours[i], width, height))
            {
                Cv2.DrawContours(result, contours, i, Scalar.Black, thickness: -1);
            }
        }

        return result;
    }

    // 检查轮廓是否接触图像边缘
    private static bool IsContourTouchingEdge(Point[] contour, int width, int height)
    {
        // 使用2像素缓冲带避免抗锯齿影响
        int edgeBuffer = 2;

        // 四周比原图小2个像素的范围，不再范围内视作接触到边缘
        Rect innerRect = new(edgeBuffer, edgeBuffer, width - 2 * edgeBuffer, height - 2 * edgeBuffer);

        foreach (var point in contour)
        {
            if (!innerRect.Contains(point))
            {
                return true;
            }
        }
        return false;
    }
}
