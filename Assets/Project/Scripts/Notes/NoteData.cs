using UnityEngine;

/// <summary>
/// Dados de uma nota/bilhete do jogo. Serve tanto para:
/// 1) Texto simples (campo "content") — exibir o conteúdo do bilhete;
/// 2) Diálogo em múltiplas linhas (campo "lines") — com tipo "Speech" ou "Thought"
///    para aplicar estilos diferentes no DialogueUI.
/// 3) (Opcional) Uma única linha "após a leitura" (pensamento final do personagem).
/// 4) (NOVO) Visual específico por nota: fundo 2D (sprite/cor) e um prefab 3D opcional.
/// </summary>
[CreateAssetMenu(menuName = "Game/Note Data", fileName = "NewNote", order = 0)]
public class NoteData : ScriptableObject
{
    [Header("Identificação (opcional)")]
    [Tooltip("Um ID único para a nota (útil para sistemas de save).")]
    public string noteId;

    [Header("Exibição (título e texto corrido)")]
    [Tooltip("Título da nota que aparece em listas/diário (ex: 'Bilhete da Diretoria').")]
    public string title;

    [Tooltip("Conteúdo completo da nota (modo texto corrido).")]
    [TextArea(5, 20)]
    public string content;

    // ========= VISUAL ESPECÍFICO DA NOTA (NOVO) =========

    [Header("Visual 2D da Nota")]
    [Tooltip("Sprite de fundo da nota (papel rasgado, recorte de jornal, foto etc.). Se estiver vazio, será usado o sprite padrão do NoteViewerUI.")]
    public Sprite backgroundSprite;

    [Tooltip("Cor aplicada sobre o fundo da nota. Se deixar em branco (0,0,0,0), o NoteViewerUI usará a cor padrão.")]
    public Color backgroundColor = Color.clear;

    [Header("Visual 3D (opcional)")]
    [Tooltip("Prefab 3D ou VFX específico desta nota (ex.: livro na mesa, crucifixo flutuando). Opcional.")]
    public GameObject backgroundPrefab3D;

    // ========= DIÁLOGO EM LINHAS =========

    public enum LineKind { Speech, Thought }

    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(2, 4)] public string text;
        public LineKind kind; // Speech = fala, Thought = pensamento
    }

    [Header("Diálogo em linhas (opcional)")]
    [Tooltip("Se quiser exibir o bilhete como falas/pensamentos em múltiplas linhas, use este array.")]
    public DialogueLine[] lines;

    [Header("Estilo de fala/pensamento (para DialogueUI)")]
    public Color speechColor = Color.white;
    public Color thoughtColor = new Color(1f, 1f, 1f, 0.9f);

    [Tooltip("Se true, linhas do tipo Thought são exibidas em itálico.")]
    public bool thoughtItalic = true;

    // ========= PÓS-LEITURA (PENSAMENTO FINAL) =========

    [Header("Após a leitura (opcional)")]
    [Tooltip("Se verdadeiro, após terminar as 'lines' será exibida esta linha final (pensamento/fala).")]
    public bool hasAfterReadingLine = false;

    [Tooltip("Linha única exibida após a leitura (pensamento ou fala final do personagem).")]
    public DialogueLine afterReadingLine;
}