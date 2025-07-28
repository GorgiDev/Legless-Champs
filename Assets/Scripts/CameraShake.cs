using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [SerializeField] private CinemachineCamera CMCam;
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultAmplitude = 2f;
    [SerializeField] private float defaultFrequency = 2f;

    private CinemachineBasicMultiChannelPerlin perlin;

    private void Awake()
    {
        if (CMCam != null)
            perlin = CMCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float duration = -1f, float amplitude = -1f, float frequency = -1f)
    {
        if (perlin == null)
            return;

        perlin.AmplitudeGain = (amplitude > 0) ? amplitude : defaultAmplitude;
        perlin.FrequencyGain = (frequency > 0) ? frequency : defaultFrequency;

        float shakeTime = (duration > 0) ? duration : defaultDuration;
        StartCoroutine(StopShake(shakeTime));
    }

    private IEnumerator StopShake(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (perlin != null)
        {
            perlin.AmplitudeGain = 0f;
            perlin.FrequencyGain = 0f;
        }
    }

    [ContextMenu("Preview Shake")]
    private void PreeewShake()
    {
        Shake(defaultDuration, defaultAmplitude, defaultFrequency);
    }
}
