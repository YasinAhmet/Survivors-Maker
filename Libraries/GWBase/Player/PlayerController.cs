using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace GWBase {

[Serializable]
public class PlayerController
{
    public XpGainedUpdate onXP = new();
    public HealthChangeEvent onOwnedHealthChange = new();

    public static PlayerController playerController;
    public GameObj_Creature ownedCreature;
    public CreatureGroup ownedGroup;
    public float XPRequirementMultiplier = 2;
    public LevelInfo currentLevel = new();

    public IEnumerator TryFetchCreature()
    {
        while (ownedCreature == null)
        {
            yield return new WaitForSeconds(0.25f);
        }

        ownedCreature.onXpGain.AddListener(GainXP);
        ownedCreature.onActionHappen.AddListener(ActionInfoProcessor);
        ownedCreature.onHealthChange.AddListener(onOwnedHealthChange.Invoke);
        Debug.Log($"[PLAYER] XP Listener Fetched..");
    }

    public void ActionInfoProcessor(string key, object value) {
        //Debug.Log($"[ACTION INFO PROCESSOR] Key: {key} Value: {value}");
        var sessionInformation = GameManager.sessionInformation;
        switch (key) {
            case "hitGiven":
                //Debug.Log($"[HITGIVEN INFO PROCESSOR] Key: {key} Value: {(HitResult)value}");
                HitResult hitResult = (HitResult)value;
                sessionInformation.totalDamageGiven += (int)hitResult.damage;
                sessionInformation.totalHitsGiven += 1;
                if(hitResult.killed) sessionInformation.killCount++;
                break;
            case "hitTaken":
                //Debug.Log($"[HITTAKEN INFO PROCESSOR] Key: {key} Value: {(int)(float)value}");
                sessionInformation.totalHitsTaken += 1;
                sessionInformation.totalDamageTaken += (int)(float)value;
                break;
            default:
                //Debug.Log($"[PC] Unknown action: {key}");
                break;
        }

        GameManager.sessionInformation = sessionInformation;
    }

    public IEnumerator Start()
    {
        Debug.Log($"[PC] Player Controller initializing..");
        playerController = this;
        currentLevel = new()
        {
            currentXP = 0,
            targetXP = 300,
            level = 1
        };

        GameObject orb = UIManager.uiManager.SpawnComponentAtUI(PrefabManager.prefabManager.GetPrefabOf("healthOrb"));
        orb.GetComponent<IBootable>().BootSync();

        yield return this;
    }

    public async void TryLevelUp()
    {
        if (currentLevel.currentXP >= currentLevel.targetXP)
        {
            currentLevel.targetXP *= XPRequirementMultiplier;
            LevelUpEvent levelUpEvent = new LevelUpEvent();
            await levelUpEvent.StartPopup();
            await levelUpEvent.WaitForDone();
        }
    }

    public void GainXP(float amount)
    {
        currentLevel.currentXP += amount;
        GameManager.sessionInformation.totalXP += (int)amount;
        TryLevelUp();

        onXP?.Invoke(currentLevel);
    }


    [System.Serializable]
    public class XpGainedUpdate : UnityEvent<LevelInfo>
    {
    }

    public struct LevelInfo
    {
        public float currentXP;
        public float targetXP;
        public int level;
    }
}

}