using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickup
{
    [Header("Configuração")]
    [Tooltip("Som ao coletar a arma (opcional)")]
    [SerializeField] private AudioClip pickupSound;

    public void Collect()
    {
        Debug.Log("[WeaponPickup] Collect chamado!");

        // Ativa a pistola no HUD
        WeaponController weaponController = FindObjectOfType<WeaponController>();
        if (weaponController != null)
        {
            weaponController.OnPickupPistol();
            Debug.Log("[WeaponPickup] Pistola coletada e ativada no HUD!");
        }
        else
        {
            Debug.LogWarning("[WeaponPickup] WeaponController não encontrado!");
        }

        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }
}