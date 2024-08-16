using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SettingsManager : Manager
{
    public PlayerController playerController;
    public override IEnumerator Kickstart()
    {
        Debug.Log($"[SETTINGS] Settings manager initializing..");
        playerController = new();
        yield return StartCoroutine(playerController.Start());
        yield return this;
        
    }
}
