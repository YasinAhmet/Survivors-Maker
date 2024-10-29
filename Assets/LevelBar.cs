using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GWBase;
using static GWBase.PlayerController;
using System;

public class LevelBar : MonoBehaviour, IBootable
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    public float targetValue = 0;
    public float currentValue = 0;
    public float easingSpeed = 1f;
    public bool alreadyLerping = false;
    float timeElapsed = 0;
    float start = 0;

    public void UpdateLevelBar(LevelInfo levelInfo)
    {
        //Debug.Log($"[LEVEL] Updating level bar...");
        targetValue = levelInfo.currentXP / levelInfo.targetXP;
        start = currentValue;
        timeElapsed = 0f;
        if(!alreadyLerping)StartCoroutine(EaseToTarget(easingSpeed));
        text.text = $"{levelInfo.currentXP}/{levelInfo.targetXP}";
    }

    public static float EaseOutQuart(double x)
    {
        return (float)(1 - Math.Pow(1 - x, 4));
    }

    public IEnumerator EaseToTarget(float duration)
    {
        alreadyLerping = true;
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
        alreadyLerping = false;
    }



    public void BootSync()
    {
    }

    public IEnumerator Boot()
    {
        while (PlayerController.playerController == null)
        {
            yield return new WaitForFixedUpdate();
        }
        PlayerController.playerController.onXP += (UpdateLevelBar);
        yield return this;
    }
}
