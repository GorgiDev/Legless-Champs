using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DeathEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private Light2D flashLight;

    [Header("Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float extraLifetimeAfterFlash = 0.3f;

    private void OnEnable()
    {
        StartCoroutine(PlayEffectRoutine());
    }

    private IEnumerator PlayEffectRoutine()
    {
        if (deathParticles != null)
            deathParticles.Play();

        if (flashLight != null)
        {
            flashLight.enabled = true;

            float elapsed = 0f;
            float startIntensity = flashLight.intensity;

            while (elapsed <= flashDuration)
            {
                elapsed += Time.deltaTime;
                flashLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / flashDuration);
                yield return null;
            }

            flashLight.enabled = false;
            flashLight.intensity = startIntensity;
        }

        float particleLifetime = deathParticles != null ? 
            deathParticles.main.startLifetime.constantMax : 0f;

        yield return new WaitForSeconds(particleLifetime + extraLifetimeAfterFlash);

        Destroy(gameObject);
    }
}
