using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private enum WeaponMode
    {
        Pistol,
        Taser
    }

    [Header("Referências")]
    [SerializeField] private Camera playerCamera;           // Câmera do jogador (Main Camera)
    [SerializeField] private WeaponFX weaponFX;             // Script de efeitos da arma (som, muzzle etc.)
    [SerializeField] private GameObject bulletImpactPrefab; // Prefab do impacto (esfera/partícula no ponto do tiro)

    [Header("Pistola (dano)")]
    [SerializeField] private float pistolDamage = 25f;
    [SerializeField] private float pistolFireRate = 5f;     // tiros por segundo
    [SerializeField] private float pistolMaxDistance = 50f;
    [SerializeField] private int pistolMaxAmmo = 5;

    [Header("Taser (atordoamento)")]
    [SerializeField] private float taserStunDuration = 3f;
    [SerializeField] private float taserFireRate = 1f;      // mais lento que a pistola
    [SerializeField] private float taserMaxDistance = 20f;

    [Header("Inputs")]
    [Tooltip("Se true, também usa o botão configurado no Input Manager (Fire1). Se false, só mouse.")]
    [SerializeField] private bool usarControle = false;
    [SerializeField] private string fireButton = "Fire1";   // Nome do botão no Input Manager (Fire1)
    [SerializeField] private KeyCode switchToPistolKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode switchToTaserKey = KeyCode.Alpha2;

    private float nextTimeToFire = 0f;
    private WeaponMode currentMode = WeaponMode.Pistol;
    private int currentPistolAmmo;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        currentPistolAmmo = pistolMaxAmmo;
        Debug.Log("[WEAPON] Modo inicial: PISTOLA. Munição: " + currentPistolAmmo);
    }

    private void Update()
    {
        HandleWeaponSwitch();
        HandleShooting();
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(switchToPistolKey))
        {
            currentMode = WeaponMode.Pistol;
            Debug.Log("[WEAPON] Troca para PISTOLA. Munição atual: " + currentPistolAmmo);
        }

        if (Input.GetKeyDown(switchToTaserKey))
        {
            currentMode = WeaponMode.Taser;
            Debug.Log("[WEAPON] Troca para TASER.");
        }
    }

    private void HandleShooting()
    {
        bool wantsToShoot = false;

        // Mouse: botão esquerdo
        if (Input.GetMouseButton(0))
            wantsToShoot = true;

        // Controle: só se estiver habilitado e Fire1 configurado no Input Manager
        if (usarControle && Input.GetButton(fireButton))
            wantsToShoot = true;

        if (!wantsToShoot)
            return;

        float fireRate = (currentMode == WeaponMode.Pistol) ? pistolFireRate : taserFireRate;
        if (fireRate <= 0f)
            fireRate = 1f;

        if (Time.time < nextTimeToFire)
            return;

        nextTimeToFire = Time.time + 1f / fireRate;

        if (currentMode == WeaponMode.Pistol)
            ShootPistol();
        else
            ShootTaser();
    }

    // ==================== PISTOLA ====================
    private void ShootPistol()
    {
        if (currentPistolAmmo <= 0)
        {
            Debug.Log("[WEAPON] Sem munição na pistola! Use o TASER (2).");
            return;
        }

        currentPistolAmmo--;
        Debug.Log("[WEAPON] Tiro de pistola. Munição restante: " + currentPistolAmmo);

        if (playerCamera == null)
        {
            Debug.LogWarning("WeaponController: playerCamera não foi atribuída no Inspector!");
            return;
        }

        if (weaponFX != null)
            weaponFX.PlayShotEffects();

        // RETÍCULA COMO ORIGEM: centro da tela
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pistolMaxDistance))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            if (bulletImpactPrefab != null)
            {
                Instantiate(
                    bulletImpactPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
            }

            // 1) TargetDummy (alvo de teste) – ainda funciona
            TargetDummy dummy = hit.collider.GetComponent<TargetDummy>();
            if (dummy != null)
            {
                dummy.TakeDamage(pistolDamage);
                Debug.Log("[WEAPON] PISTOLA acertou TargetDummy: " + dummy.gameObject.name);
                return;
            }

            // 2) Inimigo "real" com EnemyStats
            EnemyStats enemy = hit.collider.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(pistolDamage);
                Debug.Log("[WEAPON] PISTOLA acertou inimigo: " + enemy.gameObject.name +
                          " | HP após dano: " + enemy.CurrentHealth);
                return;
            }

            // 3) Não é inimigo nem dummy, mas acertou algo
            Debug.Log("[WEAPON] Pistola acertou objeto: " + hit.collider.name);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * pistolMaxDistance, Color.yellow, 0.2f);
            Debug.Log("[WEAPON] Pistola não acertou nada.");
        }
    }

    // ==================== TASER ====================
    private void ShootTaser()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("WeaponController: playerCamera não foi atribuída no Inspector!");
            return;
        }

        if (weaponFX != null)
            weaponFX.PlayShotEffects();

        // RETÍCULA COMO ORIGEM: centro da tela
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, taserMaxDistance))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.cyan, 0.2f);

            if (bulletImpactPrefab != null)
            {
                Instantiate(
                    bulletImpactPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
            }

            // TargetDummy: só log, não atordoa alvo de teste
            TargetDummy dummy = hit.collider.GetComponent<TargetDummy>();
            if (dummy != null)
            {
                Debug.Log("[WEAPON] TASER acertou TargetDummy (sem dano): " + dummy.gameObject.name);
                return;
            }

            // Inimigo real com EnemyStats: ATORDOA
            EnemyStats enemy = hit.collider.GetComponentInParent<EnemyStats>();
            if (enemy != null)
            {
                enemy.ApplyStun(taserStunDuration);
                Debug.Log("[WEAPON] TASER acertou inimigo: " + enemy.gameObject.name +
                          " | Atordoado por " + taserStunDuration + "s.");
                return;
            }

            Debug.Log("[WEAPON] TASER acertou objeto: " + hit.collider.name);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * taserMaxDistance, Color.cyan, 0.2f);
            Debug.Log("[WEAPON] TASER não acertou nada.");
        }
    }
}