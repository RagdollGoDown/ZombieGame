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

    public Objective(UnityEvent completeEvent, string objectiveText)
    {
        objectiveObjects = new();
        _onComplete = completeEvent;
        _onStarted = new();
        _objectiveText = objectiveText;
    }

    public Objective(UnityEvent completeEvent, string objectiveText,
        List<ObjectiveObject> objectiveObjects) : this(completeEvent,objectiveText)
    {
        this.objectiveObjects = new List<ObjectiveObject>(objectiveObjects);
    }

    /// <summary>
    /// calculates how complete the objective is
    /// </summary>
    /// <returns>the ratio of completeness</returns>
    public abstract float GetCompletenessRatio();

    /// <summary>
    /// add an objectiveObject to the list of the concerned objects
    /// </summary>
    /// <param name="objObj">the added object</param>
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

    /// <summary>
    /// the text describing what need to be done to complete the objective
    /// </summary>
    /// <returns>the text</returns>
    public string GetObjectiveText() { return _objectiveText; }
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

    /// <summary>
    /// Gets the object event, the event activated 
    /// when interacted with by the player(not necessarily through the interactable script)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// get the object ready to be interacted with
    /// </summary>
    public void turnOn() {
        isOn = true;

        if (_anim != null) 
        { _anim.SetBool("isOn", isOn); }

        _onTurnedOn.Invoke();
    }

    /// <summary>
    /// make the object no longer interactable with
    /// </summary>
    public void turnOff() {
        isOn = false;

        if (_anim != null) 
        { _anim.SetBool("isOn", isOn); }

        _onTurnedOff.Invoke();
    }

    /// <summary>
    /// tells you if the object is on
    /// </summary>
    /// <returns> the isOn bool</returns>
    public bool getIsOn() { return isOn; }
}
