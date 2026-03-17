namespace DesignPattern.Observable;

public interface IObserver<T>
{
    public void OnUpdate(T t);
}
