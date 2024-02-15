using UnityEngine;
using System;

namespace Objectives.Button
{
    public class ButtonObjective : Objective
    {
        private int _buttonsToPress;

        private Action onButtonPress;

        protected override void Awake()
        {
            base.Awake();

            if (GetObjectiveObjects().Count == 0)
            {
                throw new Exception("No buttons to press");
            }

            foreach (ObjectiveObject button in GetObjectiveObjects())
            {
                button.GetObjectEvent().AddListener(PressButton);
            }
        }

        public override void Begin()
        {
            base.Begin();

            _buttonsToPress =  GetObjectiveObjects().Count;
        }

        private void PressButton()
        {
            _buttonsToPress -= 1;

            onButtonPress?.Invoke();
            if (_buttonsToPress <= 0) { Complete(); }
        }

        public override float GetCompletenessRatio()
        {
            return (float)(GetObjectiveObjects().Count - _buttonsToPress) / GetObjectiveObjects().Count;
        }

        public void ObserveOnButtonPress(Action action)
        {
            onButtonPress += action;
        }

        public void StopObservingOnButtonPress(Action action)
        {
            onButtonPress -= action;
        }
    }
}
