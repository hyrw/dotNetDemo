using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaWithOpenCV.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CvPoint = OpenCvSharp.Point;
using Window = Avalonia.Controls.Window;

namespace AvaloniaWithOpenCV;

// TODO: 区分最外层轮廓
public partial class SmoothWindow : Window
{
    int Num { get; set; } = 1;

    int Strength { get; set; } = 1;

    int MinimumDeletionNum { get; set; } = 6;

    #region Styled Property

    public static readonly StyledProperty<Mat> ImgProperty;
    public Mat Img
    {
        get => GetValue(ImgProperty);
        set => SetValue(ImgProperty, value);
    }

    public static readonly StyledProperty<double> DistanceProperty;
    public double Distance
    {
        get => GetValue(DistanceProperty);
        set => SetValue(DistanceProperty, value);
    }

    #endregion

    static SmoothWindow()
    {
        ImgProperty = AvaloniaProperty.Register<SmoothWindow, Mat>(nameof(Img));
        DistanceProperty = AvaloniaProperty.Register<SmoothWindow, double>(nameof(Distance), defaultValue: 1);
    }

    public SmoothWindow()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ImgProperty || change.Property == DistanceProperty)
        {
            _ = this.UpdateImageAsync(this.Img);
        }
        if (change.Property == ImgProperty)
        {
            Avalonia.Size size = new(this.Img.Width, this.Img.Height);
            Vector dpi = new(96, 96);

            WriteableBitmap? source = this.BeforeImage.Source as WriteableBitmap;
            int width = this.Img.Width;
            int height = this.Img.Height;
            if (source is null || source.Size != size)
            {
                PixelSize pixelSize = new(width, height);
                source = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888);
                this.Img.ToBitmapParallel(source);
            }
            else
            {
                this.Img.ToBitmapParallel(source);
            }
            this.BeforeImage.Source = source;
            this.BeforeImage.InvalidateVisual();
        }
    }

    async Task UpdateImageAsync(Mat img)
    {
        if (img is null || img.Empty()) return;

        Cv2.FindContours(img, out CvPoint[][] contours, out HierarchyIndex[] hierarchyIndex, RetrievalModes.CComp, ContourApproximationModes.ApproxTC89KCOS);

        using Mat result = Mat.Zeros(img.Size(), MatType.CV_8UC1);
        try
        {
            // Bezier
            for (int i = 0; i < contours.Length; i++)
            {
                HierarchyIndex h = hierarchyIndex[i];
                int contourLength = contours[i].Length;

                // 最外层
                //if (!h.HasParent() && contourLength > MinimumDeletionNum)
                if (!h.HasParent())
                {
                    DeletePoint(ref contours[i], Distance);
                    Bezier(ref contours[i], this.Num, this.Strength);
                }
                else if (h.HasParent() && !h.HasChild() && contourLength > MinimumDeletionNum)
                {
                    DeletePoint(ref contours[i], Distance);
                    Bezier(ref contours[i], this.Num, this.Strength);
                }
            }

            // 传回层级关系，可恢复孔洞
            Cv2.DrawContours(result, contours, -1, Scalar.White, thickness: -1, hierarchy: hierarchyIndex);
        }
        catch
        {
            Trace.WriteLine("error");
        }

        using Mat color = await Task.Run(() => result.CvtColor(ColorConversionCodes.GRAY2BGR));


        Avalonia.Size size = new(result.Width, result.Height);
        Vector dpi = new(96, 96);

        WriteableBitmap? source = this.AfterImage.Source as WriteableBitmap;
        await Task.Run(() =>
        {
            if (source is null || source.Size != size)
            {
                PixelSize pixelSize = new(color.Width, color.Height);
                source = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888);
                result.ToBitmapParallel(source);
            }
            else
            {
                result.ToBitmapParallel(source);
            }
        });
        this.AfterImage.Source = source;
        this.AfterImage.InvalidateVisual();

    }
    async void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        var storeItem = e.Data.GetFiles()!;
        string localPath = storeItem.First().Path.LocalPath;

        Mat color = await ImReadAsync(localPath, ImreadModes.Grayscale);
        this.Img = color;
    }

    static Task<Mat> ImReadAsync(string file, ImreadModes flags = ImreadModes.Color) => Task.Run(() => Cv2.ImRead(file, flags));

    private void DistanceChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
    {
        this.Distance = Convert.ToDouble(e.NewValue);
    }

    private void NumChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
    {
        this.Num = Convert.ToInt32(e.NewValue);
        _ = UpdateImageAsync(this.Img);
    }

    private void StrengthChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
    {
        this.Strength = Convert.ToInt32(e.NewValue);
        _ = UpdateImageAsync(this.Img);
    }

    private void MinimumDeletionNumChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e)
    {
        this.MinimumDeletionNum = Convert.ToInt32(e.NewValue);
        _ = UpdateImageAsync(this.Img);
    }

    static void Bezier(ref CvPoint[] contour, int num, int strength)
    {
        if (contour == null || contour.Length < 2) return;

        List<CvPoint> outList = new List<CvPoint>();
        List<CvPoint> C_L = new List<CvPoint>();
        List<CvPoint> C_R = new List<CvPoint>();

        for (int i = 0; i < contour.Length; i++)
        {
            CvPoint P_CUR = contour[i];
            CvPoint P_last = i == 0 ? contour[contour.Length - 1] : contour[i - 1];
            CvPoint P_next = i == contour.Length - 1 ? contour[0] : contour[i + 1];

            CvPoint Z_L = new CvPoint(
                P_CUR.X * (strength - 1) / strength + P_last.X / strength,
                P_CUR.Y * (strength - 1) / strength + P_last.Y / strength);

            CvPoint Z_R = new CvPoint(
                P_CUR.X * (strength - 1) / strength + P_next.X / strength,
                P_CUR.Y * (strength - 1) / strength + P_next.Y / strength);

            CvPoint P_1 = new CvPoint(Z_R.X - Z_L.X, Z_R.Y - Z_L.Y);
            CvPoint P_2 = new CvPoint(P_CUR.X - Z_L.X, P_CUR.Y - Z_L.Y);

            int a1 = P_1.X, b1 = P_1.Y, a2 = P_2.X, b2 = P_2.Y;
            int x = 0, y = 0;

            if (a1 != 0 && b1 != 0)
            {
                x = (a1 * b1 * b2 + a1 * a1 * a2) / (b1 * b1 + a1 * a1);
                y = (b1 * b1 * b2 + a1 * a2 * b1) / (b1 * b1 + a1 * a1);
            }

            CvPoint MOVE = new CvPoint(P_2.X - x, P_2.Y - y);
            Z_L = new CvPoint(Z_L.X + MOVE.X, Z_L.Y + MOVE.Y);
            Z_R = new CvPoint(Z_R.X + MOVE.X, Z_R.Y + MOVE.Y);

            C_L.Add(Z_L);
            C_R.Add(Z_R);
        }

        for (int i = 0; i < contour.Length; i++)
        {
            outList.Add(contour[i]);

            for (int j = 1; j <= num; j++)
            {
                float t = j / (num + 1.0f);
                int nextIdx = (i + 1) % contour.Length;

                CvPoint P_I = new CvPoint(
                    (int)((1 - t) * (1 - t) * (1 - t) * contour[i].X +
                    3 * t * (1 - t) * (1 - t) * C_R[i].X +
                    3 * t * t * (1 - t) * C_L[nextIdx].X +
                    t * t * t * contour[nextIdx].X),
                    (int)((1 - t) * (1 - t) * (1 - t) * contour[i].Y +
                    3 * t * (1 - t) * (1 - t) * C_R[i].Y +
                    3 * t * t * (1 - t) * C_L[nextIdx].Y +
                    t * t * t * contour[nextIdx].Y));

                outList.Add(P_I);
            }
        }

        contour = [.. outList];
    }

    static void DeletePoint(ref CvPoint[] contour, double distance)
    {
        if (contour.Length < 3) return; // 至少需要3个点才有优化意义
        double distanceSq = distance * distance;

        List<CvPoint> result = new() { contour[0] }; // 保留起点

        for (int i = 1; i < contour.Length - 1; i++)
        {
            CvPoint prevDiff = contour[i] - result[^1];    // 与前一个保留点的距离
            CvPoint nextDiff = contour[i + 1] - contour[i];  // 与后一个原始点的距离

            int prevDistSq = prevDiff.X * prevDiff.X + prevDiff.Y * prevDiff.Y;
            int nextDistSq = nextDiff.X * nextDiff.X + nextDiff.Y * nextDiff.Y;

            if (prevDistSq >= distanceSq || nextDistSq >= distanceSq)
            {
                result.Add(contour[i]);
            }
        }

        result.Add(contour[^1]); // 保留终点
        contour = [.. result];
    }
}
