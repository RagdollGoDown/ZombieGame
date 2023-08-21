using UnityEngine.Events;

namespace Utility
{
    public interface Interactor
    {
        void OnInteractableEntered(Interaction interaction);

        void OnInteractableExit(Interaction interaction);
    }
}
