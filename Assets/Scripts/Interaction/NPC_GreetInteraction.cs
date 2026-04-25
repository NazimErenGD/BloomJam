using UnityEngine;

public class NPC_GreetInteraction : MonoBehaviour, IInteractable
{
    [Tooltip("Name of the JSON file under StreamingAssets/Dialogues (without extension).")]
    [SerializeField] private string dialogueJsonName;

    public void Interact()
    {
        if (string.IsNullOrEmpty(dialogueJsonName))
        {
            Debug.LogWarning($"{name}: dialogueJsonName is not set.", this);
            return;
        }

        if (DialogueStarter.Instance == null)
        {
            Debug.LogError("NPC_GreetInteraction: no DialogueStarter in scene.", this);
            return;
        }

        DialogueStarter.Instance.Begin(dialogueJsonName);
    }
}
