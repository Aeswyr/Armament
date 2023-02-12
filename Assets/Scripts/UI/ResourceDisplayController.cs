using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FixMath.NET;

public class ResourceDisplayController : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image damageBar;
    [SerializeField] private Image shieldBar;

    int maxHealth;
    int health;
    int damageValue;
    int shield;

    float damageTime;
    [SerializeField] private float damageDelay;
    
    // call to setup the health bar before use
    public void Setup(int maxHealth) {
        this.maxHealth = maxHealth;
        health = maxHealth;
        damageValue = maxHealth;
        shield = maxHealth;
        UpdateVisuals();
    }

    // used when taking unblocked damage
    public void Damage(int amount) {
        damageTime = Time.time;
        health -= amount;
        shield = health;
        UpdateVisuals();
    }

    // used when dealing chip damage
    public void ChipDamage(int amount) {
        if (health > 1) {
            health -= amount;
            if (health < 1)
                health = 1;
            damageValue = health;
        } else {
            shield -= amount;
        }
        UpdateVisuals();
    }

    // used whenever health must be restored
    public void Heal(int amount) {
        health += amount;
        if (damageValue < health)
            damageValue = health;
        if (shield < health)
            shield = health;
        UpdateVisuals();
    }

    void FixedUpdate() {
        if (damageValue > health && Time.time > damageDelay + damageTime) {
            damageValue--;
            UpdateDamage();
        }
    }

    private void UpdateVisuals() {
        float hVal = (float)health / maxHealth;
        healthBar.fillAmount = hVal;
        healthBar.color = Color.HSVToRGB((hVal * 100 + 25) / 360, 1, 1);
        float sVal = (float)shield / maxHealth;
        shieldBar.fillAmount = sVal;
        UpdateDamage();
    }

    private void UpdateDamage() {
        float val = (float)damageValue / maxHealth;
        damageBar.fillAmount = val;
    }
}
