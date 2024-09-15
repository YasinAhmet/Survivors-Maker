using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GWBase {

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float maxHealth = 0;
    [SerializeField] private Vector3 spawnOffset;
    
    public void UpdateHealthBar(HealthInfo healthInfo) {
        if(healthInfo.changeMax) maxHealth = healthInfo.currentHealth;
        var newSliderValue = healthInfo.currentHealth/maxHealth;
        slider.value = newSliderValue;
    }
}

}