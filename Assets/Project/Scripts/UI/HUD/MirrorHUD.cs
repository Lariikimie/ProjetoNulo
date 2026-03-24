using UnityEngine;
using UnityEngine.UI;

public class MirrorHUD : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera mirrorCamera;
    [SerializeField] private RawImage mirrorDisplay;

    [Header("Configuração")]
    [Tooltip("Taxa de atualização do espelho (frames). 1 = todo frame, 2 = a cada 2 frames, etc.")]
    [SerializeField] private int updateEveryNFrames = 2;

    [Header("Efeito de distorção (opcional)")]
    [Tooltip("Se true, espelha horizontalmente a imagem (como espelho real).")]
    [SerializeField] private bool flipHorizontal = true;

    private void Start()
    {
        if (mirrorCamera == null)
        {
            Debug.LogError("[MirrorHUD] MirrorCamera não atribuída!");
            return;
        }

        // Desliga rendering automático — nós controlamos quando renderiza
        mirrorCamera.enabled = false;

        if (flipHorizontal && mirrorDisplay != null)
        {
            // Espelha o UV horizontalmente para parecer um espelho real
            mirrorDisplay.uvRect = new Rect(1, 0, -1, 1);
        }
    }

    private void LateUpdate()
    {
        if (mirrorCamera == null) return;

        // Renderiza a cada N frames para economizar performance
        if (Time.frameCount % updateEveryNFrames == 0)
        {
            mirrorCamera.Render();
        }
    }

    /// <summary>Ativa/desativa o espelho (ex: quando não tem espelho no inventário).</summary>
    public void SetMirrorActive(bool active)
    {
        if (mirrorDisplay != null)
            mirrorDisplay.gameObject.SetActive(active);
    }
}