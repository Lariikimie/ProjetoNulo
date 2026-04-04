using UnityEngine;
using TMPro;

public class SafeUIController : MonoBehaviour
{
    public static bool IsSafeOpen { get; private set; } = false;

    [Header("Referências de UI")]
    [SerializeField] private GameObject safePanel;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text hintText;

    [Header("Configuração")]
    [SerializeField] private int codeLength = 8;
    [SerializeField] private char placeholderChar = '_';

    [Header("Sons")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip changeNumberSound;
    [SerializeField] private AudioClip wrongCodeSound;
    [SerializeField] private AudioClip correctCodeSound;

    [Header("Inputs (Configuráveis)")]
    [SerializeField] private KeyCode increaseNumberKey = KeyCode.W;
    [SerializeField] private KeyCode decreaseNumberKey = KeyCode.S;
    [SerializeField] private KeyCode confirmKey = KeyCode.E;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    [SerializeField] private float navigationCooldown = 0.1f;

    [Header("UI - Dica Customizável")]
    [SerializeField] private string customHintText = "A/D (Setas) = Navegar | W/S = Aumentar/Diminuir | E = Confirmar | ESC = Sair";

    private Safe currentSafe;
    private int[] currentCode = new int[8];
    private int currentPosition = 0;
    private float navigationTimer = 0f;
    private float previousTimeScale = 1f;
    private float openDelay = 0.1f; // Delay para evitar fechamento imediato

    private void Start()
    {
        if (safePanel != null)
            safePanel.SetActive(false);

        for (int i = 0; i < codeLength; i++)
            currentCode[i] = 0;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateCodeText();
        UpdateHintText();
    }

    private void Update()
    {
        if (!IsSafeOpen)
            return;

        navigationTimer -= Time.unscaledDeltaTime;
        openDelay -= Time.unscaledDeltaTime;

        HandleNavigation();
        HandleNumberChange();
        HandleConfirmCancel();
    }

    public void OpenSafe(Safe safe)
    {
        if (safe == null)
            return;

        currentSafe = safe;
        currentPosition = 0;

        for (int i = 0; i < codeLength; i++)
            currentCode[i] = 0;

        if (safePanel == null)
        {
            Debug.LogWarning("[SafeUI] safePanel nao atribuido.");
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        safePanel.SetActive(true);
        IsSafeOpen = true;
        navigationTimer = 0f;
        openDelay = 0.1f; // Reset delay para ignorar inputs iniciais

        UpdateCodeText();
        UpdateHintText();

        Debug.Log("[SafeUI] UI do cofre ABERTA.");
    }

    private void CloseSafe()
    {
        if (safePanel != null)
            safePanel.SetActive(false);

        IsSafeOpen = false;
        Time.timeScale = previousTimeScale;

        currentSafe = null;
        currentPosition = 0;

        Debug.Log("[SafeUI] UI do cofre FECHADA.");
    }

    private void HandleNavigation()
    {
        if (navigationTimer > 0f)
            return;

        int direction = 0;

        if (UINavigationInput.Instance != null)
        {
            if (UINavigationInput.Instance.RightPressedThisFrame())
                direction = 1;
            else if (UINavigationInput.Instance.LeftPressedThisFrame())
                direction = -1;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                direction = 1;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                direction = -1;
        }

        if (direction != 0)
        {
            currentPosition = (currentPosition + direction + codeLength) % codeLength;
            PlaySound(changeNumberSound);
            UpdateCodeText();
            navigationTimer = navigationCooldown;
        }
    }

    private void HandleNumberChange()
    {
        // Ignora inputs no primeiro frame após abrir
        if (openDelay > 0f)
            return;

        int numberDirection = 0;

        if (UINavigationInput.Instance != null)
        {
            if (UINavigationInput.Instance.UpPressedThisFrame())
                numberDirection = 1;
            else if (UINavigationInput.Instance.DownPressedThisFrame())
                numberDirection = -1;
        }
        else
        {
            if (Input.GetKeyDown(increaseNumberKey))
                numberDirection = 1;
            else if (Input.GetKeyDown(decreaseNumberKey))
                numberDirection = -1;
        }

        if (numberDirection != 0)
        {
            currentCode[currentPosition] = (currentCode[currentPosition] + numberDirection + 10) % 10;
            PlaySound(changeNumberSound);
            UpdateCodeText();
        }
    }

    private void HandleConfirmCancel()
    {
        // Ignora inputs no primeiro frame após abrir
        if (openDelay > 0f)
            return;

        if (Input.GetKeyDown(confirmKey))
        {
            ConfirmCode();
        }

        if (Input.GetKeyDown(cancelKey))
        {
            CloseSafe();
        }
    }

    private void ConfirmCode()
    {
        if (currentSafe == null)
        {
            Debug.LogWarning("[SafeUI] Nenhum cofre associado.");
            CloseSafe();
            return;
        }

        string codeString = "";
        for (int i = 0; i < codeLength; i++)
            codeString += currentCode[i].ToString();

        string correct = currentSafe.GetCorrectCode();

        if (codeString == correct)
        {
            PlaySound(correctCodeSound);
            currentSafe.OnCorrectCodeEntered();
            CloseSafe();
        }
        else
        {
            PlaySound(wrongCodeSound);
            currentSafe.OnWrongCodeEntered();
            for (int i = 0; i < codeLength; i++)
                currentCode[i] = 0;
            currentPosition = 0;
            UpdateCodeText();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.clip = clip;
        audioSource.PlayOneShot(clip);
    }

    private void UpdateCodeText()
    {
        if (codeText == null)
            return;

        string display = "";
        for (int i = 0; i < codeLength; i++)
        {
            if (i == currentPosition)
                display += "[" + currentCode[i] + "]";
            else
                display += currentCode[i];
        }

        codeText.text = display;
    }

    private void UpdateHintText()
    {
        if (hintText == null)
            return;

        hintText.text = customHintText;
    }
}
