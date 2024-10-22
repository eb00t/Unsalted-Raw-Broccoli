using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHandler : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthSlider; // if enemy is boss set this to HUD slider

    private void Start()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    public void TakeDamage(int damage)
    {
        if (health - damage > 0)
        {
            health -= damage;
            healthSlider.value = health;
        }
        else
        {
            healthSlider.value = 0;
            TriggerDeath();
        }
    }

    private void TriggerDeath()
    {
        gameObject.SetActive(false);
    }
}
