using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives.Button
{
    public class ButtonsObjectiveHolder : ObjectiveHolder
    {
        protected override Objective BuildPureObjective(UnityEvent onComplete, string objectiveText)
        {
            List<ButtonObjectiveObject> objects = new();

            foreach (ObjectiveObject oo in GetObjects())
            {
                if (oo is not ButtonObjectiveObject)
                    throw new System.ArgumentException("Wrong Objective object type given to " + name
                        + " given in : " + oo.name);

                objects.Add((ButtonObjectiveObject)oo);
            }

            if (objects.Count == 0)
                throw new System.ArgumentException("No objective objects in : " + name);

            return new ButtonObjective(onComplete, objectiveText, objects.ToArray());
        }
    }

}
