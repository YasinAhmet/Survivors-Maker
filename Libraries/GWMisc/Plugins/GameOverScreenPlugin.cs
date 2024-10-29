using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GWMisc
{
    public class GameOverScreenPlugin : IObjBehaviour
    {
        
        public string GetName(){return "GameOverScreen";}

        public void RareTick(object[] parameters, float deltaTime){return;}

        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            PlayerManager.newSessionStarted += AddGameOverListener;
        }

        public void AddGameOverListener(PlayerManager manager)
        {
            manager.playerController.onOwnedHealthChange += TryThrowGameOverScreen;
        }
        
        private SynchronizationContext _context;
        public void TryThrowGameOverScreen(HealthInfo healthInfo)
        {
            if(!healthInfo.gotKilled)return;
            GameManager.gameManager.gameState = false;
            var gameOverScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("gameOverScreen");
            spawnedObj = UIManager.uiManager.SpawnCanvas(gameOverScreenPrefab);
            _context = SynchronizationContext.Current;
            GameManager.gameManager.Execute(WaitForClick());
        }

        private GameObject spawnedObj;

        public IEnumerator WaitForClick()
        {
            GameManager.gameManager.gameState = false;
            while (true)
            {
                if(!spawnedObj.gameObject.activeSelf)break;
                
                yield return new WaitForSecondsRealtime(0.1f); 
            }
            
            GameManager.gameManager.gameState = true;
            PlayerController.playerController = null;
            PoolManager.poolManager = null;
            GameManager.gameManager = null;
            SpawnManager.spawnManager = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("MainMenu"); 
        }

        public void Start(XElement possess, object[] parameters){return;}


        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}
    }
}