using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ObjectiveHolder : MonoBehaviour
{
    [SerializeField] private List<ObjectiveObject> objects;

    public abstract Objective Build(UnityEvent onComplete);

    protected List<ObjectiveObject> GetObjects() { return objects; }
}
