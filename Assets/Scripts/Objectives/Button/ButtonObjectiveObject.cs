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
        private static LayerMask DEFAULT = 0;
        private static LayerMask HIGHLIGHTED = 10;

        private Interactable _interactable;

        [SerializeField] private List<GameObject> meshs;

        protected override void ReadyOnAwake()
        {
            base.ReadyOnAwake();

            GetOnTurnedOnEvent().AddListener(ReadyToPress);
            _interactable = GetComponent<Interactable>();

            Invoke(nameof(SwitchInteractable), 0.1f);
        }

        private void SwitchInteractable()
        {
            _interactable.enabled = !_interactable.enabled;
        }

        public void PressButton()
        {
            GetObjectEvent().Invoke();
            turnOff();

            foreach (GameObject go in meshs)
            {
                go.layer = DEFAULT;
            }

            SwitchInteractable();
        }

        private void ReadyToPress()
        {
            foreach (GameObject go in meshs)
            {
                go.layer = HIGHLIGHTED;
            }

            SwitchInteractable();
        }
    }
}