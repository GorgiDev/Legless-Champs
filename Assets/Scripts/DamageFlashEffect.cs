using System.Collections;
using UnityEngine;

public class DamageFlashEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material flashMat;
    [Header("Settings")]
    [SerializeField] private float flashDuration = 0.3f;

    private bool isFlashing = false;

    public void Flash()
    {
        if (!isFlashing)
            StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;

        float halfDuration = flashDuration / 2f;
        float timer = 0f;

        while(timer < halfDuration){
            timer += Time.deltaTime;
            flashMat.SetFloat("_flashAmount", Mathf.Lerp(0f, 1f, timer /  halfDuration));
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            flashMat.SetFloat("_FlashAmount", Mathf.Lerp(1f, 0f, timer / halfDuration));
            yield return null;
        }

        flashMat.SetFloat("_flashAmount", 0f);
        isFlashing = false;
    }
}
