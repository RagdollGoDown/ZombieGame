using UnityEngine.Events;

namespace Utility.Observable
{
    public class ReadOnlyObservableObject<T> : ObservableObject<T>
    {
        public ReadOnlyObservableObject(ObservableObject<T> observable) : base(observable.GetValue())
        {
            observable.AddActionToOnValueChange(base.SetValue);
        }

        public new void SetValue(T newValue)
        {
            throw new System.NotImplementedException();
        }
    }
}