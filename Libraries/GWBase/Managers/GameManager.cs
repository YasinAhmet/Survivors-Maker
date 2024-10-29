using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GWBase {

public class GameManager : Manager
{
    public static GameManager gameManager;

    public void ExecuteAfterDelay(IEnumerator routine, float delaySeconds)
    {
        StartCoroutine(ExecuteActionAfterDelay(routine, delaySeconds));
    }
    
    public void Execute(IEnumerator routine)
    {
        StartCoroutine(ExecuteAction(routine));
    }
    
    private IEnumerator ExecuteAction(IEnumerator routine)
    {
        yield return StartCoroutine(routine);
    }

    private IEnumerator ExecuteActionAfterDelay(IEnumerator routine, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        yield return StartCoroutine(routine);
    }

    public bool gameState
    {
        set
        {
            if (value)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0.0001f;
            }
        }
    }
    public float gameSpeed = 1;
    public SessionInformation sessionInformation = new SessionInformation();

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