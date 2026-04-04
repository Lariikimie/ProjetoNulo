using UnityEngine;

public class TriggerCutscene : MonoBehaviour
{
    [Header("Nome da cutscene (ex: TiaNeide01, BossRoom, etc.)")]
    public string cutsceneName = "DefaultCutscene";

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.PlayCutsceneByName(cutsceneName);
            }
        }
    }
}