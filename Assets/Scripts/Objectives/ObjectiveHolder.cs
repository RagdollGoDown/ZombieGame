using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ObjectiveHolder : MonoBehaviour
{
    [SerializeField] private List<ObjectiveObject> objects;
    [SerializeField] private UnityEvent onObjectiveStart;
    [SerializeField] private UnityEvent onObjectiveComplete;
    [SerializeField] private string objectiveText;

    protected abstract Objective BuildPureObjective(UnityEvent onComplete,string objectiveText);

    public Objective Build(UnityEvent onComplete)
    {
        Objective obj = BuildPureObjective(onComplete,objectiveText);
        obj.GetOnObjectiveStartedEvent().AddListener(() => { onObjectiveStart.Invoke(); });
        obj.GetOnObjectiveCompleteEvent().AddListener(() => { onObjectiveComplete.Invoke(); });

        return obj;
    }

    protected List<ObjectiveObject> GetObjects() { return objects; }
}
