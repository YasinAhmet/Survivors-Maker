using System.Collections;
using System.Collections.Generic;
using GWBase;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstanceFinisher : MonoBehaviour
{
    public void QuitInstance() {
        Application.Quit();
    }
    
    public void RestartIt()
    {
        PlayerController.playerController = null;
        PoolManager.poolManager = null;
        GameManager.gameManager = null;
        SpawnManager.spawnManager = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadToScene(string scene)
    {
        PlayerController.playerController = null;
        PoolManager.poolManager = null;
        GameManager.gameManager = null;
        SpawnManager.spawnManager = null;
        SceneManager.LoadScene(scene);
    }
}
