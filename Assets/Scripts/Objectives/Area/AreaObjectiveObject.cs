using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class AreaObjectiveObject : ObjectiveObject
{  
    private void OnTriggerStay(Collider other)
    {
        if (!other.tag.Equals("Player") || !getIsOn()) return;

        GetObjectEvent().Invoke();
    }
}
public class AreaObjective : Objective
{
    private float _currentStayTime;
    private readonly float _totalStayTime;

    public AreaObjective(UnityEvent onComplete, string objectiveText, 
        float totalStayTime, AreaObjectiveObject areaObjective) : 
        base(onComplete,objectiveText)
    {
        AddObjectiveObject(areaObjective);
        _totalStayTime = totalStayTime;
        areaObjective.GetObjectEvent().AddListener(stayInObjective);
    }

    public override void Begin()
    {
        base.Begin();

        _currentStayTime = 0;
    }

    private void stayInObjective()
    {
        _currentStayTime += Time.fixedDeltaTime;
        if (_currentStayTime >= _totalStayTime) Complete();
    }

    public override float getCompletenessRatio()
    {
        return _currentStayTime / _totalStayTime;
    }
}