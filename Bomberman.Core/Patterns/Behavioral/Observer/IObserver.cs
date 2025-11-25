namespace Bomberman.Core.Patterns.Behavioral.Observer;

 public interface IObserver
    {
        void Update(ISubject subject, object eventData);
    }
