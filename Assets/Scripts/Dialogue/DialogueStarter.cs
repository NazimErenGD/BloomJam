using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Single point of entry for starting and running a dialogue.
// Place one DialogueStarter in the scene, wire its DialogueUI ref,
// then call DialogueStarter.Instance.Begin("MyJsonName") from any code.
public class DialogueStarter : MonoBehaviour
{
    public static DialogueStarter Instance { get; private set; }

    [SerializeField] private DialogueUI ui;

    [Header("Gameplay Suspension")]
    [Tooltip("Player whose state machines + camera should pause while a dialogue is open.")]
    [SerializeField] private Player player;
    [Tooltip("Optional: explicit FirstPersonCamera reference. If left empty, DialogueStarter falls back to Player.CameraController, then a scene search.")]
    [SerializeField] private FirstPersonCamera cameraOverride;
    [Tooltip("Optionally also disable the input handler so movement/jump/sprint actions don't accumulate while in dialogue.")]
    [SerializeField] private bool suspendInputHandler = false;
    [Tooltip("If true, zeroes the player's Rigidbody velocity when suspending so they don't slide.")]
    [SerializeField] private bool zeroVelocityOnSuspend = true;

    private FirstPersonCamera _resolvedCamera;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    private DialogueContext _context;
    private DialogueEntry _currentEntry;
    private int _currentLineIndex;
    private bool _isRunning;

    private CursorLockMode _savedCursorLockMode;
    private bool _savedCursorVisible;

    public bool IsRunning => _isRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Begin(string jsonName)
    {
        if (_isRunning)
        {
            Debug.LogWarning("DialogueStarter: a dialogue is already running.");
            return;
        }
        if (string.IsNullOrEmpty(jsonName))
        {
            Debug.LogError("DialogueStarter: jsonName is empty.");
            return;
        }

        DialogueManager.LoadFromJson(jsonName);

        _context = new DialogueContext();
        var entry = DialogueManager.GetFirstMatchingDialogue(_context);
        if (entry == null)
        {
            Debug.LogWarning($"DialogueStarter: no matching dialogue in '{jsonName}'.");
            return;
        }

        _isRunning = true;
        SetGameplaySuspended(true);
        if (ui != null) ui.Show();
        OnDialogueStarted?.Invoke();

        EnterEntry(entry);
    }

    private void Update()
    {
        if (!_isRunning) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.vKey.wasPressedThisFrame)
        {
            if (ui != null) ui.TryAdvance();
        }
    }

    private void EnterEntry(DialogueEntry entry)
    {
        _currentEntry = entry;
        _currentLineIndex = 0;

        ApplySetFlags(entry);

        if (entry.Lines == null || entry.Lines.Count == 0)
        {
            ShowChoicesOrEnd();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = _currentEntry.Lines[_currentLineIndex];
        bool hasMoreLines = _currentLineIndex < _currentEntry.Lines.Count - 1;

        if (ui != null)
        {
            ui.SetLine(line.Speaker, line.Text, AdvanceLine);
        }

        if (!hasMoreLines && (_currentEntry.Choices == null || _currentEntry.Choices.Count == 0))
        {
            // last line, no choices: continue button will end the dialogue
        }
    }

    private void AdvanceLine()
    {
        if (!_isRunning || _currentEntry == null) return;

        _currentLineIndex++;
        if (_currentLineIndex < _currentEntry.Lines.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            ShowChoicesOrEnd();
        }
    }

    private void ShowChoicesOrEnd()
    {
        var choices = _currentEntry?.Choices;
        if (choices == null || choices.Count == 0)
        {
            End();
            return;
        }

        if (ui != null)
        {
            ui.ShowChoices(choices, OnChoicePicked);
        }
    }

    private void OnChoicePicked(Choice choice)
    {
        if (choice == null || !choice.isActive) return;

        if (string.IsNullOrEmpty(choice.NextDialogueId))
        {
            End();
            return;
        }

        var next = DialogueManager.GetDialogueById(choice.NextDialogueId);
        if (next == null)
        {
            Debug.LogWarning($"DialogueStarter: NextDialogueId '{choice.NextDialogueId}' not found.");
            End();
            return;
        }

        if (next.isOneTimeOnly)
        {
            DialogueManager.ReadDialogueIds.Add(next.Id);
        }
        DialogueManager.ChoiceFactory(next, _context);

        EnterEntry(next);
    }

    private void End()
    {
        _isRunning = false;
        _currentEntry = null;
        _currentLineIndex = 0;
        if (ui != null) ui.Hide();
        SetGameplaySuspended(false);
        OnDialogueEnded?.Invoke();
    }

    private void SetGameplaySuspended(bool suspended)
    {
        var cam = ResolveCamera();

        if (suspended)
        {
            _savedCursorLockMode = Cursor.lockState;
            _savedCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (cam != null) cam.enabled = false;

            if (player != null)
            {
                if (player.MovementMachine != null) player.MovementMachine.enabled = false;
                if (player.ActionMachine != null) player.ActionMachine.enabled = false;
                if (suspendInputHandler && player.InputHandler != null) player.InputHandler.enabled = false;

                if (zeroVelocityOnSuspend && player.Rb != null)
                {
                    player.Rb.linearVelocity = Vector3.zero;
                    player.Rb.angularVelocity = Vector3.zero;
                }
            }

            if (cam == null)
            {
                Debug.LogWarning("DialogueStarter: no FirstPersonCamera could be resolved. Camera will keep responding to mouse. Assign 'Camera Override' or 'Player.CameraController'.", this);
            }
        }
        else
        {
            if (player != null)
            {
                if (player.MovementMachine != null) player.MovementMachine.enabled = true;
                if (player.ActionMachine != null) player.ActionMachine.enabled = true;
                if (suspendInputHandler && player.InputHandler != null) player.InputHandler.enabled = true;
            }

            if (cam != null) cam.enabled = true;

            Cursor.lockState = _savedCursorLockMode;
            Cursor.visible = _savedCursorVisible;
        }
    }

    private FirstPersonCamera ResolveCamera()
    {
        if (_resolvedCamera != null) return _resolvedCamera;

        if (cameraOverride != null) { _resolvedCamera = cameraOverride; return _resolvedCamera; }
        if (player != null && player.CameraController != null) { _resolvedCamera = player.CameraController; return _resolvedCamera; }

        _resolvedCamera = FindFirstObjectByType<FirstPersonCamera>();
        return _resolvedCamera;
    }

    private static void ApplySetFlags(DialogueEntry entry)
    {
        var setFlags = entry?.SetFlags;
        if (setFlags == null) return;
        for (int i = 0; i < setFlags.Count; i++)
        {
            var name = setFlags[i];
            if (!GameFlagsService.TryAddByName(name))
            {
                Debug.LogWarning($"DialogueStarter: SetFlags entry '{name}' is not a valid GameFlags value.");
            }
        }
    }
}
