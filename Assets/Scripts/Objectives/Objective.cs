using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Objective
{
    private readonly UnityEvent _onComplete;
    private readonly UnityEvent _onStarted;

    public List<ObjectiveObject> objectiveObjects;

    public Objective(UnityEvent completeEvent)
    {
        objectiveObjects = new();
        _onComplete = completeEvent;
        _onStarted = new();
    }

    public Objective(UnityEvent completeEvent, List<ObjectiveObject> objectiveObjects) : this(completeEvent)
    {
        this.objectiveObjects = new List<ObjectiveObject>(objectiveObjects);
    }

    public abstract float getCompletenessRatio();

    public void AddObjectiveObject(ObjectiveObject objObj)
    {
        objectiveObjects.Add(objObj);
    }

    public virtual void Begin()
    {
        _onStarted.Invoke();

        foreach (ObjectiveObject o in objectiveObjects) {
            o.turnOn();
        }
    }

    public virtual void Complete()
    {
        _onComplete.Invoke();

        foreach (ObjectiveObject o in objectiveObjects)
        {
            o.turnOff();
        }
    }

    public abstract int GetScore();

    public UnityEvent GetOnObjectiveCompleteEvent()
    {
        return _onComplete;
    }

    public UnityEvent GetOnObjectiveStartedEvent()
    {
        return _onStarted;
    }
}

public abstract class ObjectiveObject : MonoBehaviour
{
    private bool isOn;

    private Animator _anim;

    private UnityEvent _objectEvent = new UnityEvent();

    private UnityEvent _onTurnedOn = new UnityEvent();
    private UnityEvent _onTurnedOff = new UnityEvent();

    private void Awake()
    {
        _anim = GetComponent<Animator>();

        ReadyOnAwake();
    }
    protected virtual void ReadyOnAwake(){}

    public UnityEvent GetObjectEvent() 
    {
        return _objectEvent;
    }

    protected UnityEvent GetOnTurnedOnEvent()
    {
        return _onTurnedOn;
    }

    protected UnityEvent GetOnTurnedOffEvent()
    {
        return _onTurnedOff;
    }

    public void turnOn() {
        isOn = true;

        if (_anim != null) 
        { _anim.SetBool("isOn", isOn); }

        _onTurnedOn.Invoke();
    }

    public void turnOff() {
        isOn = false;

        if (_anim != null) 
        { _anim.SetBool("isOn", isOn); }

        _onTurnedOff.Invoke();
    }

    public bool getIsOn() { return isOn; }
}
