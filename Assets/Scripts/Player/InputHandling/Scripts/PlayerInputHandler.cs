using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Signal Configuration")]
    [Tooltip("Drag the Asset meant for Jumping here")]
    public InputSignalSO JumpSignal;
    [Tooltip("Drag the Asset meant for Attacking here")]
    public InputSignalSO InteractionSignal;
    [Tooltip("Drag the Asset meant for Moving here")]
    public InputSignalSO MoveSignal;
    [Tooltip("Drag the Asset meant for Sprinting here")]
    public InputSignalSO SprintSignal;
    [Header("Camera Input")]
    [HideInInspector] public Vector2 LookInput { get; private set; }

    [HideInInspector] public Vector2 MovementInput { get; private set; }

    // THE EVENT BUS: Any machine can listen to this.
    public event Action<InputSignalSO> OnInputEvent;
    public event Action<InputSignalSO> OnSprintEvent;
    public event Action<InputSignalSO> OnSprintCanceledEvent;

    private PlayerControls _controls;
    private InputAction _moveAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;
    private InputAction _interactionAction;
    private InputAction _lookAction;

    private void Awake()
    {
        MovementInput = Vector2.zero;
        LookInput = Vector2.zero;

        _controls = new PlayerControls();
        _moveAction = _controls.Movement.MoveSignal;
        _sprintAction = _controls.Movement.LeftShiftSignal;
        _jumpAction = _controls.Movement.SpaceSignal;
        _interactionAction = _controls.Movement.KeyboardESignal;
        _lookAction = _controls.Camera.LookDelta;
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _sprintAction.Enable();
        _jumpAction.Enable();
        _interactionAction.Enable();
        _lookAction.Enable();

        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;
        _sprintAction.performed += OnSprint;
        _sprintAction.canceled += OnSprintCanceled;
        _jumpAction.performed += OnJump;
        _interactionAction.performed += OnInteraction;
        _lookAction.performed += OnLook;
        _lookAction.canceled += OnLookCanceled;
    }

    private void OnDisable()
    {
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= OnMove;
        _sprintAction.performed -= OnSprint;
        _sprintAction.canceled -= OnSprintCanceled;
        _jumpAction.performed -= OnJump;
        _interactionAction.performed -= OnInteraction;
        _lookAction.performed -= OnLook;
        _lookAction.canceled -= OnLookCanceled;

        _moveAction.Disable();
        _sprintAction.Disable();
        _jumpAction.Disable();
        _interactionAction.Disable();
        _lookAction.Disable();
    }

    private void OnDestroy()
    {
        _controls?.Dispose();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 newInput = ctx.ReadValue<Vector2>();

        if (MovementInput == Vector2.zero && newInput != Vector2.zero)
        {
            OnInputEvent?.Invoke(MoveSignal);
        }
        MovementInput = newInput;
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();
    }

    public void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        LookInput = Vector2.zero;
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnSprintEvent?.Invoke(SprintSignal);
        }
    }

    public void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            OnSprintCanceledEvent?.Invoke(SprintSignal);
        }
    }

    // Link this to your "Jump" Action in the PlayerInput component
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnInputEvent?.Invoke(JumpSignal);
        }
    }

    // Link this to your "Fire" Action
    public void OnInteraction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnInputEvent?.Invoke(InteractionSignal);
        }
    }
}
