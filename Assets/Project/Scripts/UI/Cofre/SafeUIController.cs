using UnityEngine;
using TMPro;

public class SafeUIController : MonoBehaviour
{
    public static bool IsSafeOpen { get; private set; } = false;

    [Header("Referências de UI")]
    [SerializeField] private GameObject safePanel;    // SafePanel
    [SerializeField] private TMP_Text codeText;       // Text_Code
    [SerializeField] private TMP_Text hintText;       // Text_Hint (opcional)

    [Header("Configuração")]
    [SerializeField] private int codeLength = 8;      // sempre 8 neste projeto
    [SerializeField] private char placeholderChar = '_';

    [Header("Inputs")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Q;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    private Safe currentSafe;
    private string currentInput = "";
    private float previousTimeScale = 1f;

    private void Start()
    {
        if (safePanel != null)
            safePanel.SetActive(false);

        UpdateCodeText();
        UpdateHintText();
    }

    private void Update()
    {
        if (!IsSafeOpen)
            return;

        HandleDigitInput();
        HandleSpecialKeys();
    }

    // ===== Abrir / Fechar =====

    public void OpenSafe(Safe safe)
    {
        if (safe == null)
            return;

        currentSafe = safe;
        currentInput = "";

        if (safePanel == null)
        {
            Debug.LogWarning("[SafeUI] safePanel não atribuído.");
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        safePanel.SetActive(true);
        IsSafeOpen = true;

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
        currentInput = "";

        Debug.Log("[SafeUI] UI do cofre FECHADA.");
    }

    // ===== Input =====

    private void HandleDigitInput()
    {
        // Teclas 0–9 do teclado principal
        for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
        {
            if (Input.GetKeyDown(key))
            {
                char digit = (char)('0' + (key - KeyCode.Alpha0));
                AddDigit(digit);
                return;
            }
        }

        // Teclas 0–9 do teclado numérico
        for (KeyCode key = KeyCode.Keypad0; key <= KeyCode.Keypad9; key++)
        {
            if (Input.GetKeyDown(key))
            {
                char digit = (char)('0' + (key - KeyCode.Keypad0));
                AddDigit(digit);
                return;
            }
        }
    }

    private void HandleSpecialKeys()
    {
        // Backspace apaga último dígito
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateCodeText();
            }
        }

        // Confirmar com Q ou Enter
        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmCode();
        }

        // Cancelar com ESC
        if (Input.GetKeyDown(cancelKey))
        {
            CloseSafe();
        }
    }

    private void AddDigit(char digit)
    {
        if (currentInput.Length >= codeLength)
            return;

        currentInput += digit;
        UpdateCodeText();
    }

    private void ConfirmCode()
    {
        if (currentSafe == null)
        {
            Debug.LogWarning("[SafeUI] Nenhum cofre associado.");
            CloseSafe();
            return;
        }

        if (currentInput.Length != codeLength)
        {
            Debug.Log("[SafeUI] Código incompleto. Precisa de " + codeLength + " dígitos.");
            return;
        }

        string correct = currentSafe.GetCorrectCode();

        if (currentInput == correct)
        {
            currentSafe.OnCorrectCodeEntered();
            CloseSafe();
        }
        else
        {
            currentSafe.OnWrongCodeEntered();
            currentInput = "";
            UpdateCodeText();
        }
    }

    // ===== UI =====

    private void UpdateCodeText()
    {
        if (codeText == null)
            return;

        // Ex: "12__5___" até 8 caracteres
        string display = currentInput;
        while (display.Length < codeLength)
        {
            display += placeholderChar;
        }

        codeText.text = display;
    }

    private void UpdateHintText()
    {
        if (hintText == null)
            return;

        hintText.text = $"Digite {codeLength} dígitos.\n" +
                        "0–9 = inserir | Backspace = apagar | Q/Enter = confirmar | ESC = sair";
    }
}