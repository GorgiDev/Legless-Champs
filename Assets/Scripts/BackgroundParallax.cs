using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    [SerializeField] private float parallaxFactor = 0.5f;

    private Vector3 lastCamPos;

    void Start()
    {
        camTransform = Camera.main.transform;
        lastCamPos = camTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = camTransform.position - lastCamPos;
        transform.position += delta * parallaxFactor;
        lastCamPos = camTransform.position;
    }
}
