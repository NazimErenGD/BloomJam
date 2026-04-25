using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Player's main body (Rigidbody) to rotate Left/Right")]
    public Transform PlayerBody;
    [Tooltip("The Camera Holder or Camera itself")]
    private Transform CameraTransform;
    public PlayerInputHandler InputHandler;

    [Header("Settings")]
    public float MouseSensitivity = 15f;
    public float TopClamp = -90f;
    public float BottomClamp = 90f;

    private float _xRotation = 0f; // Internal tracker for up/down

    private void Start()
    {
        // Lock cursor to center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraTransform = Camera.main.transform;
        CameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void LateUpdate()
    {
        // 1. Read Input
        if (InputHandler == null) return;
        Vector2 mouseInput = InputHandler.LookInput;

        // 2. Calculate Rotation (Frame-rate independent is handled by Input System usually, 
        // but multiplying by Time.deltaTime is safer if input is Raw)
        float mouseX = mouseInput.x * MouseSensitivity * Time.deltaTime;
        float mouseY = mouseInput.y * MouseSensitivity * Time.deltaTime;

        // 3. Vertical Rotation (Pitch) - Moving Camera ONLY
        _xRotation -= mouseY; // Minus because Unity Y is inverted for pitch
        _xRotation = Mathf.Clamp(_xRotation, TopClamp, BottomClamp);

        CameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 4. Horizontal Rotation (Yaw) - Moving ENTIRE BODY
        // We rotate the body so that "Forward" in the State Machine 
        // matches where we are looking.
        PlayerBody.Rotate(Vector3.up * mouseX);
    }
}