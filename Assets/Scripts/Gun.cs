using System.Collections;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Gun : NetworkIdentity
{
    #region REFERENCES AND SETTINGS

    public enum GunType { rifle, shotgun, rpg, pistol, sniper }

    [Header("UI References")]
    [SerializeField] private Sprite bulletIcon;
    [SerializeField] private RuntimeAnimatorController bulletAnim;

    [Header("References")]
    [SerializeField] private GameObject bulletPref;
    [SerializeField] private GameObject rocketPref;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform smokePoint;
    [SerializeField] private SpriteRenderer gunSprite;
    [SerializeField] private ParticleSystem smokeParticlePref;
    [SerializeField] private ParticleSystem muzzleFlashPref;
    [SerializeField] private Rigidbody2D playerRb;
    private Camera mainCam;
    private Light2D muzzleLight;

    [Header("Settings")]
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float flashDuration = 0.05f;
    public float bulletSpeed = 20f;
    public int maxAmmo = 5;
    public static bool IsPointerOverReloadButton;
    [SerializeField] private GunType type;

    [Header("Shotgun Settings")]
    [SerializeField] private int pellets = 8;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Burst Settings")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.1f;

    [Header("Sniper Settings")]
    public int enemiesCanPierce;

    [Header("Reload Settings")]
    [SerializeField] private float reloadDuration = 2f;

    private bool isReloading;

    private PlayerSystem input;
    private int currentAmmo;
    private PlayerMovement player;
    private bool isHoldingFire;
    private float lastFireTime;
    private Coroutine burstCoroutine;

    #endregion

    #region INITIALIZATION

    protected override void OnSpawned()
    {
        base.OnSpawned();

        currentAmmo = maxAmmo;

        if (isOwner)
        {
            GameManager.UpdateAmmoText(currentAmmo);
            GameManager.UpdateBulletUI(bulletIcon, bulletAnim);
        }
    }

    private void Awake()
    {
        currentAmmo = maxAmmo;

        muzzleLight = GetComponentInChildren<Light2D>();
        player = GetComponentInParent<PlayerMovement>();
        mainCam = Camera.main;
        input = new PlayerSystem();

        if (gunSprite == null)
            gunSprite = GetComponent<SpriteRenderer>();

        lastFireTime = -fireRate;
    }

    private void Start()
    {
        lastFireTime = Time.time;
        GameManager.UpdateAmmoText(currentAmmo);
        GameManager.UpdateBulletUI(bulletIcon, bulletAnim);
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Fire.performed += OnFirePerformed;
        input.Player.Fire.canceled += ctx => isHoldingFire = false;
    }

    private void OnDisable()
    {
        input.Player.Fire.performed -= OnFirePerformed;
        input.Player.Fire.canceled -= ctx => isHoldingFire = false;
        input.Player.Disable();
    }

    #endregion

    #region SHOOTING AND ROTATING

    private void Update()
    {
        if (type == GunType.rpg)
            player.canMove = false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        Vector2 direction = (worldMousePos - transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        ApplyRotation(angle);

        if (isHoldingFire && type != GunType.rifle && !isReloading && currentAmmo > 0)
        {
            Shoot();
            isHoldingFire = false;
        }
    }

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        isHoldingFire = true;

        if (type == GunType.rifle && Time.time - lastFireTime >= fireRate && !isReloading && currentAmmo > 0)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (IsPointerOverReloadButton || currentAmmo <= 0 || isReloading)
            return;

        switch (type)
        {
            case GunType.rifle:
                ShootBurst();
                break;
            case GunType.shotgun:
                ShootShotgun();
                break;
            case GunType.rpg:
                ShootRpg();
                break;
            case GunType.pistol:
                ShootPistol();
                break;
            case GunType.sniper:
                ShootSniper();
                break;
            default:
                Debug.LogError("Please select a gun type in the Inspector");
                break;
        }
    }

    private void ShootBurst()
    {
        if (burstCoroutine == null)
            burstCoroutine = StartCoroutine(BurstFire());
    }

    private void ShootRpg()
    {
        FireSingleShot(rocketPref);
    }

    private void ShootShotgun()
    {
        Vector2 shootDir = transform.right;
        Collider2D playerCol = GetComponentInParent<Collider2D>();
        float halfSpread = spreadAngle / 2;

        for (int i = 0; i < pellets; i++)
        {
            float angleOffset = Random.Range(-halfSpread, halfSpread);
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angleOffset);
            GameObject pellet = Instantiate(bulletPref, firePoint.position, rotation);
            Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = rotation * Vector2.down * bulletSpeed;
            pellet.GetComponent<Bullet>().InitializePlayer(playerCol, type, this);
        }

        ApplyRecoil(-shootDir.normalized);
        FinalizeShot(pellets);
    }

    private IEnumerator BurstFire()
    {
        int shotsFired = 0;
        Collider2D playerCol = GetComponentInParent<Collider2D>();

        while (shotsFired < burstCount && currentAmmo > 0)
        {
            Vector2 shootDir = transform.right;
            GameObject bullet = Instantiate(bulletPref, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDir * bulletSpeed;
            bullet.GetComponent<Bullet>().InitializePlayer(playerCol, type, this);

            ApplyRecoil(-shootDir.normalized);
            FinalizeShot(1);
            shotsFired++;

            if (currentAmmo <= 0 && !isReloading)
                StartCoroutine(Reload());

            yield return new WaitForSeconds(burstDelay);
        }

        burstCoroutine = null;
    }

    private void ShootPistol() => FireSingleShot(bulletPref);
    private void ShootSniper() => FireSingleShot(bulletPref);

    private void FireSingleShot(GameObject prefab)
    {
        Vector2 shootDir = transform.right;
        Collider2D playerCol = GetComponentInParent<Collider2D>();

        GameObject bullet = Instantiate(prefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = shootDir * bulletSpeed;

        if (bullet.TryGetComponent(out Bullet bulletComp))
            bulletComp.InitializePlayer(playerCol, type, this);
        else if (bullet.TryGetComponent(out Rocket rocketComp))
            rocketComp.Initialize(playerCol);

        ApplyRecoil(-shootDir.normalized);
        FinalizeShot(1);
    }

    private void FinalizeShot(int ammoUsed)
    {
        currentAmmo -= ammoUsed;
        GameManager.UpdateAmmoText(currentAmmo);

        if (currentAmmo <= 0 && !isReloading)
            StartCoroutine(Reload());

        PlaySmokeParticle();
        PlayMuzzleFlash();
        StartCoroutine(FlashMuzzleLight());
    }

    private void ApplyRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
        gunSprite.flipY = Mathf.Cos(angle * Mathf.Deg2Rad) < 0;
    }

    private void ApplyRecoil(Vector2 knockbackDir)
    {
        float scaledForce = knockbackForce * playerRb.mass;
        playerRb.AddForce(knockbackDir * scaledForce, ForceMode2D.Impulse);
        player.OnShootForce();
    }

    #endregion

    #region EFFECTS

    private void PlaySmokeParticle()
    {
        if (!smokeParticlePref) return;
        ParticleSystem ps = Instantiate(smokeParticlePref, smokePoint.position, smokePoint.rotation);
        var main = ps.main;
        ps.Play();
        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }

    private void PlayMuzzleFlash()
    {
        if (!muzzleFlashPref) return;
        ParticleSystem ps = Instantiate(muzzleFlashPref, firePoint.position, firePoint.rotation);
        var main = ps.main;
        ps.Play();
        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }

    private IEnumerator FlashMuzzleLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        muzzleLight.enabled = false;
    }

    #endregion

    #region RELOADING

    public IEnumerator Reload()
    {
        isReloading = true;
        GameManager.UpdateAmmoText(currentAmmo);
        GameManager.StartReloadAnimation();
        yield return new WaitForSeconds(reloadDuration);
        currentAmmo = maxAmmo;
        GameManager.UpdateAmmoText(currentAmmo);
        GameManager.StopReloadAnimation();
        isReloading = false;
    }

    public void StartReloadingWrapper()
    {
        if (currentAmmo == maxAmmo)
            return;
        StartCoroutine(Reload());
    }

    public void OnReloadButtonPointerEnter() => IsPointerOverReloadButton = true;
    public void OnReloadButtonPointerExit() => IsPointerOverReloadButton = false;

    #endregion
}
