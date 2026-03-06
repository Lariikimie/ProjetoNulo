using UnityEngine;

public class DebugCheckpointRespawn : MonoBehaviour
{
    [SerializeField] private KeyCode respawnKey = KeyCode.R;

    private void Update()
    {
        if (Input.GetKeyDown(respawnKey))
        {
            CheckPointManager manager = FindObjectOfType<CheckPointManager>();

            if (manager != null)
            {
                manager.ReturnToLastCheckpoint();
                Debug.Log("[DebugCheckpointRespawn] Respawn no último checkpoint.");
            }
            else
            {
                Debug.LogWarning("[DebugCheckpointRespawn] CheckPointManager não encontrado na cena!");
            }
        }
    }
}