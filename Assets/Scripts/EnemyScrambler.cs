using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class EnemyScrambler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D idleLight;
    [SerializeField] private Light2D scramblingLight;
    [SerializeField] private GameObject deathEffectPref;
    [SerializeField] private Light2D scramblerDetectionLight;

    [Header("Scramble Settings")]
    [SerializeField] private float scrambleRadius = 5f;
    [SerializeField] private float scrambleDurationAfterLeaving = 1f;
    [SerializeField] private float scramblePushForce = 10f;
    [SerializeField] private float scrambleVulnerabilyDuration = 7f;

    private CircleCollider2D triggerCol;

    private EnemyStatus status;
    private GameObject player;
    private PlayerMovement playerMovement;
    private Gun playerGun;
    private Animator anim;
    private SpriteRenderer sprite;

    private bool isScrambling = false;
    private bool isVulnerable = false;
    private Coroutine scrambleCooldownCoroutine;

    private int maxHealth = 1;
    private int currentHealth;
    private bool isProtected;

    private enum State {idle, scrambling};
    private State state = State.idle;

    private void Start()
    {
        status = GetComponent<EnemyStatus>();
        scramblerDetectionLight = GetComponent<Light2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        triggerCol = gameObject.AddComponent<CircleCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.radius = scrambleRadius;

        scramblerDetectionLight.color = new Color(0f / 255f, 255f / 255f, 0f / 255f);

        currentHealth = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerMovement = player.GetComponent<PlayerMovement>();
            playerGun = player.GetComponentInChildren<Gun>();

            if (playerMovement != null && playerGun != null)
            {
                if (scrambleCooldownCoroutine != null)
                    StopCoroutine(scrambleCooldownCoroutine);

                StartScrambling();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (scrambleCooldownCoroutine != null)
                StopCoroutine(scrambleCooldownCoroutine);

            scrambleCooldownCoroutine = StartCoroutine(ScrambleCooldown());
        }
    }

    private IEnumerator ScrambleCooldown()
    {
        yield return new WaitForSeconds(scrambleDurationAfterLeaving);
        StopScrambling();
    }

    private void StartScrambling()
    {
        isScrambling = true;
        isVulnerable = true;
        scramblerDetectionLight.color = new Color(255f / 255f, 0f / 255f, 0f / 255f);

        if (playerGun != null)
            playerGun.enabled = false;
        if (playerMovement != null)
            playerMovement.isScrambled = true;
        playerMovement.scrambleDuration = 3;

        PushPlayerAway();

    }

    private void StopScrambling()
    {
        isScrambling = false;
        StartCoroutine(ScramblerVulnerabilityCooldown());

        if (playerGun != null)
            playerGun.enabled = true;

        playerMovement.isScrambled = false;

        player = null;
        playerMovement = null;
        playerGun = null;
    }

    private IEnumerator ScramblerVulnerabilityCooldown()
    {
        yield return new WaitForSeconds(scrambleVulnerabilyDuration);
        isVulnerable = false;
        scramblerDetectionLight.color = new Color(0f / 255f, 255f / 255f, 0f / 255f);
    }

    private void Update()
    {
        isProtected = false;
        if (status.isInvincible)
            isProtected = true;
        
        ChangeState();
        anim.SetInteger("state", (int)state);

        if (!isScrambling || playerMovement == null)
            return;
        if (isScrambling && player != null)
            FlipTowardPlayer();
    }


    private void ChangeState()
    {
        if (state == State.idle)
            if (isScrambling || isVulnerable)
            {
                state = State.scrambling;
            } else
                return;

        else if (state == State.scrambling)
            if (!isScrambling && !isVulnerable)
            {
                state = State.idle;
            } else
                return;
    }

    private void FlipTowardPlayer() 
    {
        if (sprite == null || player == null)
            return;

        Vector3 dir2Player = player.transform.position - transform.position;

        if (dir2Player.x > 0)
            sprite.flipX = false;
        else
            sprite.flipX = true;
    }

    private void PushPlayerAway()
    {
        if (player == null)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        Vector2 pushDir = (player.transform.position - transform.position).normalized;

        pushDir += Vector2.up * 0.5f;
        pushDir.Normalize();

        rb.AddForce(pushDir * scramblePushForce, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage)
    {   if (isVulnerable && !isProtected)
            currentHealth -= damage;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Vector3 deathPos = transform.position;
        Quaternion deathRot = transform.rotation;

        Destroy(gameObject);
        Instantiate(deathEffectPref, deathPos, deathRot);
    }

    public void EnableIdleLight()
    {
        idleLight.enabled = true;
    }
    public void DisableIdleLight()
    {
        idleLight.enabled = false;
    }
    public void EnableScramblingLight()
    {
        scramblingLight.enabled = true;
    }
    public void DisableScramblingLight()
    {
        scramblingLight.enabled = false;
    }
}
