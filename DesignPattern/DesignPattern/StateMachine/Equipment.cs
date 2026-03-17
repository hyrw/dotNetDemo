public class Equipment : DesignPattern.Observable.IObservable<Equipment>
{
    readonly StateMachine<DeviceState, DeviceEvent> stateMachine = new(DeviceState.UnInit);
    readonly HashSet<DesignPattern.Observable.IObserver<Equipment>> observers = [];

    public Equipment()
    {
        stateMachine.Config(DeviceState.UnInit, DeviceEvent.Init, DeviceState.Idle, new DeviceInit());
        stateMachine.Config(DeviceState.Idle, DeviceEvent.Start, DeviceState.Running, new DeviceRunning());
        stateMachine.Config(DeviceState.Idle, DeviceEvent.Stop, DeviceState.Stop, new DeviceStop());
        stateMachine.Config(DeviceState.Running, DeviceEvent.Complete, DeviceState.Idle, new DeviceIdle());
        stateMachine.Config(DeviceState.Running, DeviceEvent.Stop, DeviceState.Stop, new DeviceStop());
    }

    public void Init()
    {
        stateMachine.StateTransition(DeviceEvent.Init);
        Notify();
    }

    CancellationTokenSource stopCts = new();

    public void Start()
    {
        CancellationToken stopToken = stopCts.Token;
        stateMachine.StateTransition(DeviceEvent.Start);
        Thread.Sleep(Random.Shared.Next(maxValue: 10));
        Notify();
        stateMachine.StateTransition(DeviceEvent.Complete);
    }


    public void Pause()
    {
        stateMachine.StateTransition(DeviceEvent.Pause);
        Notify();
    }

    public void Stop()
    {
        stateMachine.StateTransition(DeviceEvent.Stop);
        Notify();
    }

    public void Notify()
    {
        foreach (var observer in observers)
        {
            try
            {
                observer.OnUpdate(this);
            }
            catch { }
        }
    }

    public void Register(DesignPattern.Observable.IObserver<Equipment> observer)
    {
        observers.Add(observer);
    }

    public void Unregister(DesignPattern.Observable.IObserver<Equipment> observer)
    {
        observers.Remove(observer);
    }

}
