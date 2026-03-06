using UnityEngine;

public enum GameState { Gameplay, Dialogue, Paused }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState current = GameState.Gameplay;
    public GameState Current => current;

    public static bool IsGameplay => Instance != null && Instance.current == GameState.Gameplay;
    public static bool IsDialogue => Instance != null && Instance.current == GameState.Dialogue;
    public static bool IsPaused => Instance != null && Instance.current == GameState.Paused;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary> Entra no estado de Diálogo (tempo normal). </summary>
    public void EnterDialogue()
    {
        if (current == GameState.Paused) return;
        current = GameState.Dialogue;
        Time.timeScale = 1f;
    }

    /// <summary> Sai do Diálogo e volta para Gameplay (se não estiver em Pause). </summary>
    public void ExitDialogue()
    {
        if (current == GameState.Paused) return;
        current = GameState.Gameplay;
        Time.timeScale = 1f;
    }

    /// <summary> Define Pause ON/OFF e ajusta TimeScale. </summary>
    public void SetPaused(bool paused)
    {
        current = paused ? GameState.Paused : GameState.Gameplay;
        Time.timeScale = paused ? 0f : 1f;
    }
}