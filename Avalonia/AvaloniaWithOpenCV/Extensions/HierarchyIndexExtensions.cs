using OpenCvSharp;

namespace AvaloniaWithOpenCV.Extensions;

public static class HierarchyIndexExtensions
{
    public static bool HasParent(this HierarchyIndex h) => h.Parent != -1;

    public static bool HasChild(this HierarchyIndex h) => h.Child != -1;
}
