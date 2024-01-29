using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeEnableEvents : MonoBehaviour
{
    public event System.Action OnEnableEvent;
    public event System.Action OnDisableEvent;

    private void OnEnable()
    {
        OnEnableEvent?.Invoke();
    }

    private void OnDisable()
    {
        OnDisableEvent?.Invoke();
    }
}
