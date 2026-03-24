using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui a Main Camera (filha da cabeça do player).")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 15f;
    [SerializeField] private float staminaRecovery = 10f;
    [SerializeField] private float minStaminaToRun = 10f;

    [Header("Inputs (nome dos eixos/botões)")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private string runButton = "Run";

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100.0f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float pitchClamp = 85f;
    [SerializeField] private float lookSmoothTime = 0.01f; // resposta rápida para FPS

    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private bool isRunning;

    private Camera cam;
    private float yaw;
    private float pitch;

    private float yawTarget, pitchTarget;
    private float yawSmoothed, pitchSmoothed;
    private float yawVel, pitchVel;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Start()
    {
        currentStamina = maxStamina;
        if (cameraTransform != null)
            cam = cameraTransform.GetComponent<Camera>() ?? Camera.main;
        else
            cam = Camera.main;

        yaw = transform.localEulerAngles.y;
        if (cam != null)
        {
            pitch = cam.transform.localEulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
        }
        yawTarget = yawSmoothed = yaw;
        pitchTarget = pitchSmoothed = pitch;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Bloqueia movimentação se o jogo estiver pausado
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        HandleMovement();
        HandleGravity();
        HandleStamina();
        HandleMouseLook();
        HandleCursorLock();
    }

    private void LateUpdate()
    {
        // Rotação do corpo do player (yaw)
        transform.rotation = Quaternion.Euler(0f, yawSmoothed, 0f);

        // Rotação da câmera (pitch)
        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(pitchSmoothed, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis(horizontalAxis);
        float vertical = Input.GetAxis(verticalAxis);

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical);
        if (inputDir.sqrMagnitude < 0.01f)
            return;

        // Movimento sempre relativo à câmera (primeira pessoa)
        Transform reference = cameraTransform != null ? cameraTransform : transform;
        Vector3 camForward = reference.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = reference.right; camRight.y = 0f; camRight.Normalize();
        Vector3 move = camForward * vertical + camRight * horizontal;
        if (move.sqrMagnitude > 1f) move.Normalize();

        bool runKey = Input.GetKey(KeyCode.LeftShift);
        bool runButtonPressed = Input.GetButton(runButton);
        bool wantsToRun = (runKey || runButtonPressed) && vertical > 0.1f;
        isRunning = wantsToRun && currentStamina > minStaminaToRun;
        float speed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleStamina()
    {
        if (isRunning)
            currentStamina -= staminaDrain * Time.deltaTime;
        else
            currentStamina += staminaRecovery * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        float sensitivity = mouseSensitivity * Time.deltaTime;
        yawTarget += mx * sensitivity;
        pitchTarget += (invertY ? my : -my) * sensitivity;
        pitchTarget = Mathf.Clamp(pitchTarget, -pitchClamp, pitchClamp);
        yawSmoothed = Mathf.SmoothDampAngle(yawSmoothed, yawTarget, ref yawVel, lookSmoothTime);
        pitchSmoothed = Mathf.SmoothDampAngle(pitchSmoothed, pitchTarget, ref pitchVel, lookSmoothTime);
    }

    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}