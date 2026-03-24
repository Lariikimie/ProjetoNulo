using UnityEngine;

public class ItemHighlight : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;

    // Ativa ou desativa o objeto de highlight visual
    public void SetHighlight(bool state)
    {
        if (highlightObject != null)
            highlightObject.SetActive(state);
    }
}