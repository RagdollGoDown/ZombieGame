using UnityEngine.Events;

namespace Utility.Observable
{
    public class ReadOnlyObservableObject<T> : ObservableObject<T>
    {
        public ReadOnlyObservableObject(ObservableObject<T> observable) : base(observable.GetValue())
        {
            observable.onValueChange += DiscretelySetValue;
        }

        private void DiscretelySetValue(T newValue)
        {
            base.SetValue(newValue);
        }

        public new void SetValue(T newValue)
        {
            throw new System.NotImplementedException();
        }
    }
}