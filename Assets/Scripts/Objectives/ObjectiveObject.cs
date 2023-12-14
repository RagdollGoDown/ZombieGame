using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives
{
    public abstract class ObjectiveObject : MonoBehaviour
    {
        private bool isOn;

        private Animator _anim;

        private UnityEvent _objectEvent = new UnityEvent();

        private UnityEvent _onTurnedOn = new UnityEvent();
        private UnityEvent _onTurnedOff = new UnityEvent();

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            ReadyOnAwake();
        }
        protected virtual void ReadyOnAwake() { }

        /// <summary>
        /// Gets the object event, the event activated 
        /// when interacted with by the player(not necessarily through the interactable script)
        /// </summary>
        /// <returns></returns>
        public UnityEvent GetObjectEvent()
        {
            return _objectEvent;
        }

        protected UnityEvent GetOnTurnedOnEvent()
        {
            return _onTurnedOn;
        }

        protected UnityEvent GetOnTurnedOffEvent()
        {
            return _onTurnedOff;
        }

        /// <summary>
        /// get the object ready to be interacted with
        /// </summary>
        public void turnOn()
        {
            isOn = true;

            if (_anim != null)
            { _anim.SetBool("isOn", isOn); }

            _onTurnedOn.Invoke();
        }

        /// <summary>
        /// make the object no longer interactable with
        /// </summary>
        public void turnOff()
        {
            isOn = false;

            if (_anim != null)
            { _anim.SetBool("isOn", isOn); }

            _onTurnedOff.Invoke();
        }

        /// <summary>
        /// tells you if the object is on
        /// </summary>
        /// <returns> the isOn bool</returns>
        public bool getIsOn() { return isOn; }
    }
}
