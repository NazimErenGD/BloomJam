using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Player : MonoBehaviour
{
    [Header("Sub-Systems")]
    public PlayerMovementStateMachine MovementMachine;
    public PlayerActionStateMachine ActionMachine;
    public PlayerInputHandler InputHandler;

    [Header("Data")]
    public Rigidbody Rb;
    public Transform PlayerBody;
    // Add Health, Inventory, etc. here

    [Header("Editor Automation")]
    [SerializeField] private string _movementStatesPath = "Assets/Game/States/Movement";
    [SerializeField] private string _actionStatesPath = "Assets/Game/States/Actions";
    [Header("Camera")]
    public FirstPersonCamera CameraController;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        PlayerBody = GetComponent<Transform>();
        //InputHandler = GetComponent<PlayerInputHandler>();

        // Initialize Parallel Machines
        MovementMachine.Initialize(this);
        ActionMachine.Initialize(this);
        if (CameraController != null) { CameraController.InputHandler = InputHandler; }
        PlayerBody.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    // EDITOR ONLY: Auto-fill the state arrays
    [ContextMenu("Auto-Load States")]
    private void LoadStates()
    {
#if UNITY_EDITOR
        MovementMachine._possibleStates = LoadStatesFromFolder(_movementStatesPath);
        ActionMachine._possibleStates = LoadStatesFromFolder(_actionStatesPath);
        Debug.Log("States Loaded Successfully.");
#endif
    }

#if UNITY_EDITOR
    private PlayerStateSO[] LoadStatesFromFolder(string path)
    {
        string[] guids = AssetDatabase.FindAssets("t:PlayerStateSO", new[] { path });
        List<PlayerStateSO> states = new List<PlayerStateSO>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            PlayerStateSO state = AssetDatabase.LoadAssetAtPath<PlayerStateSO>(assetPath);
            if (state != null) states.Add(state);
        }
        return states.ToArray();
    }
#endif
#if UNITY_EDITOR
    [ContextMenu("Load State Machines")]
    private void LoadStateMachinesAndInputHandler()
    {
        MovementMachine = Object.FindFirstObjectByType<PlayerMovementStateMachine>();
        ActionMachine = Object.FindFirstObjectByType<PlayerActionStateMachine>();
        InputHandler = Object.FindFirstObjectByType<PlayerInputHandler>();
    }
#endif
}