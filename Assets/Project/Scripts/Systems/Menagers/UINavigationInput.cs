using UnityEngine;

/// <summary>
/// Gerencia as entradas de navegação de UI (esquerda/direita/cima/baixo,
/// confirmar e cancelar), com tudo configurável via Inspector.
/// 
/// Painéis como InventoryUI, PauseMenuController, MainMenuController
/// chamam os métodos deste script em vez de ler Input direto.
/// </summary>
public class UINavigationInput : MonoBehaviour
{
    public static UINavigationInput Instance { get; private set; }

    [Header("Teclas de navegação (teclado)")]
    [SerializeField] private KeyCode leftKey1 = KeyCode.A;
    [SerializeField] private KeyCode leftKey2 = KeyCode.LeftArrow;

    [SerializeField] private KeyCode rightKey1 = KeyCode.D;
    [SerializeField] private KeyCode rightKey2 = KeyCode.RightArrow;

    [SerializeField] private KeyCode upKey1 = KeyCode.W;
    [SerializeField] private KeyCode upKey2 = KeyCode.UpArrow;

    [SerializeField] private KeyCode downKey1 = KeyCode.S;
    [SerializeField] private KeyCode downKey2 = KeyCode.DownArrow;

    [Header("Eixos para controle (Gamepad / D-Pad)")]
    [Tooltip("Nome do eixo horizontal de UI no Input Manager (ex.: UI_Horizontal).")]
    [SerializeField] private string horizontalUIAxis = "UI_Horizontal";

    [Tooltip("Nome do eixo vertical de UI no Input Manager (ex.: UI_Vertical).")]
    [SerializeField] private string verticalUIAxis = "UI_Vertical";

    [Tooltip("Valor mínimo do eixo para considerar como navegação (0.5 recomendado).")]
    [SerializeField] private float axisThreshold = 0.5f;

    [Header("Botões de ação")]
    [Tooltip("Tecla de confirmar no teclado (ex.: Q, Enter, Space).")]
    [SerializeField] private KeyCode confirmKeyKeyboard = KeyCode.Q;

    [Tooltip("Botão de confirmar no controle (mapeado no Input Manager, ex.: Submit).")]
    [SerializeField] private string confirmButtonGamepad = "Submit";

    [Tooltip("Tecla de cancelar/voltar no teclado (ex.: Escape, Backspace).")]
    [SerializeField] private KeyCode cancelKeyKeyboard = KeyCode.Escape;

    [Tooltip("Botão de cancelar/voltar no controle (ex.: Cancel, B).")]
    [SerializeField] private string cancelButtonGamepad = "Cancel";

    // Controle interno para evitar repetição quando o eixo fica segurado
    private bool horizontalAxisInUse = false;
    private bool verticalAxisInUse = false;

    // Flags para desativar uso de eixo caso o Input Manager não tenha o axis
    private bool horizontalAxisAvailable = true;
    private bool verticalAxisAvailable = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UINavigationInput] Já existe uma instância, destruindo duplicata.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Se quiser que continue em cenas de menu/jogo:
        // DontDestroyOnLoad(gameObject);
    }

    // ------------------ MÉTODOS PÚBLICOS PARA OS PAINÉIS ------------------

    public bool LeftPressedThisFrame()
    {
        // Teclado
        bool key =
            Input.GetKeyDown(leftKey1) ||
            Input.GetKeyDown(leftKey2);

        // Eixo (controle)
        bool axis = false;
        float value = GetHorizontalAxisSafe();

        if (horizontalAxisAvailable)
        {
            if (!horizontalAxisInUse && value < -axisThreshold)
            {
                axis = true;
                horizontalAxisInUse = true;
            }
            else if (Mathf.Abs(value) < 0.1f)
            {
                // Soltou o eixo
                horizontalAxisInUse = false;
            }
        }

        return key || axis;
    }

    public bool RightPressedThisFrame()
    {
        // Teclado
        bool key =
            Input.GetKeyDown(rightKey1) ||
            Input.GetKeyDown(rightKey2);

        // Eixo (controle)
        bool axis = false;
        float value = GetHorizontalAxisSafe();

        if (horizontalAxisAvailable)
        {
            if (!horizontalAxisInUse && value > axisThreshold)
            {
                axis = true;
                horizontalAxisInUse = true;
            }
            else if (Mathf.Abs(value) < 0.1f)
            {
                horizontalAxisInUse = false;
            }
        }

        return key || axis;
    }

    public bool UpPressedThisFrame()
    {
        // Teclado
        bool key =
            Input.GetKeyDown(upKey1) ||
            Input.GetKeyDown(upKey2);

        // Eixo (controle)
        bool axis = false;
        float value = GetVerticalAxisSafe();

        if (verticalAxisAvailable)
        {
            if (!verticalAxisInUse && value > axisThreshold)
            {
                axis = true;
                verticalAxisInUse = true;
            }
            else if (Mathf.Abs(value) < 0.1f)
            {
                verticalAxisInUse = false;
            }
        }

        return key || axis;
    }

    public bool DownPressedThisFrame()
    {
        // Teclado
        bool key =
            Input.GetKeyDown(downKey1) ||
            Input.GetKeyDown(downKey2);

        // Eixo (controle)
        bool axis = false;
        float value = GetVerticalAxisSafe();

        if (verticalAxisAvailable)
        {
            if (!verticalAxisInUse && value < -axisThreshold)
            {
                axis = true;
                verticalAxisInUse = true;
            }
            else if (Mathf.Abs(value) < 0.1f)
            {
                verticalAxisInUse = false;
            }
        }

        return key || axis;
    }

    public bool ConfirmPressedThisFrame()
    {
        bool key = Input.GetKeyDown(confirmKeyKeyboard);
        bool btn = !string.IsNullOrEmpty(confirmButtonGamepad) &&
                   Input.GetButtonDown(confirmButtonGamepad);

        return key || btn;
    }

    public bool CancelPressedThisFrame()
    {
        bool key = Input.GetKeyDown(cancelKeyKeyboard);
        bool btn = !string.IsNullOrEmpty(cancelButtonGamepad) &&
                   Input.GetButtonDown(cancelButtonGamepad);

        return key || btn;
    }

    // ------------------ LEITURA SEGURA DE EIXOS ------------------

    private float GetHorizontalAxisSafe()
    {
        if (!horizontalAxisAvailable || string.IsNullOrEmpty(horizontalUIAxis))
            return 0f;

        try
        {
            return Input.GetAxisRaw(horizontalUIAxis);
        }
        catch (System.ArgumentException e)
        {
            Debug.LogWarning($"[UINavigationInput] Axis '{horizontalUIAxis}' não está configurado no Input Manager. Desativando uso de eixo horizontal para UI.\n{e.Message}");
            horizontalAxisAvailable = false;
            return 0f;
        }
    }

    private float GetVerticalAxisSafe()
    {
        if (!verticalAxisAvailable || string.IsNullOrEmpty(verticalUIAxis))
            return 0f;

        try
        {
            return Input.GetAxisRaw(verticalUIAxis);
        }
        catch (System.ArgumentException e)
        {
            Debug.LogWarning($"[UINavigationInput] Axis '{verticalUIAxis}' não está configurado no Input Manager. Desativando uso de eixo vertical para UI.\n{e.Message}");
            verticalAxisAvailable = false;
            return 0f;
        }
    }
}