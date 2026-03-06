using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui a Main Camera ou o CameraHolder. Se deixar vazio, o script tenta usar Camera.main.")]
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
    [SerializeField] private string horizontalAxis = "Horizontal";   // teclado + analógico esquerdo
    [SerializeField] private string verticalAxis = "Vertical";     // teclado + analógico esquerdo
    [SerializeField] private string runButton = "Run";          // botão de correr no controle

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100.0f; // sensibilidade
    [SerializeField] private bool invertY = false;  // inverte eixo Y
    [SerializeField] private float pitchClamp = 85f;    // limite pra cima/baixo
    [SerializeField] private float lookSmoothTime = 0.03f;  // suavização SmoothDamp

    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private bool isRunning;

    // ---- Variáveis de rotação (copiadas/adaptadas do outro projeto) ----
    private Camera cam;
    private float yaw;   // rotação em Y (esquerda/direita) do player
    private float pitch; // rotação em X (cima/baixo) da câmera

    private float yawTarget, pitchTarget;
    private float yawSmoothed, pitchSmoothed;
    private float yawVel, pitchVel;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Fallback de referência de câmera
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Start()
    {
        currentStamina = maxStamina;

        // Pega referência da câmera
        if (cameraTransform != null)
            cam = cameraTransform.GetComponent<Camera>() ?? Camera.main;
        else
            cam = Camera.main;

        // ---- Inicialização da rotação (igual ao outro script) ----
        yaw = transform.localEulerAngles.y;

        if (cam != null)
        {
            pitch = cam.transform.localEulerAngles.x;
            // converte para -180..180
            if (pitch > 180f) pitch -= 360f;
        }

        yawTarget = yawSmoothed = yaw;
        pitchTarget = pitchSmoothed = pitch;

        // ---- Trava o cursor no começo ----
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleStamina();

        HandleMouseLook();
        HandleCursorLock();
    }

    private void LateUpdate()
    {
        // Aplica a rotação suave ao PLAYER (yaw) e à CÂMERA (pitch)
        transform.rotation = Quaternion.Euler(0f, yawSmoothed, 0f);

        if (cam != null)
        {
            cam.transform.localRotation = Quaternion.Euler(pitchSmoothed, 0f, 0f);
        }
        else if (cameraTransform != null)
        {
            // Se a câmera estiver em outro filho (ex.: CameraHolder), ainda assim gira o holder
            cameraTransform.localRotation = Quaternion.Euler(pitchSmoothed, 0f, 0f);
        }
    }

    // ───────────────────────── MOVIMENTO ─────────────────────────

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis(horizontalAxis);
        float vertical = Input.GetAxis(verticalAxis);

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical);

        if (inputDir.sqrMagnitude < 0.01f)
            return;

        // Referência para o movimento continua sendo a câmera
        Transform reference = cameraTransform != null ? cameraTransform : transform;

        Vector3 camForward = reference.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = reference.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 move = camForward * vertical + camRight * horizontal;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

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
            velocity.y = -2f; // mantém preso ao chão
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

    // ───────────────────────── MOUSE LOOK ─────────────────────────

    private void HandleMouseLook()
    {
        // se o cursor não estiver travado, não gira (evita bug quando estiver mexendo em UI)
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // usar Time.deltaTime deixa a sensibilidade mais estável em FPS diferentes
        float sensitivity = mouseSensitivity * Time.deltaTime;

        // acumula alvo de rotação
        yawTarget += mx * sensitivity;
        pitchTarget += (invertY ? my : -my) * sensitivity;

        // limita o olhar pra cima/baixo
        pitchTarget = Mathf.Clamp(pitchTarget, -pitchClamp, pitchClamp);

        // suaviza com SmoothDampAngle (igual ao projeto antigo)
        yawSmoothed = Mathf.SmoothDampAngle(yawSmoothed, yawTarget, ref yawVel, lookSmoothTime);
        pitchSmoothed = Mathf.SmoothDampAngle(pitchSmoothed, pitchTarget, ref pitchVel, lookSmoothTime);
    }

    // ───────────────────────── CURSOR LOCK ─────────────────────────

    private void HandleCursorLock()
    {
        // ESC libera o cursor (útil pra sair no editor / menus)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Clique esquerdo volta a travar, se não estiver travado
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}