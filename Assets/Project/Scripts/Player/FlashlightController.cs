using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Light flashlightLight; // Luz da lanterna (Point/Spot)

    [Header("Follow da Câmera")]
    [Tooltip("Transform da câmera que define a direção da lanterna (geralmente Main Camera).")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Pivot da lanterna (objeto pai, filho do Player).")]
    [SerializeField] private Transform pivotTransform;

    [Tooltip("Offset local da lanterna em relação ao pivot (em unidades locais).")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, 0.5f);

    [Header("Bateria")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainPerSecond = 5f;

    [Tooltip("Valor inicial da bateria ao começar o jogo")]
    [SerializeField] private float startBattery = 100f;

    [Header("Inputs")]
    [SerializeField] private bool usarControle = false;
    [SerializeField] private string flashlightButton = "Flashlight"; // Botão no Input Manager

    private float currentBattery;
    private bool isOn = true;

    private void Start()
    {
        if (flashlightLight == null)
        {
            flashlightLight = GetComponentInChildren<Light>();
        }

        // Se não definir no Inspector, tenta achar Main Camera
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Se não definir no Inspector, tenta usar o pai como pivot
        if (pivotTransform == null)
        {
            pivotTransform = transform.parent;
            if (pivotTransform == null)
                Debug.LogWarning("[Flashlight] pivotTransform não atribuído e o objeto não tem pai. Crie um FlashlightPivot como pai.");
        }

        // Garante o offset inicial da lanterna em relação ao pivot
        transform.localPosition = localOffset;

        currentBattery = Mathf.Clamp(startBattery, 0f, maxBattery);
        SetFlashlight(true); // começa ligada (se quiser começar desligada, troque pra false)
    }

    private void Update()
    {
        HandleInput();
        HandleBattery();
    }

    private void LateUpdate()
    {
        UpdatePivotFollowCamera();
    }

    // ================= FOLLOW DA CÂMERA =======================

    private void UpdatePivotFollowCamera()
    {
        if (cameraTransform == null || pivotTransform == null)
            return;

        // Faz o pivot apontar na mesma direção que a câmera (apenas rotação)
        pivotTransform.rotation = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);

        // Mantém a lanterna no offset definido em relação ao pivot
        transform.localPosition = localOffset;
    }

    // ================= INPUT =======================

    private void HandleInput()
    {
        // Teclado
        bool toggleKey = Input.GetKeyDown(KeyCode.F);

        // Controle (se habilitado)
        bool toggleButton = usarControle && Input.GetButtonDown(flashlightButton);

        if (toggleKey || toggleButton)
        {
            // Só permite ligar se tiver bateria
            if (!isOn && currentBattery <= 0f)
                return;

            SetFlashlight(!isOn);
        }
    }

    // ================= BATERIA =======================

    private void HandleBattery()
    {
        if (isOn && currentBattery > 0f)
        {
            currentBattery -= drainPerSecond * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

            if (currentBattery <= 0f)
            {
                SetFlashlight(false);
            }
        }
    }

    private void SetFlashlight(bool value)
    {
        isOn = value;

        if (flashlightLight != null)
            flashlightLight.enabled = isOn;
    }

    // ---- MÉTODOS PÚBLICOS PARA INVENTÁRIO / HUD / OUTROS SISTEMAS ----

    public float GetBatteryPercent()
    {
        if (maxBattery <= 0f) return 0f;
        return currentBattery / maxBattery;
    }

    public float GetCurrentBattery()
    {
        return currentBattery;
    }

    public float GetMaxBattery()
    {
        return maxBattery;
    }

    /// <summary>
    /// Recarrega a bateria da lanterna em "amount" unidades.
    /// Ex: se maxBattery = 100, amount = 50 → recarrega metade.
    /// </summary>
    public void RechargeBattery(float amount)
    {
        if (amount <= 0f) return;

        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        // Se tiver bateria de novo, podemos ligar a lanterna
        if (currentBattery > 0f && !isOn)
        {
            SetFlashlight(true);
        }

        Debug.Log("[Lanterna] Bateria recarregada. Atual: " + currentBattery);
    }

    /// <summary>
    /// Força liga/desliga da lanterna por outros sistemas
    /// (ex.: NoteCineController, Cutscenes).
    /// </summary>
    public void ForceFlashlight(bool enabled)
    {
        SetFlashlight(enabled);
    }
}