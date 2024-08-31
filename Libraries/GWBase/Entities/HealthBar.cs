using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GWBase {

public class HealthBar : MonoBehaviour, IBootable
{
    [SerializeField] private Slider slider;
    [SerializeField] private float maxHealth = 0;
    [SerializeField] private Vector3 spawnOffset;
    private GameObject attachedObject;

    public void UpdateHealthBar(HealthInfo healthInfo) {
        if(healthInfo.changeMax) maxHealth = healthInfo.currentHealth;
        var newSliderValue = healthInfo.currentHealth/maxHealth;
        slider.value = newSliderValue;
    }

    public void FixedUpdate()
    {
        if(attachedObject) transform.position = attachedObject.transform.position + spawnOffset;
    }

    public void AttachObj(GameObj attachedObj) {
        attachedObj.onHealthChange.AddListener(UpdateHealthBar);
        attachedObject = attachedObj.gameObject;
        attachedObj.onActivationChange.AddListener(Toggle);
    }

    public void Toggle(bool toggle) {
        if(attachedObject) transform.position = attachedObject.transform.position + spawnOffset;
        gameObject.SetActive(toggle);
    }

    public IEnumerator Boot()
    {
        yield return this;
    }

    public void BootSync()
    {
        throw new System.NotImplementedException();
    }
}

}