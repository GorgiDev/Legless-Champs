using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Referencee")]
    [SerializeField] private Animator bulletAnim;

    [Header("UI References")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image bulletIcon;
    [SerializeField] private TMPro.TextMeshProUGUI ammoText;
    [SerializeField] private TMPro.TextMeshProUGUI healthPercent;

    [Header("CPU Healthbar Sprites")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite mediumSprite;
    [SerializeField] private Sprite lowSprite;
    [SerializeField] private Sprite reallyLowSprite;
    [SerializeField] private Sprite criticalSprite;

    [Header("Timer")] 
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    
    private float elapsedTime = 0f;
    private bool isTimerRunning = false;

    private static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        StartTimer();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public static void UpdatePlayerHealthUI(float currentHealth, float maxHealth)
    {
        if (instance == null) return;
        instance.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarImage == null) return;

        float percent = Mathf.Clamp01(currentHealth / maxHealth);

        if (percent >= 0.75f)
            healthBarImage.sprite = fullSprite;
        else if (percent >= 0.5f)
            healthBarImage.sprite = mediumSprite;
        else if (percent >= 0.25f)
            healthBarImage.sprite = lowSprite;
        else if (percent > 0f)
            healthBarImage.sprite = criticalSprite;

        if (healthPercent != null)
            healthPercent.text = $"{Mathf.RoundToInt(percent * 100)}%";
    }

    public static void UpdateAmmoText(int currentAmmo)
    {
        if (instance == null || instance.ammoText == null)
            return;

        instance.ammoText.text = currentAmmo.ToString();
    }

    public static void StartReloadAnimation()
    {
        if (instance == null || instance.bulletAnim == null) 
            return;

        instance.bulletAnim.SetBool("isReloading", true);
    }

    public static void StopReloadAnimation()
    {
        if (instance == null || instance.bulletAnim == null)
            return;

        instance.bulletAnim.SetBool("isReloading", false);
    }

    public static void UpdateBulletUI(Sprite icon, RuntimeAnimatorController anim)
    {
        if (instance == null)
            return;

        if (instance.bulletIcon != null)
            instance.bulletIcon.sprite = icon;

        if (instance.bulletAnim != null)
            instance.bulletAnim.runtimeAnimatorController = anim;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int miliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, miliseconds);
        }
    }

    public static void StartTimer()
    {
        if (instance == null)
            return;

        instance.isTimerRunning = true;
        instance.elapsedTime = 0f;
    }

    public static void StopTimer()
    {
        if (instance == null)
            return;
        
        instance.isTimerRunning = false;
    }
    
    public static void ResetTimer()
    {
        if (instance == null) 
            return;
        
        instance.elapsedTime = 0f;
        instance.UpdateTimerUI();
    }
}
