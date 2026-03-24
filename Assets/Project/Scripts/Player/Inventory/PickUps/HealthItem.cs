using UnityEngine;

public class HealthItem : MonoBehaviour, IPickup
{
    [SerializeField] private AudioClip pickupSound;

    public void Collect()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
        {
            inv.AddHealthItem(1);
            if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Destroy(gameObject);
        }
    }
}