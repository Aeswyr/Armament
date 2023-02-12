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
    [SerializeField] private TextMeshProUGUI meterStocks;
    [SerializeField] private Image meterBar;

    [Header("Exhaust")]
    [SerializeField] private Image exhaustBar;
    [SerializeField] private Image rechargeBar;
    [SerializeField] private Color highExhaust;
    [SerializeField] private Color lowExhaust;

    int maxHealth;
    int health;
    int damageValue;
    int maxMeter;
    int maxExhaust;
    float damageTime;
    [SerializeField] private float damageDelay;
    
    // call to setup the health bar before use
    public void Setup(int maxHealth, int maxMeter, int maxExhaust) {

        this.maxMeter = maxMeter;
        
        this.maxExhaust = maxExhaust;

        this.maxHealth = maxHealth;
        damageValue = maxHealth;

        UpdateDamage();
    }

    // used when taking unblocked damage
    public void SetHealth(int val) {
        damageTime = Time.time;
        health = val;
        float hVal = (float)val / maxHealth;
        healthBar.fillAmount = hVal;
        healthBar.color = Color.HSVToRGB((hVal * 115 + 10) / 360, 1, 1);

        
    }

    // used when dealing chip damage
    public void SetWhiteHealth(int val) {
        shieldBar.fillAmount = (float)val / maxHealth;

        if (val > health) {
            damageValue = health;
            UpdateDamage();
        }
        
    }

    public void SetMeter(int val) {
        meterBar.fillAmount = (float)val / maxMeter;
    }

    public void SetMeterStocks(int amt) {
        meterStocks.text = amt.ToString();
    }

    public void  SetExhaust(int val, bool exhausted) {
        if (exhausted) {
            exhaustBar.gameObject.SetActive(false);
            rechargeBar.gameObject.SetActive(true);

            rechargeBar.fillAmount = (float)val / maxExhaust;
            return;
        }

        exhaustBar.gameObject.SetActive(true);
        rechargeBar.gameObject.SetActive(false);

        if (val > maxExhaust / 2)
            exhaustBar.color = highExhaust;
        else 
            exhaustBar.color = lowExhaust;

        exhaustBar.fillAmount = (float)val / maxExhaust;
    }

    void FixedUpdate() {
        if (damageValue > health && Time.time > damageDelay + damageTime) {
            damageValue--;
            UpdateDamage();
        }
    }

    private void UpdateDamage() {
        float val = (float)damageValue / maxHealth;
        damageBar.fillAmount = val;
    }
}
