using UnityEngine;

/// <summary>
/// Dados de uma nota/bilhete do jogo.
/// </summary>
[CreateAssetMenu(menuName = "Game/Note Data", fileName = "NewNote", order = 0)]
public class NoteData : ScriptableObject
{
    [Header("Identificação (opcional)")]
    public string noteId;

    [Header("Exibição (título e texto corrido)")]
    public string title;
    [TextArea(5, 20)]
    public string content;

    [Header("Visual 2D da Nota")]
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.clear;

    [Header("Visual 3D (opcional)")]
    public GameObject backgroundPrefab3D;

    public enum LineKind { Speech, Thought }

    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(2, 4)] public string text;
        public LineKind kind;
    }

    [Header("Diálogo em linhas (opcional)")]
    public DialogueLine[] lines;

    [Header("Estilo de fala/pensamento (para DialogueUI)")]
    public Color speechColor = Color.white;
    public Color thoughtColor = new Color(1f, 1f, 1f, 0.9f);
    public bool thoughtItalic = true;

    [Header("Após a leitura (opcional)")]
    public bool hasAfterReadingLine = false;
    public DialogueLine afterReadingLine;
}