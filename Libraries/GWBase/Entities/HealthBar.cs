using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GWBase {

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider = null;
    [SerializeField] private float maxHealth = 0;
    
    public void UpdateHealthBar(HealthInfo healthInfo) {
        if(healthInfo.changeMax) maxHealth = healthInfo.currentHealth;
        var newSliderValue = healthInfo.currentHealth/maxHealth;
        slider.value = newSliderValue;
    }

    private void Start()
    {
        GameObj owned = transform.parent.GetComponent<GameObj>();
        UpdateHealthBar(new HealthInfo()
        {
            currentHealth = owned.GetPossessed().GetStatValueByName("Health"),
            changeMax = true
        });
        owned.onHealthChange += UpdateHealthBar;
    }
}

}