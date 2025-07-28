using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Rocket : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private ParticleSystem explosionParticlePref;
    [SerializeField] private Light2D explosionLight;

    [Header("Settings")]
    [SerializeField] private float lifetime = .2f;
    [SerializeField] private float blastRadius = 5f;
    [SerializeField] private int maxDamage = 50;

    private GameObject ignoreThis;
    private CameraShake camShake;

    private void Awake()
    {
        StartCoroutine(DestroyAfterTime());
    }

    private void Start()
    {
        camShake = FindAnyObjectByType<CameraShake>();
    }

    public void Initialize(Collider2D shooterCol)
    {
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null || shooterCol == null)
            return;

        GameObject shooterGO = shooterCol.gameObject;
        Collider2D[] shooterCols = shooterGO.GetComponents<Collider2D>();

        foreach (Collider2D col in shooterCols)
        {
            Physics2D.IgnoreCollision(myCol, col, true);
        }

        this.ignoreThis = shooterCol.gameObject;
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == ignoreThis)
                continue;

            float dis = Vector3.Distance(transform.position, hit.transform.position);
            float damagePercent = Mathf.Clamp01(1 - (dis / blastRadius));
            int damage = Mathf.RoundToInt(maxDamage * damagePercent);

            PlayerMovement player = hit.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                continue;
            }
            if (hit.CompareTag(Tags.Enemies.Turret))
            {
                EnemyTurret turret = hit.GetComponentInChildren<EnemyTurret>();
                if (turret != null)
                    turret.TakeDamage(damage);
            }
            else if (hit.CompareTag(Tags.Enemies.DroneScrambler))
            {
                EnemyScrambler scrambler = hit.GetComponent<EnemyScrambler>();
                if (scrambler != null)
                    scrambler.TakeDamage(damage);
            }
            else if (hit.gameObject.CompareTag(Tags.Other.BreakableBox))
            {
                BreakableBox breakableBox = hit.gameObject.GetComponent<BreakableBox>();
                if (breakableBox != null) 
                    breakableBox.Shatter();
            }
            else if (hit.gameObject.CompareTag(Tags.Other.ExplosiveBarrel))
            {
                ExplosiveBarrel explosiveBarrel = hit.gameObject.GetComponent<ExplosiveBarrel>();
                if (explosiveBarrel != null)
                    explosiveBarrel.Explode();
            }
        }
        StartCoroutine(PlayExplosionEffects());
        Destroy(gameObject);
    }
    private IEnumerator PlayExplosionEffects()
    {
        if (camShake != null)
            camShake.Shake(0.7f, 2f, 500f);

        if (explosionParticlePref != null)
        {
            ParticleSystem ps = Instantiate(explosionParticlePref, transform.position,
        Quaternion.identity);
            var main = ps.main;
            ps.Play();
            Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
        }

        if (explosionLight != null)
        {
            explosionLight.enabled = true;

            float elapsed = 0f;
            float startIntensity = explosionLight.intensity;

            while (elapsed <= lifetime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            explosionLight.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
