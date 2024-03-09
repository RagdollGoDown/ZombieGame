using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Utility.Events
{
    public class InvokeTriggerEvents : MonoBehaviour
    {
        public UnityEvent<Collider> OnTriggerEnterEvent;
        public UnityEvent<Collider> OnTriggerStayEvent;
        public UnityEvent<Collider> OnTriggerExitEvent;

        private void OnTriggerEnter(Collider collider)
        {
            OnTriggerEnterEvent?.Invoke(collider);
        }

        private void OnTriggerStay(Collider collider)
        {
            OnTriggerStayEvent?.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            OnTriggerExitEvent?.Invoke(collider);
        }
    }
}
