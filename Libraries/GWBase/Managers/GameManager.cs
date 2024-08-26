using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GWBase {

public class GameManager : Manager
{
    public static GameManager gameManager;
    public bool gameState = false;
    public float gameSpeed = 1;
    public static SessionInformation sessionInformation = new SessionInformation();
    public async void SetGameState(bool active) {
        if (active) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            gameState = true;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0.0001f;
            gameState = false;
        }
    }

        public override IEnumerator Kickstart()
        {
            gameManager = this;
            yield return this;
        }

        public struct SessionInformation {
        public int killCount;
        public int totalXP;
        public int totalDamageGiven;
        public int totalDamageTaken;
        public int totalHitsGiven;
        public int totalHitsTaken;
        
    }
}

}