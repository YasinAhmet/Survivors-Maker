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

    public static float easeOutCirc(double x)
    {
        return (float)Math.Sqrt(1 - Math.Pow(x -1, 2));
    }

    public IEnumerator EaseToTarget(float duration)
    {
        alreadyLerping = true;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            // Use easeOutQuart for smoothing
            float easedValue = easeOutCirc(t);
            currentValue = Mathf.Lerp(start, targetValue, easedValue);
            slider.value = currentValue;

            yield return null; // Wait for the next frame
        }

        // Ensure the final value is exactly the target
        currentValue = targetValue;
        slider.value = currentValue;
        alreadyLerping = false;
    }

    public void HealthChangeConverter(string statName, float newValue, float oldValue)
    {
        //Debug.Log("Health change converter run.. " + statName + " " + newValue + " " + oldValue);
        if (statName == "MaxHealth")
        {
            StopAllCoroutines();
            maxHealth = newValue;
            alreadyLerping = false;
        }
        
        if (statName == "Health")
        {
            start = (oldValue) / maxHealth;
            timeElapsed = 0f;
            targetValue = (newValue) / maxHealth;
            if(!alreadyLerping)StartCoroutine(EaseToTarget(easingSpeed));
        }
    }

    
    public void BootSync()
    {
        _ = Task.Run(SyncHealthStatUpdates);
    }

    private async Task SyncHealthStatUpdates()
    {
        //Debug.Log("Syncing health updates");

        PlayerController.playerController.onOwnedHealthChange += (UpdateBar);

        while (PlayerController.playerController.ownedCreature == null || !PlayerController.playerController.ownedCreature.isActive)
        {
            await Task.Delay(500);
        }

        //Debug.Log("Adding stat change callback");
        PlayerController.playerController.ownedCreature.GetPossessed().onStatChange += HealthChangeConverter;
    }

    public IEnumerator Boot()
    {
        yield return this;
    }
}
