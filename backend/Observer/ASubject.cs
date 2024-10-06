using System;

namespace backend.Observer
{
    public abstract class ASubject
    {
        protected List<IObserver> observersList = new List<IObserver>();

        public void AddObserver(IObserver observer)
        {
            observersList.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            observersList.Remove(observer);
        }
    }
}
