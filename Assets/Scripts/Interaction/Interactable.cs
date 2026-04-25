using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        TryGetComponent<IInteractable>(out var interactable);
        if (interactable != null)
        {
            interactable.Interact();
        }
    }
}
