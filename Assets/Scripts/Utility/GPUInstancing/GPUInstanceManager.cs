using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstanceManager : MonoBehaviour
{
    private static GPUInstanceManager currentManager;
    private static List<GPUInstance> instances;

    //------------------------------------unity events

    private void Awake()
    {
        if (instances == null)
        {
            instances = new List<GPUInstance>();
            Debug.Log("new instance list");
        }
    }

    private void Update()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            instances[i].UpdateInstance(Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        if (currentManager == null)
        {
            currentManager = this;
        }
        else
        {
            Debug.Log("There is possibly two GPUInstanceManager");
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (currentManager == this) currentManager = null;
    }


    //----------------------------------------instance adding and removing

    /// <summary>
    /// Adds a gpu instance to the list of instances to be rendered
    /// </summary>
    /// <param name="instance">the instance in question</param>
    public static void AddInstance(GPUInstance instance)
    {
        if (instances == null)
        {
            instances = new List<GPUInstance>();
            Debug.Log("new instance list");
        }

        instances.Add(instance);
    }

    /// <summary>
    /// Removes a gpu instance from the list of instances to be rendered
    /// </summary>
    /// <param name="instance">the instance in question</param>
    public static void RemoveInstance(GPUInstance instance)
    {
        if (instances == null) return;

        instances.Remove(instance);
    }
}
