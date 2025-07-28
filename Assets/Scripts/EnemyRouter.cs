using System.Collections.Generic;
using UnityEngine;

public class EnemyRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] antennaPoints;
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject deathEffectPref;

    [Header("Settings")]
    [SerializeField] private float boostRadius = 5f;
    [SerializeField] private int maxBoostedEnemies = 2;

    private Animator anim;
    private EnemyStatus status;

    private readonly int maxHealth = 1;
    private int currentHealth;
    private bool isProtected;

    private readonly List<BoostedEnemy> boostedEnemies = new();
    

    private void Start()
    {
        currentHealth = maxHealth;

        status = GetComponent<EnemyStatus>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        isProtected = status.isInvincible;

        // Remove destroyed enemies from the list
        boostedEnemies.RemoveAll(e => e.enemy == null);

        // Remove enemies that are out of boost radius
        for (int i = boostedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = boostedEnemies[i].enemy;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist > boostRadius)
            {
                RemoveBoost(boostedEnemies[i]);
                boostedEnemies.RemoveAt(i);
            }
        }

        // Add new enemies to boost if below max-count
        if (boostedEnemies.Count < maxBoostedEnemies)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, boostRadius);
            foreach (var hit in hits)
            {
                string otherTag = hit.gameObject.tag;
                if (Tags.Enemies.isEnemy(otherTag) &&
                    hit.gameObject != gameObject &&
                    boostedEnemies.Find(be => be.enemy == hit.gameObject) == null)
                {
                    AddBoost(hit.gameObject);
                    if (boostedEnemies.Count >= maxBoostedEnemies)
                        break;
                }
            }
        }

        // Assign closest boosted enemy per antenna
        List<BoostedEnemy> unassignedEnemies = new List<BoostedEnemy>(boostedEnemies);
        for (int i = 0; i < antennaPoints.Length; i++)
        {
            if (unassignedEnemies.Count == 0)
            {
                // Optional: disable or clear lines on antennas without enemies
                continue;
            }

            BoostedEnemy closestEnemy = null;
            float closestDist = float.MaxValue;
            int closestIndex = -1;

            for (int j = 0; j < unassignedEnemies.Count; j++)
            {
                float dist = Vector2.Distance(antennaPoints[i].position, unassignedEnemies[j].enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = unassignedEnemies[j];
                    closestIndex = j;
                }
            }

            if (closestEnemy != null)
            {
                LineRenderer line = closestEnemy.line;
                line.SetPosition(0, antennaPoints[i].position);
                line.SetPosition(1, closestEnemy.enemy.transform.position);

                unassignedEnemies.RemoveAt(closestIndex);
            }
        }

        if (anim != null)
            anim.SetBool("hasFoundEnemy", boostedEnemies.Count > 0);
    }

    private void AddBoost(GameObject enemy)
    {
        EnemyStatus enemyStatus = enemy.GetComponent<EnemyStatus>();
        if (enemyStatus != null)
        {
            enemyStatus.SetInvincible(true);
            enemyStatus.SetGlow(true);
        }

        GameObject lightning = Instantiate(lightningPrefab);
        LineRenderer lr = lightning.GetComponent<LineRenderer>();
        lr.positionCount = 2;

        boostedEnemies.Add(new BoostedEnemy
        {
            enemy = enemy,
            line = lr
        });
    }

    private void RemoveBoost(BoostedEnemy boosted)
    {
        if (boosted.enemy != null)
        {
            EnemyStatus enemyStatus = boosted.enemy.GetComponent<EnemyStatus>();
            if (enemyStatus != null)
            {
                enemyStatus.SetInvincible(false);
                enemyStatus.SetGlow(false);
            }
        }

        if (boosted.line != null)
            Destroy(boosted.line.gameObject);
    }

    private void OnDestroy()
    {
        foreach (var boosted in boostedEnemies)
        {
            if (boosted.enemy != null)
            {
                EnemyStatus enemyStatus = boosted.enemy.GetComponent<EnemyStatus>();
                if (enemyStatus != null)
                {
                    enemyStatus.SetInvincible(false);
                    enemyStatus.SetGlow(false);
                }
            }

            if (boosted.line != null)
                Destroy(boosted.line.gameObject);
        }

        boostedEnemies.Clear();
    }

    public void TakeDamage(int damage)
    {
        if (!isProtected)
            currentHealth -= damage;
        else
            return;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {        
        Instantiate(deathEffectPref, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}

[System.Serializable]
public class BoostedEnemy
{
    public GameObject enemy;
    public LineRenderer line;
}
