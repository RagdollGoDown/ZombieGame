using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AreaObjectiveHolder : ObjectiveHolder
{
    [SerializeField] private float totalStayTime;

    public override Objective Build(UnityEvent onComplete)
    {
        List<ObjectiveObject> objects = GetObjects();

        if (objects.Count == 0) 
            throw new System.ArgumentException("No objective objects in : " + name);

        if (objects[0] is not AreaObjectiveObject)
            throw new System.ArgumentException("Wrong Objective object type given to " + name);

        return new AreaObjective(onComplete, totalStayTime, (AreaObjectiveObject)objects[0]);
    }
}
