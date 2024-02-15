using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Utility
{
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private UnityEvent interactEvent;

        [SerializeField] private string interactionText;

        private Interaction _interaction;

        private Interactor interactorInArea;

        private void Awake()
        {
            _interaction = new Interaction(interactEvent, interactionText);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out Interactor interactor))
            {
                if (enabled) interactor.OnInteractableEntered(_interaction);

                interactorInArea = interactor;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player" && other.TryGetComponent(out Interactor interactor))
            {
                if (enabled) interactor.OnInteractableExit(_interaction);

                interactorInArea = null;
            }
        }

        /*
        Changes the interation to have the new text
        if the player is in the zone the it changes it for him too
         */
        public void SetInteractionText(string newText)
        {
            if (interactorInArea == null) interactorInArea.OnInteractableExit(_interaction);

            interactionText = newText;
            _interaction = new Interaction(interactEvent, interactionText);

            if (interactorInArea == null) interactorInArea.OnInteractableEntered(_interaction);
        }

        /*
         * returns the player in the area where it can interact with this or null
         */
        public Interactor GetInteractorInArea()
        {
            return interactorInArea;
        }

        private void OnEnable()
        {
            interactorInArea?.OnInteractableEntered(_interaction);
        }

        private void OnDisable()
        {
            interactorInArea?.OnInteractableExit(_interaction);
        }
    }

    public class Interaction
    {
        private readonly UnityEvent interactionAction;
        private readonly string interactionText;

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
}