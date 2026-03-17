public interface IStateHandle<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    void Handle(StateMachine<TState, TTrigger> stateMatchine);

    void Enter();

    void Exit();
}
