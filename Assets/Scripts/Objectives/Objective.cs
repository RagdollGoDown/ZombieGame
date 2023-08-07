using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Objective
{
    private readonly UnityEvent _onComplete;
    private readonly UnityEvent _onStarted;

    public List<ObjectiveObject> objectiveObjects;

    private string _objectiveText;

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

    /// <summary>
    /// Begins the objective, making it possible to complete and activating it's objectiveObjects
    /// </summary>
    public virtual void Begin()
    {
        _onStarted.Invoke();

        foreach (ObjectiveObject o in objectiveObjects) {
            o.turnOn();
        }
    }

    /// <summary>
    /// Completes the objective and turns off the objectiveObjects
    /// </summary>
    public virtual void Complete()
    {
        _onComplete.Invoke();

        foreach (ObjectiveObject o in objectiveObjects)
        {
            o.turnOff();
        }
    }

    /// <summary>
    /// Used to modify the listeners of the onComplete event by the holder
    /// </summary>
    /// <returns>the event</returns>
    public UnityEvent GetOnObjectiveCompleteEvent()
    {
        return _onComplete;
    }

    /// <summary>
    /// Used to modify the listeners of the onStarted event by the holder
    /// </summary>
    /// <returns>the event</returns>
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
