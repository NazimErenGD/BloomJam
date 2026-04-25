using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Thin view layer for the dialogue system. Bind TMP/Button refs in the inspector;
// DialogueStarter drives this through SetLine / ShowChoices / Hide.
public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panel;

    [Header("Line")]
    [SerializeField] private TMP_Text speakerLabel;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button continueButton;

    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private bool showInactiveChoices = false;

    private readonly List<Button> _spawnedChoiceButtons = new();
    private Action _onContinue;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
        }
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        ClearChoices();
        if (panel != null) panel.SetActive(false);
    }

    public void SetLine(string speaker, string text, Action onContinue)
    {
        if (speakerLabel != null) speakerLabel.text = speaker ?? string.Empty;
        if (lineText != null) lineText.text = text ?? string.Empty;
        _onContinue = onContinue;

        ClearChoices();
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(onContinue != null);
            continueButton.interactable = onContinue != null;
        }
    }

    public void ShowChoices(IList<Choice> choices, Action<Choice> onPick)
    {
        ClearChoices();
        if (continueButton != null) continueButton.gameObject.SetActive(false);

        if (choicesContainer == null || choiceButtonPrefab == null || choices == null)
            return;

        foreach (var choice in choices)
        {
            if (!showInactiveChoices && !choice.isActive) continue;

            var button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.gameObject.SetActive(true);

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = choice.Text;

            var image = button.GetComponent<Image>();
            if (image != null) image.color = ParseButtonColor(choice.ButtonColor);

            button.interactable = choice.isActive;

            var captured = choice;
            button.onClick.AddListener(() => onPick?.Invoke(captured));

            _spawnedChoiceButtons.Add(button);
        }
    }

    public void ClearChoices()
    {
        for (int i = 0; i < _spawnedChoiceButtons.Count; i++)
        {
            if (_spawnedChoiceButtons[i] != null)
            {
                _spawnedChoiceButtons[i].onClick.RemoveAllListeners();
                Destroy(_spawnedChoiceButtons[i].gameObject);
            }
        }
        _spawnedChoiceButtons.Clear();
    }

    private void HandleContinueClicked()
    {
        var cb = _onContinue;
        _onContinue = null;
        cb?.Invoke();
    }

    // Maps a ButtonColor string from JSON to a Unity Color.
    // Accepts common color names; also accepts hex strings via ColorUtility.
    private static Color ParseButtonColor(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Color.white;

        if (raw.StartsWith("#") && ColorUtility.TryParseHtmlString(raw, out var hex))
            return hex;

        return raw.Trim().ToLowerInvariant() switch
        {
            "red"     => Color.red,
            "green"   => Color.green,
            "blue"    => Color.blue,
            "yellow"  => Color.yellow,
            "cyan"    => Color.cyan,
            "magenta" => Color.magenta,
            "purple"  => new Color(0.5f, 0f, 0.5f),
            "orange"  => new Color(1f, 0.5f, 0f),
            "pink"    => new Color(1f, 0.41f, 0.71f),
            "brown"   => new Color(0.55f, 0.27f, 0.07f),
            "white"   => Color.white,
            "black"   => Color.black,
            "gray"    => Color.gray,
            "grey"    => Color.gray,
            _ => ColorUtility.TryParseHtmlString(raw, out var parsed) ? parsed : Color.white
        };
    }
}
