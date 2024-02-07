using UnityEngine;

namespace Objectives.Button
{
    public class ButtonObjective : Objective
    {
        private int totalButtonsToPress;
        private int _buttonsToPress;

        protected override void Awake()
        {
            base.Awake();

            foreach (ObjectiveObject button in GetObjectiveObjects())
            {
                button.GetObjectEvent().AddListener(PressButton);
            }
        }

        public override void Begin()
        {
            base.Begin();

            _buttonsToPress =  GetObjectiveObjects().Count;
            Debug.Log("began");
            Debug.Log(_buttonsToPress);
        }

        private void PressButton()
        {
            Debug.Log("Button Pressed");
            Debug.Log(_buttonsToPress);

            _buttonsToPress -= 1;
            Debug.Log(_buttonsToPress);
            if (_buttonsToPress <= 0) { Complete(); }
        }

        public override float GetCompletenessRatio()
        {
            return _buttonsToPress / totalButtonsToPress;
        }
    }
}
