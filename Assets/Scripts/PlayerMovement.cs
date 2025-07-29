using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : NetworkIdentity
{
    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask ground;
    [SerializeField] private ParticleSystem landingParticlePref;
    [SerializeField] private ParticleSystem sparkEffectPref;
    [SerializeField] private DamageFlashEffect dmg;
    [SerializeField] private GameObject deathEffectPref;
    [SerializeField] private Light2D screenLight;
    [SerializeField] private Light2D redLight;
    [SerializeField] private Transform sparkPoint;

    [Header("Movement Settings")]
    public float walkSpeed;
    public float sprintSpeed;
    public bool isScrambled;
    public float scrambleDuration;
    public bool canMove = true;
    
    private int maxHealth = 100;
    private int currentHealth;
    
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerSystem input;
    private CameraShake camShake;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    private float fallStartY;
    
    private bool scrambleEffectPlayed = false;

    private enum State {idle, walking, jumping, falling, scrambled }
    private State state = State.idle;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Awake()
    {
        input = new PlayerSystem();
        currentHealth = maxHealth;
    }
    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        camShake = FindAnyObjectByType<CameraShake>();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Sprint.performed += ctx => isSprinting = true;
        input.Player.Sprint.canceled += ctx => isSprinting = false;

        GameManager.UpdatePlayerHealthUI(currentHealth, maxHealth);
    }

    private void FixedUpdate()
    {
        if (maxHealth > 0 || canMove)
        {
            if (canMove)
                HandleMovement();
            ChangeState();
        }
    }

    private void HandleMovement()
    {
        if (isScrambled)
        {
            screenLight.enabled = false;
            camShake.Shake(scrambleDuration, 0.4f, 200f);

            if (!scrambleEffectPlayed)
            {
                PlaySparks();
                StartCoroutine(FlashSparkLight());
                scrambleEffectPlayed = true;
            }
            
            return;
        }
        else
        {
            screenLight.enabled = true;
            scrambleEffectPlayed = false;
        }

        bool prevGroundState = isGrounded;
        Vector2 groundNorm = Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 
            groundCheckRadius + 0.1f, ground);

        if (hit)
        {
            isGrounded = true;
            groundNorm = hit.normal;
        }else
            isGrounded = false;


        if (prevGroundState && !isGrounded)
        {
            fallStartY = transform.position.y;
            state = State.jumping;
        }

        if (!prevGroundState && isGrounded)
        {
            float fallDis = fallStartY - transform.position.y;
            if (fallDis > 0.2f)
            {
                PlayLandingParticle(fallDis);

                float intensity = Mathf.Clamp(fallDis, 0.8f, 1f);
                camShake?.Shake(0.5f, intensity, 100);
            }
        }

        ChangeState();

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        if (isGrounded)
        {
            Vector2 slopeDir = new Vector2(groundNorm.y, - groundNorm.x);
            float targetSpeed = moveInput.x * speed;
            float currentSpeed = Vector2.Dot(rb.linearVelocity, slopeDir);
            float forceAmount = (targetSpeed - currentSpeed) * rb.mass;
            rb.AddForce(slopeDir * forceAmount, ForceMode2D.Force);
        }else
        {
            float targetVel = moveInput.x * speed;
            float forceX = targetVel - rb.linearVelocity.x;
            rb.AddForce(new Vector2(forceX * rb.mass, 0f), ForceMode2D.Force);
        }
    }

    private void ChangeState()
    {
        if (isScrambled)
        {
            state = State.scrambled;
        }
        else
        {
            if (state == State.jumping)
            {
                if (rb.linearVelocity.y < 0.1f)
                {
                    state = State.falling;
                    return;
                }
            }
            else if (state == State.falling)
            {
                if (isGrounded)
                {
                    state = State.idle;
                    return;
                }
            }

            if (isGrounded)
            {
                if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                {
                    state = State.walking;
                }
                else
                {
                    state = State.idle;
                }   
            }
        }
        anim.SetInteger("state", (int)state);
    }

    private void PlayLandingParticle(float fallDis)
    {
        if (landingParticlePref == null)
            return;

        ParticleSystem ps = Instantiate(landingParticlePref,
                                        groundCheck.position + new
                                        Vector3(Random.Range(-0.1f, 0.1f), -0.05f, 0f),
                                        Quaternion.identity);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }

    public void OnShootForce()
    {
        if (isGrounded)
        {
            fallStartY = transform.position.y;
            state = State.jumping;
            isGrounded = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        GameManager.UpdatePlayerHealthUI(currentHealth, maxHealth);

        dmg.Flash();
        camShake.Shake(0.8f, 0.3f, 100f);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Vector3 deathPos = transform.position;
        Quaternion deathRot = transform.rotation;
        
        GameManager.StopTimer();

        Destroy(gameObject);
        Instantiate(deathEffectPref, deathPos, deathRot);
        dmg.Flash();
        camShake.Shake(0.5f, 4f, 50f);
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    public void EnableRedLight()
    {
        redLight.enabled = true;
    }

    public void DisableRedLight()
    {
        redLight.enabled = false;
    }
    
    private void PlaySparks()
    {
        if (sparkEffectPref == null)
            return;

        ParticleSystem ps = Instantiate(sparkEffectPref, sparkPoint.position, sparkPoint.rotation, sparkPoint);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 5f); ;
    }

    private IEnumerator FlashSparkLight()
    {
        Light2D sparkLight = sparkEffectPref.GetComponentInChildren<Light2D>();
        
        sparkLight.enabled = true;
        yield return new WaitForSeconds(sparkEffectPref.main.startLifetime.constantMax + 5f);
        sparkLight.enabled = false;
    }
}
