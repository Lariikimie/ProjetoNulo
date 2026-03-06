// PlayerCameraController.cs
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraHolder; // pai da câmera (CameraHolder)

    [Header("Sensibilidade")]
    [SerializeField] private float mouseSensitivityX = 200f;
    [SerializeField] private float mouseSensitivityY = 150f;
    [SerializeField] private float controllerSensitivityX = 150f;
    [SerializeField] private float controllerSensitivityY = 130f;

    [Header("Limites de Rotação Vertical")]
    [SerializeField] private float minYAngle = -30f;
    [SerializeField] private float maxYAngle = 60f;

    [Header("Inputs (nome dos eixos)")]
    [SerializeField] private string mouseXInput = "Mouse X";
    [SerializeField] private string mouseYInput = "Mouse Y";
    [SerializeField] private string rightStickXInput = "RightStickHorizontal"; // criar no Input Manager
    [SerializeField] private string rightStickYInput = "RightStickVertical";   // criar no Input Manager

    private float rotationX = 0f; // vertical (cima/baixo)

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCameraRotation();
    }

    private void HandleCameraRotation()
    {
        // Mouse
        float mouseX = Input.GetAxis(mouseXInput) * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis(mouseYInput) * mouseSensitivityY * Time.deltaTime;

        // Analógico direito
        float stickX = Input.GetAxis(rightStickXInput) * controllerSensitivityX * Time.deltaTime;
        float stickY = Input.GetAxis(rightStickYInput) * controllerSensitivityY * Time.deltaTime;

        float finalX = mouseX + stickX;
        float finalY = mouseY + stickY;

        // Rotaciona o player no eixo Y (esquerda/direita)
        transform.Rotate(Vector3.up * finalX);

        // Rotação vertical aplicada no cameraHolder
        rotationX -= finalY;
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }
}
