using UnityEngine;

public class ShatteredPiece : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float fadeDuration = 1f;
    
    private float timer;
    private SpriteRenderer sr;
    private Color ogColor;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ogColor = sr.color;
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BreakableBoxPiece"), LayerMask.NameToLayer("Player"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BreakableBoxPiece"), LayerMask.NameToLayer("Enemy"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BreakableBoxPiece"), LayerMask.NameToLayer("BreakableBoxPiece"));
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ExplosiveBoxPiece"), LayerMask.NameToLayer("Player"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ExplosiveBoxPiece"), LayerMask.NameToLayer("Enemy"));
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ExplosiveBoxPiece"), LayerMask.NameToLayer("ExplosiveBoxPiece"));
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BreakableBoxPiece"), LayerMask.NameToLayer("ExplosiveBoxPiece"));
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > lifetime)
        {
            float t = (timer - lifetime) / fadeDuration;
            sr.color = new Color(ogColor.r, ogColor.g, ogColor.b, 1f -t);

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
