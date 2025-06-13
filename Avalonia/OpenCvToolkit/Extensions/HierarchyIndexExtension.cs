using OpenCvSharp;

namespace OpenCvToolkit.Extensions;

public static class HierarchyIndexExtension
{
    public static bool HasParent(this HierarchyIndex h) => h.Parent != -1;

    public static bool HasChild(this HierarchyIndex h) => h.Child != -1;
}
