using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GWBase;
using static GWBase.PlayerController;

public class LevelBar : MonoBehaviour, IBootable
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    public void UpdateLevelBar(LevelInfo levelInfo) {
        Debug.Log($"[LEVEL] Updating level bar...");
        var newSliderValue = levelInfo.currentXP/levelInfo.targetXP;
        slider.value = newSliderValue;
        text.text = $"{levelInfo.currentXP}/{levelInfo.targetXP}";
    }

    public void BootSync() {
        
    }

    public IEnumerator Boot()
    {
        PlayerController.playerController.onXP.AddListener(UpdateLevelBar);
        yield return this;
    }
}
