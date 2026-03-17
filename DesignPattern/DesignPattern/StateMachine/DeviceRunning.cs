using System.Diagnostics;

public class DeviceRunning : IStateHandle<DeviceState, DeviceEvent>
{
    TimeSpan totalTime;
    readonly Stopwatch stopwatch = new();

    public void Enter()
    {
        stopwatch.Start();
    }

    public void Exit()
    {
        TimeSpan time = stopwatch.Elapsed;
        stopwatch.Stop();
        totalTime += time;
        Console.WriteLine($"设备运行时间: {time}");
    }

    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
    }
}
