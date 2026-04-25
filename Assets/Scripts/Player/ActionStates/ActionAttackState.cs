using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Interaction Action State", menuName = "Player/States/Action/Interaction")]
public class ActionInteractionState : ActionStateSO
{
    [Header("Interaction Settings")]
    [Tooltip("Layer mask to ignore the player and only hit valid targets.")]
    [SerializeField] private LayerMask _interactionLayerMask;
    [SerializeField] private float _interactionRange = 10f;
    [SerializeField] private float _interactionDuration = 0.5f;

    public override void Enter()
    {
        Debug.Log("Interacting");
        
        // 1. Delegate the coroutine to an existing MonoBehaviour context (e.g., _stateMachine or _player)
        // This keeps the ScriptableObject completely stateless.
        if (_stateMachine is MonoBehaviour stateMachineMono)
        {
            stateMachineMono.StartCoroutine(InteractionAnimationRoutine());
        }
        else
        {
            Debug.LogError("StateMachine is not a MonoBehaviour. Cannot run Coroutine.");
            return;
        }

        ExecuteInteractionRaycast();
    }

    private void ExecuteInteractionRaycast()
    {
        // 2. Properly evaluate the boolean return of the raycast.
        // 3. Implement a LayerMask to prevent self-collision.
        bool hitSuccess = Physics.Raycast(
            _player.transform.position, 
            Camera.main.transform.forward, 
            out RaycastHit hit, 
            _interactionRange, 
            _interactionLayerMask
        );

        if (!hitSuccess)
        {
            Debug.Log("Interaction Raycast missed entirely.");
            return; // Early return on failure reduces nested if-statements.
        }

        // 4. Use TryGetComponent efficiently now that we know we hit a valid surface.
        if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
        {
            Debug.Log($"Interaction Raycast found: {hit.collider.gameObject.name}");
            interactable.Interact();
        }
        else
        {
            Debug.Log($"Interaction Raycast hit {hit.collider.gameObject.name}, but it is not IInteractable.");
        }
    }

    public override void Exit()
    {
        base.Exit();
        // Ensure any necessary cleanup happens here.
    }

    // 5. Encapsulate the routine within the state logic, but run it on the state machine's thread.
    private IEnumerator InteractionAnimationRoutine()
    {
        yield return new WaitForSeconds(_interactionDuration);
        OnInteractionAnimationEnded();
    }

    private void OnInteractionAnimationEnded()
    {
        Debug.Log("Interaction Animation Ended");
        _stateMachine.ChangeToIdleState();
    }
}

