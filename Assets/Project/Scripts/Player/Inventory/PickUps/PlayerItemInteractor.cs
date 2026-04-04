using UnityEngine;

public class PlayerItemInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayerMask;
    public KeyCode interactKey = KeyCode.E;

    private ItemHighlight currentHighlight;

    void Update()
    {
        HandleItemHighlight();
        HandleItemPickup();
    }

    void HandleItemHighlight()
    {
        // Verifica se Camera.main existe antes de usar
        if (Camera.main == null)
        {
            if (currentHighlight != null)
            {
                currentHighlight.SetHighlight(false);
                currentHighlight = null;
            }
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayerMask))
        {
            ItemHighlight highlight = hit.collider.GetComponent<ItemHighlight>();
            if (highlight != null)
            {
                if (currentHighlight != highlight)
                {
                    if (currentHighlight != null)
                        currentHighlight.SetHighlight(false);

                    currentHighlight = highlight;
                    currentHighlight.SetHighlight(true);
                }
                Debug.Log("[Interactor] Highlight detectado em: " + hit.collider.name);
                return;
            }
        }

        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
            currentHighlight = null;
        }
    }

    void HandleItemPickup()
    {
        if (currentHighlight != null && Input.GetKeyDown(interactKey))
        {
            var pickup = currentHighlight.GetComponent<IPickup>();
            if (pickup != null)
            {
                pickup.Collect();
                currentHighlight.SetHighlight(false);
                currentHighlight = null;
            }
        }
    }
}