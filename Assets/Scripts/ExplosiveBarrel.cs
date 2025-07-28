using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject wholeBarrel;
    [SerializeField] private GameObject brokenPieces;

    [SerializeField] private ParticleSystem explosionParticlePref;
    [SerializeField] private Light2D explosionLight;

    [SerializeField] private Transform explosionSpawnPoint;
    [SerializeField] private LayerMask explosionMask;

    [Header("Settings")]
    [SerializeField] private float lifetime = .2f;
    [SerializeField] private float blastRadius = 5f;
    [SerializeField] private int maxDamage = 50;

    private GameObject ignoreThis;
    private CameraShake camShake;

    private bool hasExploded = false;

    private void Start()
    {
        camShake = FindAnyObjectByType<CameraShake>();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius, explosionMask);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == ignoreThis)
                continue;

            float dis = Vector3.Distance(transform.position, hit.transform.position);
            float damagePercent = Mathf.Clamp01(1 - (dis / blastRadius));
            int damage = Mathf.RoundToInt(maxDamage * damagePercent);

            if (hit.TryGetComponent(out PlayerMovement player))
            {
                player.TakeDamage(damage);
                continue;
            }

            if (hit.CompareTag(Tags.Enemies.Turret))
            {
                if (hit.GetComponentInChildren<EnemyTurret>() is EnemyTurret turret)
                    turret.TakeDamage(damage);
            }
            else if (hit.CompareTag(Tags.Enemies.DroneScrambler))
            {
                if (hit.TryGetComponent(out EnemyScrambler scrambler))
                    scrambler.TakeDamage(damage);
            }
            else if (hit.CompareTag(Tags.Other.BreakableBox))
            {
                if (hit.TryGetComponent(out BreakableBox box))
                    box.Shatter();
            }
            else if (hit.CompareTag(Tags.Other.ExplosiveBarrel))
            {
                if (hit.TryGetComponent(out ExplosiveBarrel barrel))
                    barrel.Explode();
            }
        }

        StartCoroutine(PlayExplosionEffects());
        Shatter();
        Destroy(gameObject);
    }

    private void Shatter()
    {
        wholeBarrel.SetActive(false);
        brokenPieces.SetActive(true);

        if (TryGetComponent(out Collider2D col))
            col.enabled = false;

        foreach (Transform piece in brokenPieces.transform)
        {
            piece.SetParent(null);
            
            if (piece.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * 5f;
                rb.AddForce(randomForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-200f, 200f));
            }
        }
        
        Destroy(brokenPieces);
    }

    private IEnumerator PlayExplosionEffects()
    {
        if (camShake != null)
            camShake.Shake(0.7f, 2f, 500f);

        if (explosionParticlePref != null)
        {
            ParticleSystem ps = Instantiate(explosionParticlePref, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, 2f); // Safer lifetime
        }

        if (explosionLight != null)
        {
            explosionLight.enabled = true;
            float elapsed = 0f;

            while (elapsed <= lifetime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            explosionLight.enabled = false;
        }
    }

    public void SetIgnore(GameObject obj)
    {
        ignoreThis = obj;
    }
}
