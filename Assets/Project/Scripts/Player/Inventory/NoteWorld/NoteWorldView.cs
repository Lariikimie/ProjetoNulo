using UnityEngine;

public class NoteWorldView : MonoBehaviour
{
    [Header("Dados")]
    [Tooltip("Qual NoteData essa nota 3D representa.")]
    [SerializeField] private NoteData noteData;

    [Header("Ponto de câmera")]
    [Tooltip("Transform com a posição/rotação ideais para a câmera ver essa nota. Se deixar vazio, usa o próprio transform.")]
    [SerializeField] private Transform cameraPoint;

    public NoteData NoteData => noteData;
    public Transform CameraPoint => cameraPoint != null ? cameraPoint : transform;

    private void OnDrawGizmos()
    {
        if (CameraPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(CameraPoint.position, 0.1f);
        Gizmos.DrawLine(CameraPoint.position, CameraPoint.position + CameraPoint.forward * 0.5f);
    }
}