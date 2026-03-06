// Assets/_CopilotPack/Dialogue/NoteDataSO.cs
using UnityEngine;

namespace Project.Gameplay.Dialogue
{
    public enum DLineKind { Speech, Thought }

    [System.Serializable]
    public struct DLine
    {
        [TextArea(2, 4)] public string text;
        public DLineKind kind;
    }

    [CreateAssetMenu(fileName = "NoteDataSO", menuName = "Game/Dialogue/Note Data SO", order = 0)]
    public class NoteDataSO : ScriptableObject
    {
        [Header("Título opcional")]
        public string title;

        [Header("Linhas do bilhete")]
        public DLine[] lines;

        [Header("Estilo")]
        public Color speechColor = Color.white;
        public Color thoughtColor = new Color(1f, 1f, 1f, 0.9f);
        public bool thoughtItalic = true;
    }
}