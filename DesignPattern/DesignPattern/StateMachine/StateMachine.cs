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

    IStateHandle<TState, TTrigger>? lastStateHandle;

    public void StateTransition(TTrigger e)
    {
        if (CanFire(e))
        {
            var (nextState, nextHandle) = transitions[CurrentState][e];
            CurrentState = nextState;
            lastStateHandle?.Exit();
            nextHandle?.Handle(this);
            nextHandle?.Enter();
            lastStateHandle = nextHandle;
        }
        else
        {
            throw new InvalidOperationException($"状态转换失败");
        }
    }

}
