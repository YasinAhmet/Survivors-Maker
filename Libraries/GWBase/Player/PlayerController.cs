using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace GWBase {

[Serializable]
public class PlayerController
{
    public delegate void XpGainedUpdate(LevelInfo levelInfo);
    public delegate void NewCharacter(GameObj_Creature creature);
    public delegate void LeveledUp(LevelInfo levelInfo);
    public event LeveledUp onPreLevelUp = delegate(LevelInfo level) {  }; 
    public event LeveledUp onLevelUp = delegate(LevelInfo level) {  }; 
    public event XpGainedUpdate onXP = delegate(LevelInfo info) {  };
    public event GameObj.HealthChangeEvent onOwnedHealthChange = delegate(HealthInfo info) {  };
    public event NewCharacter gotNewChar = delegate(GameObj_Creature creature) {  };

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

        ownedCreature.onXpGain += (GainXP);
        ownedCreature.onActionHappen += (ActionInfoProcessor);
        ownedCreature.onHealthChange += (onOwnedHealthChange.Invoke);
        gotNewChar?.Invoke(ownedCreature);
    }

    public void ActionInfoProcessor(string key, object value) {
        var sessionInformation = GameManager.gameManager.sessionInformation;
        switch (key) {
            case "hitGiven":
                HitResult hitResult = (HitResult)value;
                sessionInformation.totalDamageGiven += (int)hitResult.damage;
                sessionInformation.totalHitsGiven += 1;
                if(hitResult.killed) sessionInformation.killCount++;
                break;
            case "hitTaken":
                sessionInformation.totalHitsTaken += 1;
                sessionInformation.totalDamageTaken += (int)(float)value;
                break;
            default:
                break;
        }

        GameManager.gameManager.sessionInformation = sessionInformation;
    }

    public IEnumerator Start()
    {
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
            currentLevel.targetXP += (50*((float)(currentLevel.level/10f)));
            currentLevel.currentXP = 0;
            LevelUpEvent levelUpEvent = new LevelUpEvent();
            await levelUpEvent.StartPopup();
            onPreLevelUp.Invoke(currentLevel);
            await levelUpEvent.WaitForDone();
            currentLevel.level++;
            onLevelUp.Invoke(currentLevel);
        }
    }

    public void GainXP(float amount)
    {
        currentLevel.currentXP += amount;
        GameManager.gameManager.sessionInformation.totalXP += (int)amount;
        TryLevelUp();

        onXP?.Invoke(currentLevel);
    }
    

    [Serializable]
    public struct LevelInfo
    {
        public float currentXP;
        public float targetXP;
        public int level;
    }
}

}