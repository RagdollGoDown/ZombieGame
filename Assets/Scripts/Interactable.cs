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

    private Interaction _interaction;

    private PlayerController _playerInArea;

    private void Awake()
    {
        _interaction = new Interaction(interactEvent, interactionText);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.TryGetComponent(out PlayerController pc))
        {
            pc.AddInteractListener(_interaction);
            _playerInArea = pc;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && other.TryGetComponent(out PlayerController pc))
        {
            pc.RemoveInteractListener(_interaction);
            _playerInArea = pc;
        }
    }

    /*
    Changes the interation to have the new text
    if the player is in the zone the it changes it for him too
     */
    public void SetInteractionText(string newText)
    {
            
        if (_playerInArea)_playerInArea.RemoveInteractListener(_interaction);

        _interaction = new Interaction(interactEvent, newText);

        if (_playerInArea) _playerInArea.AddInteractListener(_interaction);
    }

    /*
     * returns the player in the area where it can interact with this or null
     */
    public PlayerController GetPlayerInArea()
    {
        return _playerInArea;
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
