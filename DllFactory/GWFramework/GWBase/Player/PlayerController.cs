using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerController
{
    public XpGainedUpdate onXP = new();

    public static PlayerController playerController;
    public GameObj_Creature ownedCreature;
    public float XPRequirementMultiplier = 2;
    public LevelInfo currentLevel = new();

    public IEnumerator TryFetchCreature() {
        while (ownedCreature == null) {
            yield return new WaitForSeconds(0.25f);
        }

        ownedCreature.onXpGain.AddListener(GainXP);
        Debug.Log($"[PLAYER] XP Listener Fetched..");
    }

    public IEnumerator Start()
    {
        Debug.Log($"[PC] Player Controller initializing..");
        playerController = this;
        currentLevel = new() {
            currentXP = 0,
            targetXP = 300,
            level = 1
        };

        yield return this;
    }

    public void TryLevelUp() {
        if(currentLevel.currentXP >= currentLevel.targetXP) currentLevel.targetXP *= XPRequirementMultiplier;
    }

    public void GainXP(float amount) {
        currentLevel.currentXP += amount;
        TryLevelUp();

        onXP?.Invoke(currentLevel);
    }


[System.Serializable]
public class XpGainedUpdate : UnityEvent<LevelInfo>
{
}

public struct LevelInfo {
    public float currentXP;
    public float targetXP;
    public int level;
}
}
