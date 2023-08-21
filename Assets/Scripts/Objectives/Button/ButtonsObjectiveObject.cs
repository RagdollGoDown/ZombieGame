using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Objectives.Button
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Interactable))]
    public class ButtonsObjectiveObject : ObjectiveObject
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
    public class ButtonsObjective : Objective
    {
        private readonly int _totalButtonsToPress;
        private int _buttonsToPress;

        public ButtonsObjective(UnityEvent onComplete, string objectiveText,
            ButtonsObjectiveObject[] buttons) :
            base(onComplete, objectiveText)
        {
            _totalButtonsToPress = buttons.Length;

            foreach (ButtonsObjectiveObject button in buttons)
            {
                AddObjectiveObject(button);
                button.GetObjectEvent().AddListener(PressButton);
            }
        }

        public override void Begin()
        {
            base.Begin();

            _buttonsToPress = _totalButtonsToPress;
        }

        private void PressButton()
        {
            _buttonsToPress -= 1;

            if (_buttonsToPress <= 0) { Complete(); }
        }

        public override float GetCompletenessRatio()
        {
            return _buttonsToPress / _totalButtonsToPress;
        }
    }
}