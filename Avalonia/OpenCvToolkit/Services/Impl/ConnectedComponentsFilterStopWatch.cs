using OpenCvSharp;
using System;
using System.Diagnostics;

namespace OpenCvToolkit.Services.Impl;

internal class ConnectedComponentsFilterStopWatch(IConnectedComponentsFilterService impl) : IConnectedComponentsFilterService
{
    readonly Stopwatch stopwatch = new ();
    public Mat Filter(Mat defect)
    {
        stopwatch.Restart();
        var result = impl.Filter(defect);
        Console.WriteLine (stopwatch.ElapsedMilliseconds);
        stopwatch.Stop();
        return result;
    }
}
