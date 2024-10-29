using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GWBase;
using static GWBase.PlayerController;
using System;
using System.Threading.Tasks;

public class HealthOrb : MonoBehaviour, IBootable
{
    [SerializeField] private Slider slider;
    public float targetValue = 0;
    public float currentValue = 0;
    public float easingSpeed = 1f;
    public bool alreadyLerping = false;
    float timeElapsed = 0;
    float start = 0;
    [SerializeField] private float maxHealth;

    public void UpdateBar(HealthInfo healthInfo)
    {
        maxHealth = healthInfo.maxHealth;
        start = (healthInfo.currentHealth+healthInfo.damageTaken) / maxHealth;
        timeElapsed = 0f;
        targetValue = (healthInfo.currentHealth) / maxHealth;
        if(!alreadyLerping)StartCoroutine(EaseToTarget(easingSpeed));
    }
    
    public void UpdateBar(float current, float target)
    {
        start = current / maxHealth;
        timeElapsed = 0f;
        targetValue = target / maxHealth;
        if(!alreadyLerping)StartCoroutine(EaseToTarget(easingSpeed));
    }

    public static float easeOutCirc(double x)
    {
        return (float)Math.Sqrt(1 - Math.Pow(x -1, 2));
    }

    public IEnumerator EaseToTarget(float duration)
    {
        alreadyLerping = true;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.fixedDeltaTime;
            float t = timeElapsed / duration;
            float easedValue = easeOutCirc(t);
            currentValue = Mathf.Lerp(start, targetValue, easedValue);
            slider.value = currentValue;

            yield return new WaitForFixedUpdate();
        }

        currentValue = targetValue;
        slider.value = currentValue;
        alreadyLerping = false;
    }

    public void HealthChangeConverter(string statName, float newValue, float oldValue)
    {
        
        
        if (statName == "MaxHealth")
        {
            maxHealth = newValue;
            alreadyLerping = false;
            UpdateBar(oldValue, newValue);
        }
        
        if (statName == "Health")
        {
            UpdateBar(oldValue, newValue);
        }
    }

    
    public void BootSync()
    {
        StartCoroutine(SyncHealthStatUpdates());
    }

    private IEnumerator SyncHealthStatUpdates()
    {
        PlayerController.playerController.onOwnedHealthChange += (UpdateBar);

        while (PlayerController.playerController.ownedCreature == null || !PlayerController.playerController.ownedCreature.isActive)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        PlayerController.playerController.ownedCreature.GetPossessed().onStatChange += HealthChangeConverter;
        maxHealth = playerController.ownedCreature.GetPossessed().GetStatValueByName("MaxHealth");
    }

    public IEnumerator Boot()
    {
        yield return this;
    }
}
