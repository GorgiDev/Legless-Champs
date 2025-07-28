using UnityEditor;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [SerializeField] private GameObject wholeBox;
    [SerializeField] private GameObject brokenPieces;

    [SerializeField] private ParticleSystem shatterParticlesPref;
    [SerializeField] private Transform shatterParticleSpawnPoint;

    public void Shatter()
    {
        wholeBox.SetActive(false);
        brokenPieces.SetActive(true);
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (shatterParticlesPref != null && shatterParticleSpawnPoint != null)
        {
            PlayShatterParticles();
        }
        
        foreach (Transform piece in brokenPieces.transform)
        {
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * 5f;
                rb.AddForce(randomForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-200f, 200f));
            }
        }
    }

    private void PlayShatterParticles()
    {
        if (shatterParticlesPref == null)
            return;

        ParticleSystem ps = Instantiate(shatterParticlesPref, shatterParticleSpawnPoint.position, 
            Quaternion.identity, shatterParticleSpawnPoint);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }
}
