using UnityEngine;

public class AmmoItem : MonoBehaviour, IPickup
{
    [SerializeField] private int ammoAmount = 10;
    [SerializeField] private AudioClip pickupSound;

    public void Collect()
    {
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv != null)
        {
            inv.AddAmmo(ammoAmount);
            if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Destroy(gameObject);
        }
    }
}