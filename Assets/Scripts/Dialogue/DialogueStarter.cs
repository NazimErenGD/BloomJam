using System;
using System.Collections.Generic;
using UnityEngine;

// Single point of entry for starting and running a dialogue.
// Place one DialogueStarter in the scene, wire its DialogueUI ref,
// then call DialogueStarter.Instance.Begin("MyJsonName") from any code.
public class DialogueStarter : MonoBehaviour
{
    public static DialogueStarter Instance { get; private set; }

    [SerializeField] private DialogueUI ui;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    private DialogueContext _context;
    private DialogueEntry _currentEntry;
    private int _currentLineIndex;
    private bool _isRunning;

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
        if (ui != null) ui.Show();
        OnDialogueStarted?.Invoke();

        EnterEntry(entry);
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
        OnDialogueEnded?.Invoke();
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
