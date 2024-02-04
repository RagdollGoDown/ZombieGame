using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives.Button
{
    public class ButtonObjective : Objective
    {
        private int totalButtonsToPress;
        private int _buttonsToPress;

        protected override void Awake()
        {
            base.Awake();

            totalButtonsToPress = GetObjectiveObjects().Count;


            foreach (ObjectiveObject button in GetObjectiveObjects())
            {
                button.GetObjectEvent().AddListener(PressButton);
            }
        }

        public override void Begin()
        {
            base.Begin();

            _buttonsToPress = totalButtonsToPress;
        }

        private void PressButton()
        {
            _buttonsToPress -= 1;

            if (_buttonsToPress <= 0) { Complete(); }
        }

        public override float GetCompletenessRatio()
        {
            return _buttonsToPress / totalButtonsToPress;
        }
    }
}
