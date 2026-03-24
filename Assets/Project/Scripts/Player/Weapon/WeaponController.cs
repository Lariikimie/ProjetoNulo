using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    private enum WeaponMode
    {
        Pistol,
        Taser
    }

    [Header("Referências")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WeaponFX weaponFX;
    [SerializeField] private GameObject bulletImpactPrefab;
    [SerializeField] private PlayerInventory playerInventory; // Referência ao inventário

    [Header("Pistola (dano)")]
    [SerializeField] private float pistolDamage = 25f;
    [SerializeField] private float pistolFireRate = 5f;
    [SerializeField] private float pistolMaxDistance = 50f;

    [Header("Taser (atordoamento)")]
    [SerializeField] private float taserStunDuration = 3f;
    [SerializeField] private float taserFireRate = 1f;
    [SerializeField] private float taserMaxDistance = 20f;

    [Header("Inputs")]
    [Tooltip("Se true, também usa o botão configurado no Input Manager (Fire1). Se false, só mouse.")]
    [SerializeField] private bool usarControle = false;
    [SerializeField] private string fireButton = "Fire1";
    [SerializeField] private KeyCode switchToPistolKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode switchToTaserKey = KeyCode.Alpha2;

    [Header("HUD")]
    [SerializeField] private Image taserIconHUD; // arraste o objeto TaserIcon aqui no Inspector
    [SerializeField] private Image pistolIconHUD; // arraste o objeto PistolIcon aqui no Inspector

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip weaponSwitchClip;

    private float nextTimeToFire = 0f;
    private WeaponMode currentMode = WeaponMode.Taser; // Começa com o Taser
    private bool hasPistol = false; // Só mostra o ícone se pegar a arma

    // LayerMask para ignorar HintTrigger
    private int ignoreHintTriggerMask;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerInventory == null)
            playerInventory = GetComponentInParent<PlayerInventory>();

        ignoreHintTriggerMask = ~(1 << LayerMask.NameToLayer("HintTrigger"));

        UpdateWeaponHUD();
    }

    private void Update()
    {
        HandleWeaponSwitch();
        HandleShooting();
    }

    private void HandleWeaponSwitch()
    {
        bool switched = false;

        if (hasPistol && Input.GetKeyDown(switchToPistolKey))
        {
            if (currentMode != WeaponMode.Pistol)
            {
                currentMode = WeaponMode.Pistol;
                switched = true;
            }
        }
        else if (Input.GetKeyDown(switchToTaserKey))
        {
            if (currentMode != WeaponMode.Taser)
            {
                currentMode = WeaponMode.Taser;
                switched = true;
            }
        }

        if (switched)
        {
            PlayWeaponSwitchSound();
            UpdateWeaponHUD();
            Debug.Log("[WeaponController] Troca de arma: " + currentMode);
        }
    }

    private void UpdateWeaponHUD()
    {
        if (taserIconHUD != null)
            taserIconHUD.enabled = (currentMode == WeaponMode.Taser);

        if (pistolIconHUD != null)
            pistolIconHUD.enabled = (hasPistol && currentMode == WeaponMode.Pistol);
    }

    // Chame este método quando o player pegar a arma na cena
    public void OnPickupPistol()
    {
        hasPistol = true;
        UpdateWeaponHUD();
        Debug.Log("[WeaponController] Pistola coletada!");
    }

    private void PlayWeaponSwitchSound()
    {
        if (audioSource != null && weaponSwitchClip != null)
            audioSource.PlayOneShot(weaponSwitchClip);
    }

    private void HandleShooting()
    {
        if (usarControle ? Input.GetButton(fireButton) : Input.GetMouseButton(0))
        {
            if (Time.time >= nextTimeToFire)
            {
                if (currentMode == WeaponMode.Pistol && hasPistol)
                {
                    ShootPistol();
                    nextTimeToFire = Time.time + 1f / pistolFireRate;
                }
                else if (currentMode == WeaponMode.Taser)
                {
                    ShootTaser();
                    nextTimeToFire = Time.time + 1f / taserFireRate;
                }
            }
        }
    }

    private void ShootPistol()
    {
        Debug.Log("[WeaponController] ShootPistol() chamado!");
        if (playerInventory != null && playerInventory.GetAmmoCount() > 0)
        {
            playerInventory.UseAmmo(1);
            Debug.Log("[WeaponController] Pistola disparada! Munição restante: " + playerInventory.GetAmmoCount());
            if (weaponFX != null)
                weaponFX.PlayShotEffects();

            // Raycast para acertar inimigos/objetos
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, pistolMaxDistance))
            {
                Debug.Log("[WeaponController] Pistola acertou: " + hit.collider.name);

                // Instancia o efeito de impacto
                if (bulletImpactPrefab != null)
                    Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                // Aplica dano se for inimigo
                var enemy = hit.collider.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(pistolDamage);
                }
            }
        }
        else
        {
            Debug.Log("[WeaponController] Sem munição!");
        }
    }

    private void ShootTaser()
    {
        Debug.Log("[WeaponController] ShootTaser() chamado!");

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, taserMaxDistance))
        {
            Debug.Log("[WeaponController] Taser acertou: " + hit.collider.name);

            // Aplica stun se for inimigo
            var enemy = hit.collider.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.ApplyStun(taserStunDuration);
            }
        }
        // Adicione aqui efeito visual/sonoro do taser se desejar
    }
}