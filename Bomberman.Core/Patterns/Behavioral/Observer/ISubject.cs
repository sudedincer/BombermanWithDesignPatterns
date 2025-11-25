namespace Bomberman.Core.Patterns.Behavioral.Observer;

public interface ISubject
{
    void AddObserver(IObserver observer);
    void RemoveObserver(IObserver observer);
    void Notify(object eventData);
}