using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Objectives.Selectable
{
    public class SelectableObjectiveHolder : ObjectiveHolder
    {
        // these act as proxys to hold the selectable objective
        protected override Objective BuildPureObjective(UnityEvent onComplete, string objectiveText)
        {
            return new SelectableObjective(onComplete, objectiveText, GetObjects());
        }
    }

}
