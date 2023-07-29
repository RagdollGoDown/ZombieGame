using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonsObjectiveHolder : ObjectiveHolder
{

    protected override Objective BuildPureObjective(UnityEvent onComplete)
    {
        List<ButtonsObjectiveObject> objects = new();

        foreach(ObjectiveObject oo in GetObjects())
        {
            if (oo is not ButtonsObjectiveObject)
                throw new System.ArgumentException("Wrong Objective object type given to " + name
                    + " given in : " + oo.name);

            objects.Add((ButtonsObjectiveObject)oo);
        }

        if (objects.Count == 0)
            throw new System.ArgumentException("No objective objects in : " + name);

        return new ButtonsObjective(onComplete, objects.ToArray());
    }
}
