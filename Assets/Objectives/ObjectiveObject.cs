using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives
{
    public abstract class ObjectiveObject : MonoBehaviour
    {
        private bool isOn;

        private UnityEvent _objectEvent = new UnityEvent();
        private Animator animator;

        public UnityEvent onTurnedOn;
        public UnityEvent onTurnedOff;

        private void Awake()
        {
            ReadyOnAwake();
        }
        protected virtual void ReadyOnAwake() 
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Gets the object event, the event activated 
        /// when interacted with by the player(not necessarily through the interactable script)
        /// </summary>
        /// <returns></returns>
        public UnityEvent GetObjectEvent()
        {
            return _objectEvent;
        }

        /// <summary>
        /// get the object ready to be interacted with
        /// </summary>
        public void turnOn()
        {
            isOn = true;
            
            onTurnedOn.Invoke();
        }

        /// <summary>
        /// make the object no longer interactable with
        /// </summary>
        public void turnOff()
        {
            isOn = false;

            onTurnedOff.Invoke();
        }

        /// <summary>
        /// tells you if the object is on
        /// </summary>
        /// <returns> the isOn bool</returns>
        public bool getIsOn() { return isOn; }

        //-----------------------------------utility

        /// <summary>
        /// Sets a bool in the animator to true
        /// a bit of a cope for the lack of an event function of this
        /// </summary>
        /// <param name="parameterName">the name of the parameter</param>
        public void SetBoolTrueInAnimator(string parameterName)
        {
            if (animator != null) animator.SetBool(parameterName, true);
        }

        /// <summary>
        /// Sets a bool in the animator to false
        /// a bit of a cope for the lack of an event function of this
        /// </summary>
        /// <param name="parameterName">the name of the parameter</param>
        public void SetBoolFalseInAnimator(string parameterName)
        {
            if (animator != null) animator.SetBool(parameterName, false);
        }
    }
}
