using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyTurret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPref;
    [SerializeField] private Transform sparkPoint;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform smokePoint;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private Light2D muzzleLight;
    [SerializeField] private ParticleSystem muzzleFlashPref;
    [SerializeField] private ParticleSystem smokeParticlePref;
    [SerializeField] private GameObject deathEffectPref;
    [SerializeField] private ParticleSystem sparkEffectPref;

    [Header("Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float fireRate = 10f;
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    private Transform player;

    private float fireCooldown;
    private bool playerDetected;

    private int maxHealth = 1;
    private int currentHealth;

    private bool canShoot = true;
    private bool disabled;
    
    private float damageCooldown = 0.1f;
    private float lastDamageTime = -999f;

    private enum State {idle, shooting, disabled};
    private State state = State.idle;

    private void Start()
    {
        currentHealth = maxHealth;
            
        GameObject playerObj = GameObject.FindGameObjectWithTag(Tags.Friendlies.Player);
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        if (canShoot)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            playerDetected = (distance <= detectionRadius && CanSeePlayer());

            if (playerDetected)
            {
                RotateTowardsPlayer();
                FlipSprite();
                ShootAtPlayer();
            }
        }
        else
        {
            rotatingPart.rotation = Quaternion.Euler(0f, 0f, 11.532f);
            sprite.flipY = false;
        }
        
        ChangeState();
    }

    private void RotateTowardsPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rotatingPart.rotation = Quaternion.Euler(0f, 0f, angle + 180 /*rotating away fix*/);
    }

    private void FlipSprite()
    {
        if (player.position.x < transform.position.x)
            sprite.flipY = false;
        else
            sprite.flipY = true;
    }

    private void ShootAtPlayer()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            GameObject bullet = Instantiate(bulletPref, firePoint.position, firePoint.rotation);
            PlaySmokeParticle();
            PlayMuzzleFlash();
            StartCoroutine(FlashMuzzleLight());

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = -firePoint.right * bulletSpeed;
            fireCooldown = 1f / fireRate;

            GameObject grandParentGo = transform.parent?.parent?.gameObject;
            Collider2D col = grandParentGo?.GetComponent<Collider2D>();
            bullet.GetComponent<Bullet>().InitializeEnemy(col);
        }
    }

    private bool CanSeePlayer()
    {
        Vector2 origin = firePoint.position;
        Vector2 target = player.position;

        RaycastHit2D hit = Physics2D.Linecast(origin, target, obstructionMask);
        if (hit.collider != null && !hit.collider.CompareTag(Tags.Friendlies.Player))
            return false;
        return true;
    }

    private void ChangeState()
    {
        if (!canShoot && state != State.disabled)
        {
            state = State.disabled;
            anim.SetInteger("state", (int)state);
            fireCooldown = 0f;
        }
        else if (canShoot)
        {
            if (playerDetected && state != State.shooting)
            {
                state = State.shooting;
                anim.SetInteger("state", (int)state);
            }
            else if (!playerDetected && state != State.idle)
            {
                state = State.idle;
                anim.SetInteger("state", (int)state);
                fireCooldown = 0f;
            }
        }
    }

    private void PlaySmokeParticle()
    {
        if (smokeParticlePref == null)
            return;

        ParticleSystem ps = Instantiate(smokeParticlePref, smokePoint.position, smokePoint.rotation);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlashPref == null)
            return;

        ParticleSystem ps = Instantiate(muzzleFlashPref, firePoint.position, firePoint.rotation);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }

    private IEnumerator FlashMuzzleLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleLight.enabled = false;
    }
    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < damageCooldown) 
            return;
        
        GameObject grandParentGo = transform.parent?.parent?.gameObject;
        EnemyStatus status = grandParentGo.GetComponent<EnemyStatus>();
        
        if (!status.isInvincible)
            currentHealth -= damage;
        
        lastDamageTime = Time.time;
        
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        GameObject grandParentGo = transform.parent?.parent?.gameObject;
        Vector3 deathPos = grandParentGo.transform.position;
        Quaternion deathRot = grandParentGo.transform.rotation;

        Destroy(grandParentGo);
        Instantiate(deathEffectPref, deathPos, deathRot);
    }

    public void GetDisabled()
    {
        if (Time.time - lastDamageTime < damageCooldown) 
            return;
        
        if (disabled) //Die if enemy hit 2 times
        {
            Die();
            StopCoroutine(FlashSparkLight());
            return;
        }
        
        GameObject grandParentGo = transform.parent?.parent?.gameObject;
        EnemyStatus status = grandParentGo.GetComponent<EnemyStatus>();

        if (!status.isInvincible)
        {
            canShoot = false;

            PlaySparks();
            StartCoroutine(FlashSparkLight());
            StartCoroutine(WaitForSparks());
            
            disabled = true;
        }
        lastDamageTime = Time.time;
    }

    private void PlaySparks()
    {
        if (sparkEffectPref == null)
            return;

        ParticleSystem ps = Instantiate(sparkEffectPref, sparkPoint.position, sparkPoint.rotation);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 5f);
    }

    private IEnumerator FlashSparkLight()
    {
        Light2D sparkLight = sparkEffectPref.GetComponentInChildren<Light2D>();
        
        sparkLight.enabled = true;
        yield return new WaitForSeconds(sparkEffectPref.main.startLifetime.constantMax + 5f);
        sparkLight.enabled = false;
    }

    private IEnumerator WaitForSparks()
    {
        yield return new WaitForSeconds(sparkEffectPref.main.startLifetime.constantMax + 5f);
        canShoot = true;
        disabled = false;
    }
}
