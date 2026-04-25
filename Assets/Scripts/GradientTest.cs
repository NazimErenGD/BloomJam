using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GradientTest : MonoBehaviour
{
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Image _image;
    [SerializeField] private InputAction _inputAction;

    [Tooltip("Amount the bar advances per 'ProgressBar.Advance' action (or per debug key press).")]
    [SerializeField] private float _stepSize = 0.20f;

    [Header("Dialogue Action Keys")]
    [SerializeField] private string _advanceKey = "ProgressBar.Advance";
    [SerializeField] private string _setFillKey = "ProgressBar.SetFill";
    [SerializeField] private string _resetKey = "ProgressBar.Reset";

    private float _fillAmount = 0f;

    private void OnEnable()
    {
        DialogueActions.Register(_advanceKey, AdvanceStep);
        DialogueActions.Register(_setFillKey, SetFillFromString);
        DialogueActions.Register(_resetKey, ResetBar);
    }

    private void OnDisable()
    {
        DialogueActions.Unregister(_advanceKey);
        DialogueActions.Unregister(_setFillKey);
        DialogueActions.Unregister(_resetKey);
    }

    private void Start()
    {
        Apply();
        _inputAction.Enable();
        _inputAction.performed += OnInputActionPerformed;
    }

    private void OnDestroy()
    {
        _inputAction.performed -= OnInputActionPerformed;
        _inputAction.Disable();
    }

    private void OnInputActionPerformed(InputAction.CallbackContext context) => AdvanceStep();

    public void AdvanceStep()
    {
        _fillAmount = Mathf.Clamp01(_fillAmount + _stepSize);
        Apply();
    }

    private void SetFillFromString(string raw)
    {
        if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
        {
            _fillAmount = Mathf.Clamp01(v);
            Apply();
        }
        else
        {
            Debug.LogWarning($"GradientTest: SetFill arg '{raw}' is not a valid float.");
        }
    }

    public void ResetBar()
    {
        _fillAmount = 0f;
        Apply();
    }

    private void Apply()
    {
        if (_image == null) return;
        _image.fillAmount = _fillAmount;
        _image.color = _gradient.Evaluate(_fillAmount);
    }
}
