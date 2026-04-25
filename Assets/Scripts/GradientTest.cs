using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class GradientTest : MonoBehaviour
{
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Image _image;
    [SerializeField] private InputAction _inputAction;
    private float _fillAmount = 0f;
    void Start()
    {
        _image.fillAmount = _fillAmount;
        _inputAction.Enable();
        _inputAction.performed += OnInputActionPerformed;
    }

    private void OnInputActionPerformed(InputAction.CallbackContext context)
    {
        _fillAmount += 0.20f;
        _image.fillAmount = _fillAmount;
        _image.color = _gradient.Evaluate(_fillAmount);
    }
}
