using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GWBase {

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider = null;
    
    public void UpdateHealthBar(HealthInfo healthInfo) {
        var newSliderValue = healthInfo.currentHealth/healthInfo.maxHealth;
        slider.value = newSliderValue;
    }

    private void Start()
    {
        GameObj owned = transform.parent.GetComponent<GameObj>();
        UpdateHealthBar(new HealthInfo()
        {
            currentHealth = owned.GetPossessed().GetStatValueByName("Health"),
            maxHealth = owned.GetPossessed().GetStatValueByName("MaxHealth")
        });
        owned.onHealthChange += UpdateHealthBar;
    }
}

}