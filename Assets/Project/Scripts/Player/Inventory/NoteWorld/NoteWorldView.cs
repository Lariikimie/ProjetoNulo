using UnityEngine;

public class NoteWorldView : MonoBehaviour
{
    [Header("Referência visual")]
    [Tooltip("Renderer do squad 3D que exibe a textura da nota.")]
    [SerializeField] private Renderer squadRenderer;

    // Troca a textura do squad para a da nota recebida
    public void SetNoteVisual(NoteData note)
    {
        if (note == null || squadRenderer == null) return;
        if (note.backgroundSprite != null)
        {
            squadRenderer.material.mainTexture = note.backgroundSprite.texture;
            Debug.Log("[NoteWorldView] Textura trocada para: " + note.backgroundSprite.name);
        }
        else
        {
            Debug.LogWarning("[NoteWorldView] NoteData sem backgroundSprite!");
        }
    }
}