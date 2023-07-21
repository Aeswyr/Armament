using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FixMath.NET;
using TMPro;

public class ResourceDisplayController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image damageBar;
    [SerializeField] private Image shieldBar;

    [Header("Meter")]
    [SerializeField] private Image meterBar;
    [SerializeField] private Image stocksBar;
    
    [Header("Break")]
    [SerializeField] private Image burstBar;
    [SerializeField] private Color burstCharging;
    [SerializeField] private Color burstFull;

    [Header("Guard")]
    [SerializeField] private Image guardBar;
    [SerializeField] private Color guardCharging;
    [SerializeField] private Color guardReady;

    int maxHealth;
    int health;
    int damageValue;
    int maxMeter;
    int maxBurst;
    int maxGuard;
    float damageTime;
    [SerializeField] private float damageDelay;
    
    // call to setup the health bar before use
    public void Setup(int maxHealth, int maxMeter, int maxBurst, int maxGuard) {

        this.maxMeter = maxMeter;

        this.maxHealth = maxHealth;
        damageValue = maxHealth;

        this.maxBurst = maxBurst;

        this.maxGuard = maxGuard;

        UpdateDamage();
    }

    // used when taking unblocked damage
    public void SetHealth(int val) {
        damageTime = Time.time;
        health = val;
        healthBar.fillAmount = ScaleHealth(val, maxHealth);
        healthBar.color = Color.HSVToRGB((((float)val / maxHealth) * 115 + 10) / 360, 1, 1);

        
    }

    // used when dealing chip damage
    public void SetWhiteHealth(int val) {
        shieldBar.fillAmount = ScaleHealth(val, maxHealth);

        if (val > health) {
            damageValue = health;
            UpdateDamage();
        }
        
    }

    private float ScaleHealth(float val, float max) {
        var x = val / max;
        return (Mathf.Pow(x, 3f) + x) / 2f;
    }

    public void SetMeter(int val) {
        meterBar.fillAmount = (float)val / maxMeter;

        stocksBar.fillAmount = (float)(4 * val / maxMeter) / 4;
    }

    public void SetBurst(int val) {
        burstBar.fillAmount = (float)val / maxBurst;

        burstBar.color = burstCharging;
        if (val >= maxBurst)
            burstBar.color = burstFull;
    }

    public void SetGuard(int val, bool usable) {
        guardBar.fillAmount = (float)val / maxGuard;

        guardBar.color = guardCharging;
        if (usable)
            guardBar.color = guardReady;
    }

    void FixedUpdate() {
        if (damageValue > health && Time.time > damageDelay + damageTime) {
            damageValue -= 2;
            UpdateDamage();
        }
    }

    private void UpdateDamage() {
        damageBar.fillAmount = ScaleHealth(damageValue, maxHealth);
    }
}
