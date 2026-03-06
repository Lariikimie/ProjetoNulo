// Assets/Project/Scripts/UI/Dialogue/DialogueUI.cs
using System.Collections;
using UnityEngine;
using TMPro;

namespace Project.Gameplay.Dialogue
{
    /// <summary>
    /// Painel inferior de diálogo com efeito typewriter (TextMeshPro).
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("Referências UI")]
        [SerializeField] private GameObject panel;            // painel raiz (inferior)
        [SerializeField] private TextMeshProUGUI text;        // TMP principal
        [SerializeField] private TextMeshProUGUI hint;        // dica "[E] para continuar" (opcional)

        [Header("Typewriter")]
        [SerializeField] private float charsPerSecond = 50f;

        private Coroutine _typing;
        private bool _isTyping;

        // tags de estilo calculadas a partir do NoteDataSO (cor + itálico)
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

        /// <summary>Define cor/itálico para a próxima linha conforme o tipo.</summary>
        public void SetStyle(NoteDataSO data, DLineKind kind)
        {
            var color = (kind == DLineKind.Speech) ? data.speechColor : data.thoughtColor;
            bool italic = (kind == DLineKind.Thought) && data.thoughtItalic;

            string open = italic ? "<i>" : "";
            string close = italic ? "</i>" : "";

            _openTag = open + $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
            _closeTag = "</color>" + close;
        }

        /// <summary>Inicia o efeito de digitação para o conteúdo informado.</summary>
        public void TypeLine(string content)
        {
            if (_typing != null) StopCoroutine(_typing);
            string rich = _openTag + content + _closeTag;
            _typing = StartCoroutine(TypeCoroutine(rich));
        }

        /// <summary>Revela texto com suporte a RichText via maxVisibleCharacters.</summary>
        private IEnumerator TypeCoroutine(string richContent)
        {
            _isTyping = true;

            text.maxVisibleCharacters = 0;
            text.text = richContent;

            // espera 1 frame para o TMP preencher textInfo
            yield return null;
            int total = text.textInfo.characterCount;

            float t = 0f;
            while (text.maxVisibleCharacters < total)
            {
                t += Time.unscaledDeltaTime * charsPerSecond; // independe de Time.timeScale
                text.maxVisibleCharacters = Mathf.Clamp(Mathf.FloorToInt(t), 0, total);
                yield return null;
            }

            _isTyping = false;
            _typing = null;
        }

        /// <summary>Pula o efeito e revela todo o texto.</summary>
        public void SkipTyping()
        {
            if (!_isTyping) return;

            text.maxVisibleCharacters = int.MaxValue;
            _isTyping = false;

            if (_typing != null) StopCoroutine(_typing);
            _typing = null;
        }
    }
}