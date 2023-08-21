using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.Observable
{
    public class ObservableObject<T> : Object
    {
        private T value;

        private UnityEvent<T> onValueChange;

        public ObservableObject(T value)
        {
            this.value = value;

            onValueChange = new();
        }

        public T GetValue()
        {
            return value;
        }

        public void SetValue(T newValue)
        {
            value = newValue;
            onValueChange.Invoke(value);
        }

        public void AddActionToOnValueChange(UnityAction<T> action)
        {
            onValueChange.AddListener(action);
        }

        public void RemoveActionToOnValueChange(UnityAction<T> action)
        {
            onValueChange.RemoveListener(action);
        }
    }
}
