using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Referências do Player")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FlashlightController flashlight;

    [Header("Barras")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image batteryFill;

    void Update()
    {
        if (playerStats != null)
            healthFill.fillAmount = playerStats.GetHealthPercent();

        if (playerMovement != null)
            staminaFill.fillAmount = playerMovement.GetStaminaPercent();

        if (flashlight != null)
            batteryFill.fillAmount = flashlight.GetBatteryPercent();
    }
}

