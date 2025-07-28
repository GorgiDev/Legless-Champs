using System;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [SerializeField] private Material outlineMat;
    
    [NonSerialized] public bool isInvincible = false;

    private Material[] originalMats;
    private Material[] outlineMats;
    private SpriteRenderer[] spriteRenderers;
    
    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        int count = spriteRenderers.Length;

        originalMats = new Material[count];
        outlineMats = new Material[count];

        for (int i = 0; i < count; i++)
        {
            originalMats[i] = spriteRenderers[i].material;
            outlineMats[i] = new Material(outlineMat);  // create instance once
        }
    }
    
    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public void SetGlow(bool enable)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Material targetMat = enable ? outlineMats[i] : originalMats[i];
            if (spriteRenderers[i].material != targetMat)
            {
                spriteRenderers[i].material = targetMat;
            }
        }
    }
}
