using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    public enum DoorAccessMode
    {
        Unopenable = 0,
        Free = 1,
        RequiresKey = 2
    }

    [Header("References")]
    [Tooltip("Transform to rotate. If empty, uses this transform.")]
    [SerializeField] private Transform doorTransform;

    [Header("Door Access")]
    [SerializeField] private DoorAccessMode accessMode = DoorAccessMode.Free;
    [SerializeField] private string requiredKeyId = "ChaveDiretoria";
    [SerializeField] private bool consumeKeyOnUnlock = false;

    [Header("Door State")]
    [SerializeField] private bool startOpen = false;
    [SerializeField] private bool allowClose = true;

    [Header("Animation")]
    [Tooltip("Target local Y delta in degrees when opened.")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openCloseSpeed = 180f;

    [Header("Controller Support")]
    [SerializeField] private bool fallbackToSubmitButton = true;
    [SerializeField] private string interactButton = "Submit";

    private bool isOpen;
    private bool isUnlocked;
    private bool isAnimating;
    private bool playerInRange;

    private PlayerInventory playerInventory;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (doorTransform == null)
            doorTransform = transform;
    }

    private void Awake()
    {
        if (doorTransform == null)
            doorTransform = transform;

        closedRotation = doorTransform.localRotation;
        openedRotation = Quaternion.Euler(
            doorTransform.localEulerAngles.x,
            doorTransform.localEulerAngles.y + openAngle,
            doorTransform.localEulerAngles.z
        );
    }

    private void Start()
    {
        isOpen = startOpen;
        isUnlocked = accessMode != DoorAccessMode.RequiresKey;
        doorTransform.localRotation = isOpen ? openedRotation : closedRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        playerInventory = other.GetComponent<PlayerInventory>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        playerInventory = null;
    }

    private void Update()
    {
        if (isAnimating)
        {
            UpdateDoorRotation();
        }

        if (!playerInRange || isAnimating)
            return;

        if (!GetInteractionPressedThisFrame())
            return;

        Interact();
    }

    private bool GetInteractionPressedThisFrame()
    {
        if (UINavigationInput.Instance != null)
            return UINavigationInput.Instance.InteractPressedThisFrame();

        bool keyboard = Input.GetKeyDown(KeyCode.E);
        bool button = fallbackToSubmitButton && !string.IsNullOrEmpty(interactButton) && Input.GetButtonDown(interactButton);
        return keyboard || button;
    }

    private void Interact()
    {
        if (accessMode == DoorAccessMode.Unopenable)
        {
            Debug.Log("[DoorController] This door cannot be opened.");
            return;
        }

        if (isOpen)
        {
            if (!allowClose)
                return;

            CloseDoor();
            return;
        }

        if (!TryUnlockIfNeeded())
            return;

        OpenDoor();
    }

    private bool TryUnlockIfNeeded()
    {
        if (accessMode != DoorAccessMode.RequiresKey)
            return true;

        if (isUnlocked)
            return true;

        if (playerInventory == null)
        {
            Debug.LogWarning("[DoorController] PlayerInventory not found on player.");
            return false;
        }

        if (!playerInventory.HasKey(requiredKeyId))
        {
            Debug.Log("[DoorController] Door locked. Required key: " + requiredKeyId);
            return false;
        }

        isUnlocked = true;

        if (consumeKeyOnUnlock)
        {
            Debug.Log("[DoorController] Key consumption requested, but inventory has no remove method yet.");
        }

        return true;
    }

    private void OpenDoor()
    {
        isOpen = true;
        isAnimating = true;
        Debug.Log("[DoorController] Door opening.");
    }

    private void CloseDoor()
    {
        isOpen = false;
        isAnimating = true;
        Debug.Log("[DoorController] Door closing.");
    }

    private void UpdateDoorRotation()
    {
        Quaternion target = isOpen ? openedRotation : closedRotation;
        doorTransform.localRotation = Quaternion.RotateTowards(
            doorTransform.localRotation,
            target,
            openCloseSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(doorTransform.localRotation, target) <= 0.05f)
        {
            doorTransform.localRotation = target;
            isAnimating = false;
        }
    }
}
