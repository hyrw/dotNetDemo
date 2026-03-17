using System.Diagnostics;

public class DeviceStop : IStateHandle<DeviceState, DeviceEvent>
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
    }

    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备已停止");
    }
}
