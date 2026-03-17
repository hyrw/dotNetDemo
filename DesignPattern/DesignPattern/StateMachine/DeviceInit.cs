public class DeviceInit : IStateHandle<DeviceState, DeviceEvent>
{
    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备初始化完毕");
    }
}
