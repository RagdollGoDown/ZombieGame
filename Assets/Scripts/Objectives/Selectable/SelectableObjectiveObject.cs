using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives.Selectable
{
    public class SelectableObjectiveObject : ObjectiveObject
    {
        [SerializeField] private ObjectiveHolder heldSelectableObjective;

        private Objective selectableObjective;

        private bool hasBeenCompleted;

        private SelectableObjective selector;

        /// <summary>
        /// this attempts to start this object's held objective
        /// </summary>
        public void TryToSelectObjective()
        {
            if (selector == null) Debug.LogAssertion("No objective assigned to this objective object");
            Debug.Log("try");
            if (selector != null && selector.CanSelectObjective() && !hasBeenCompleted)
            {
                Debug.Log("tried");

                selectableObjective.GetOnObjectiveCompleteEvent().AddListener(() => hasBeenCompleted = true);

                selectableObjective.Begin();
            }
        }

        /// <summary>
        /// Gives the selectable objective held by this object
        /// </summary>
        /// <returns>the held objective</returns>
        public Objective GetSelectableObjective()
        {
            if (selector == null) throw new System.Exception("The selectable objective object needs his selector " +
                "to be given to him before getting the objective");

            if (selectableObjective == null)
            {
                selectableObjective = heldSelectableObjective.Build(selector.GetOnObjectiveCompleteEvent());
            }

            return selectableObjective;
        }

        /// <summary>
        /// Give this object the selector which tells him if he can start his held objective or not
        /// </summary>
        /// <param name="selector">the selectableObjective</param>
        public void SetSelector(SelectableObjective selector)
        {
            this.selector = selector;
        }
    }

    public class SelectableObjective : Objective
    {
        private readonly Objective[] selectedObjectives;

        private bool oneObjectiveIsActive;

        public SelectableObjective(UnityEvent completeEvent, string objectiveText,
            List<ObjectiveObject> objectiveObjects) : 
            base(completeEvent, objectiveText, objectiveObjects)
        {
            selectedObjectives = objectiveObjects.Select(obj => obj as SelectableObjectiveObject).Select(obj => 
            {
                obj.SetSelector(this);
                return obj.GetSelectableObjective();
            }
            ).ToArray();

            foreach (Objective obj in selectedObjectives)
            {
                obj.GetOnObjectiveCompleteEvent().AddListener(CompleteSelectedObjective);
            }
        }

        public override float GetCompletenessRatio()
        {
            return selectedObjectives.Select(obj => obj.GetCompletenessRatio()).Sum() / selectedObjectives.Length;
        }

        /// <summary>
        /// If an new objective can begin or not
        /// </summary>
        /// <returns>false if one of the objective is already started</returns>
        public bool CanSelectObjective()
        {
            if (!oneObjectiveIsActive)
            {
                oneObjectiveIsActive = true;
                return true;
            }

            return false;
        }

        private void CompleteSelectedObjective()
        {
            Debug.Log("done");
            oneObjectiveIsActive = false;
        }
    }
}