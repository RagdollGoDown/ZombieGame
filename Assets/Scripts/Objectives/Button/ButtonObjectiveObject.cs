using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Objectives.Button
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Interactable))]
    public class ButtonObjectiveObject : ObjectiveObject
    {
        private Interactable _interactable;

        protected override void ReadyOnAwake()
        {
            base.ReadyOnAwake();
            _interactable = GetComponent<Interactable>();

            if (_interactable == null)
                throw new System.Exception("Button Objective Object not on same object as interactor component");
        }

        public void PressButton()
        {
            GetObjectEvent().Invoke();
            turnOff();
        }
    }
}