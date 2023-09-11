using System;
using UnityEngine;

namespace Utility.Observable
{
    public class ObservableObject<T> : UnityEngine.Object
    {
        private T value;

        public event Action<T> onValueChange;

        public ObservableObject(T value)
        {
            this.value = value;
            onValueChange += (a) => { };
        }

        public T GetValue()
        {
            return value;
        }

        public void SetValue(T newValue)
        {
            value = newValue;
            onValueChange(value);
        }
    }
}
