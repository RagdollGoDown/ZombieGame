using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent interactEvent;

    [SerializeField] private string interactionText;

    private Interaction interaction;

    private void Awake()
    {
        interaction = new Interaction(interactEvent, interactionText);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.TryGetComponent(out PlayerController pc))
        {
            pc.AddInteractListener(interaction);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && other.TryGetComponent(out PlayerController pc))
        {
            pc.RemoveInteractListener(interaction);
        }
    }
}

public class Interaction
{
    private UnityEvent interactionAction;
    private string interactionText;

    public Interaction(UnityEvent interactionAction, string interactionText)
    {
        this.interactionAction = interactionAction;
        this.interactionText = interactionText;
    }

    public UnityEvent GetAction()
    {
        return interactionAction;
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}
