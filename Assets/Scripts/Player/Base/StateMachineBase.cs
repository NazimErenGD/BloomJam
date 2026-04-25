using UnityEngine;
using System.Collections.Generic;

public abstract class StateMachineBase : MonoBehaviour
{
    [Header("Config")]
    // This array gets filled by the Player script automation
    public PlayerStateSO[] _possibleStates;
    public PlayerInputHandler PlayerInputHandler;

    protected Dictionary<PlayerStateSO, PlayerStateSO> _stateInstances;
    protected Dictionary<System.Type, PlayerStateSO> _statesByType;
    public PlayerStateSO CurrentState { get; protected set; }
    public PlayerStateSO PreviousState { get; protected set; }
    private PlayerStateSO _idleState;
    protected Player _player;

    public virtual void Initialize(Player player)
    {
        _player = player;
        PlayerInputHandler = _player.InputHandler;
        _stateInstances = new Dictionary<PlayerStateSO, PlayerStateSO>();
        _statesByType = new Dictionary<System.Type, PlayerStateSO>();
        // Create Runtime Copies
        foreach (var stateAsset in _possibleStates)
        {
            var instance = Instantiate(stateAsset);
            // We defer 'Init' to the concrete machine so we can cast it correctly
            _stateInstances.Add(stateAsset, instance);

            var type = instance.GetType();
            if (!_statesByType.ContainsKey(type))
            {
                _statesByType.Add(type, instance);
                if (instance is IIdle)
                    _idleState = instance;
            }
        }

        // SUBSCRIBE TO EVENTS (Observer Pattern)
        if (PlayerInputHandler != null)
            _player.InputHandler.OnInputEvent += OnInputReceived;
    }
    public PlayerStateSO GetState<T>() where T : PlayerStateSO
    {
        System.Type type = typeof(T);
        if (_statesByType.ContainsKey(type))
            return _statesByType[type];
        return null;
    }

    public void ChangeToIdleState()
    {
        ChangeState(_idleState);
    }



    protected virtual void OnEnable()
    {
        // Re-subscribe after a previous OnDisable (e.g. when DialogueStarter
        // suspended the machines for a dialogue). On the very first OnEnable
        // _player is still null because Initialize hasn't run yet, so we skip;
        // Initialize will subscribe in that case.
        if (_player != null && PlayerInputHandler != null)
            PlayerInputHandler.OnInputEvent += OnInputReceived;
    }

    protected virtual void OnDisable()
    {
        // UNSUBSCRIBE (Safety)
        if (_player != null && PlayerInputHandler != null)
            PlayerInputHandler.OnInputEvent -= OnInputReceived;
    }

    protected virtual void Update()
    {
        CurrentState?.LogicUpdate();
    }

    protected virtual void FixedUpdate()
    {
        // Only meaningful for Movement, but harmless for Actions
        if (CurrentState is MovementStateSO moveState)
            moveState.PhysicsUpdate();
    }

    private void OnInputReceived(InputSignalSO inputSignal)
    {
        //Debug.Log("OnInputReceived: " + inputSignal.name.ToString() + " CurrentState: " + CurrentState.name.ToString());
        if (CurrentState == null) return;

        // 1. Check Dictionary
        PlayerStateSO targetAsset = CurrentState?.GetTransitionFor(inputSignal);
        //Debug.Log("TargetAsset: " + targetAsset?.name?.ToString());
        // 2. Validate Target
        if (targetAsset != null && _stateInstances.ContainsKey(targetAsset))
        {
            PlayerStateSO targetInstance = _stateInstances[targetAsset];
            //Debug.Log("TargetInstance: " + targetInstance.name.ToString());
            // 3. Check Conditions
            if (targetInstance.CanEnter())
            {
                //Debug.Log("ChangingState: " + targetInstance.name.ToString());
                ChangeState(targetInstance);
            }
        }
    }
    public virtual void ChangeStateRequest<T>() where T : PlayerStateSO
    {
        PlayerStateSO targetState = GetState<T>();
        if (targetState == null || !targetState.CanEnter()) return;
        ChangeState(targetState);
    }

    public virtual void ChangeStateOrDefaultRequest<T>(PlayerStateSO defaultState) where T : PlayerStateSO
    {
        PlayerStateSO targetState = GetState<T>();
        if (targetState == null || !targetState.CanEnter())
        {
            ChangeState(defaultState);
            return;
        }
        ChangeState(targetState);
    }

    protected virtual void ChangeState(PlayerStateSO newState)
    {
        if (!newState.CanEnter())
            return;
        CurrentState?.Exit();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.Enter();
    }
}