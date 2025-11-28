public class Singleton_0
{
    private static readonly Singleton_0 instance = new();
    public static Singleton_0 Instance => instance;

    private Singleton_0() { }
}

public class Singleton_1
{
    private readonly static object lockObj = new();
    private static Singleton_1? instance;
    public static Singleton_1 Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    instance ??= new Singleton_1();
                }
            }
            return instance;
        }
    }

    private Singleton_1() { }
}

public class Singleton_2
{
    private readonly static Lazy<Singleton_2> instance = new();
    public static Singleton_2 Instance
    {
        get => instance.Value;
    }

    public Singleton_2() { }
}

public class Singleton_3<T> where T : new()
{
    readonly static Lazy<T> instance = new();
    public static T Instance => instance.Value;
}

public enum DeviceState
{
    Init,
    Running,
    Idle,
    Stop,
}

public enum DeviceEvent
{
    Init,
    Start,
    Pause,
    Complete,
    Stop,
}

public interface IStateHandle<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    void Handle(StateMachine<TState, TTrigger> stateMatchine);
}

public class DeviceInit : IStateHandle<DeviceState, DeviceEvent>
{
    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备初始化完毕");
    }
}

public class DeviceRunning : IStateHandle<DeviceState, DeviceEvent>
{
    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备处理完毕");
        Thread.Sleep(TimeSpan.FromSeconds(20));
        Console.WriteLine("设备处理完毕");
        stateMatchine.StateTransition(DeviceEvent.Complete);
    }
}

public class DeviceIdle : IStateHandle<DeviceState, DeviceEvent>
{
    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备空闲中");
    }
}

public class DeviceStop : IStateHandle<DeviceState, DeviceEvent>
{
    public void Handle(StateMachine<DeviceState, DeviceEvent> stateMatchine)
    {
        Console.WriteLine("设备已停止");
    }
}

public class StateMachine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{

    readonly Dictionary<TState, Dictionary<TTrigger, (TState, IStateHandle<TState, TTrigger>)>> transitions = [];

    TState CurrentState;

    public StateMachine(TState CurrentState)
    {
        this.CurrentState = CurrentState;
    }

    public void Config(TState from, TTrigger e, TState to, IStateHandle<TState, TTrigger> nextHandle)
    {
        if (!transitions.TryGetValue(from, out var dict))
        {
            dict = [];
            transitions[from] = dict;
        }

        dict[e] = (to, nextHandle);
    }

    public TState GetNextState(TState state, TTrigger e)
    {
        return transitions[state][e].Item1;
    }

    public IStateHandle<TState, TTrigger> GetHandle(TState state, TTrigger e)
    {
        return transitions[state][e].Item2;
    }

    bool CanFire(TTrigger e)
    {
        return transitions.TryGetValue(CurrentState, out var next) && next.TryGetValue(e, out _);
    }

    public void StateTransition(TTrigger e)
    {
        if (CanFire(e))
        {
            var (nextState, nextHandle) = transitions[CurrentState][e];
            nextHandle?.Handle(this);
            CurrentState = nextState;
        }
        else
        {
            throw new InvalidOperationException("状态转换失败");
        }
    }

}

public class Equipment
{
    readonly StateMachine<DeviceState, DeviceEvent> stateMachine = new(DeviceState.Init);

    public Equipment()
    {
        stateMachine.Config(DeviceState.Init, DeviceEvent.Init, DeviceState.Idle, new DeviceInit());
        stateMachine.Config(DeviceState.Idle, DeviceEvent.Start, DeviceState.Running, new DeviceIdle());
        stateMachine.Config(DeviceState.Running, DeviceEvent.Complete, DeviceState.Idle, new DeviceRunning());
        stateMachine.Config(DeviceState.Running, DeviceEvent.Stop, DeviceState.Idle, new DeviceStop());
    }

    public void Init()
    {
        stateMachine.StateTransition(DeviceEvent.Init);
    }

    CancellationTokenSource stopCts = new();

    public void Start()
    {
        CancellationToken stopToken = stopCts.Token;
        stateMachine.StateTransition(DeviceEvent.Start);
        stateMachine.StateTransition(DeviceEvent.Complete);

        // Task.Run(async () =>
        // {
        //     try
        //     {
        //         stopToken.ThrowIfCancellationRequested();
        //         Console.WriteLine("设备开始运行");
        //         await Task.Delay(TimeSpan.FromSeconds(20), stopToken);
        //         Console.WriteLine("设备运行完毕");
        //         stateMachine.StateTransition(DeviceEvent.Complete);
        //     }
        //     catch (OperationCanceledException ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        //     finally
        //     {
        //         if (stopCts.IsCancellationRequested)
        //         {
        //             stopCts = new();
        //         }
        //     }
        // });
    }


    public void Pause()
    {
        stateMachine.StateTransition(DeviceEvent.Pause);
    }

    public void Stop()
    {
        stateMachine.StateTransition(DeviceEvent.Stop);
    }
}
