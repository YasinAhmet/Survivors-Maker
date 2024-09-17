using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GWBase;
using static GWBase.PlayerController;
using System;

public class HealthOrb : MonoBehaviour, IBootable
{
    [SerializeField] private Slider slider;
    public float targetValue = 0;
    public float currentValue = 0;
    public float easingSpeed = 1f;
    public float maxValue = 1;

    public void UpdateBar(HealthInfo healthInfo)
    {
        if (maxValue <= 1 && PlayerController.playerController.ownedCreature != null)
        {
            maxValue = PlayerController.playerController.ownedCreature.GetPossessed().GetStatValueByName("Health");
        }

        Debug.Log($"[LEVEL] Updating level bar...");
        StopCoroutine("EaseToTarget");
        targetValue = (healthInfo.currentHealth+healthInfo.damageTaken) / maxValue;
        StartCoroutine(EaseToTarget(easingSpeed));
    }

    public static float EaseOutQuart(double x)
    {
        return (float)(1 - Math.Pow(1 - x, 4));
    }

    public IEnumerator EaseToTarget(float duration)
    {
        float start = currentValue;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            // Use easeOutQuart for smoothing
            float easedValue = EaseOutQuart(t);
            currentValue = Mathf.Lerp(start, targetValue, easedValue);
            slider.value = currentValue;

            yield return null; // Wait for the next frame
        }

        // Ensure the final value is exactly the target
        currentValue = targetValue;
        slider.value = currentValue;
    }



    public void BootSync()
    {
        PlayerController.playerController.onOwnedHealthChange.AddListener(UpdateBar);
    }

    public IEnumerator Boot()
    {
        yield return this;
    }
}
