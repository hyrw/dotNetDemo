using System.Diagnostics;

public class DeviceIdle : IStateHandle<DeviceState, DeviceEvent>
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
        Console.WriteLine($"设备总空闲时间：{totalTime}");
    }

    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备空闲中");
    }
}
