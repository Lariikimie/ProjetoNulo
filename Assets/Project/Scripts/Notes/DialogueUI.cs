using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Painel inferior de diálogo com efeito typewriter (TextMeshPro).
/// Trabalha com NoteData: usa suas cores/itálico de (Speech/Thought).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private GameObject panel;            // painel inferior (raiz)
    [SerializeField] private TextMeshProUGUI text;        // TMP principal
    [SerializeField] private TextMeshProUGUI hint;        // dica "[E] para continuar" (opcional)

    [Header("Typewriter")]
    [SerializeField] private float charsPerSecond = 50f;

    private Coroutine _typing;
    private bool _isTyping;

    // tags de estilo correntes (montadas com base no NoteData + LineKind)
    private string _openTag = "";
    private string _closeTag = "";

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (_typing != null) StopCoroutine(_typing);
        _typing = null;
        _isTyping = false;
        if (panel != null) panel.SetActive(false);
    }

    public bool IsVisible => panel != null && panel.activeSelf;

    public bool IsTyping()
    {
        return _isTyping;
    }

    public void SetHint(string message)
    {
        if (hint != null) hint.text = message;
    }

    /// <summary>
    /// Define o estilo (cor + itálico) da próxima linha baseado no tipo (Speech/Thought)
    /// e nos dados de estilo do NoteData.
    /// </summary>
    public void SetStyle(NoteData data, NoteData.LineKind kind)
    {
        var color = (kind == NoteData.LineKind.Speech) ? data.speechColor : data.thoughtColor;
        bool italic = (kind == NoteData.LineKind.Thought) && data.thoughtItalic;

        string open = italic ? "<i>" : "";
        string close = italic ? "</i>" : "";

        _openTag = open + $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        _closeTag = "</color>" + close;
    }

    /// <summary>
    /// Inicia o efeito de digitação para o conteúdo informado (com o estilo atual).
    /// </summary>
    public void TypeLine(string content)
    {
        if (_typing != null) StopCoroutine(_typing);
        string rich = _openTag + content + _closeTag;
        _typing = StartCoroutine(TypeCoroutine(rich));
    }

    /// <summary>
    /// Revela o texto gradualmente usando maxVisibleCharacters (rich text seguro).
    /// Usa unscaledDeltaTime para funcionar mesmo com Time.timeScale=0.
    /// </summary>
    private IEnumerator TypeCoroutine(string richContent)
    {
        _isTyping = true;

        text.maxVisibleCharacters = 0;
        text.text = richContent;

        // Aguarda 1 frame para TMP calcular textInfo
        yield return null;
        int total = text.textInfo.characterCount;

        float t = 0f;
        while (text.maxVisibleCharacters < total)
        {
            t += Time.unscaledDeltaTime * charsPerSecond;
            text.maxVisibleCharacters = Mathf.Clamp(Mathf.FloorToInt(t), 0, total);
            yield return null;
        }

        _isTyping = false;
        _typing = null;
    }

    /// <summary>
    /// Pula o efeito e revela todo o texto.
    /// </summary>
    public void SkipTyping()
    {
        if (!_isTyping) return;

        text.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;

        if (_typing != null) StopCoroutine(_typing);
        _typing = null;
    }
}