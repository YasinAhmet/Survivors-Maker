using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GWMisc
{
    public class GameOverScreenPlugin : IObjBehaviour
    {

        public Camera targettedCamera;
        private InteractionStash interactionStash;

        public string GetName(){return "GameOverScreen";}

        public void RareTick(object[] parameters, float deltaTime){return;}

        public async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            targettedCamera = Camera.main;
            while (!SettingsManager.settingsManager.loaded || !SettingsManager.settingsManager.playerController.ownedCreature)
            {
                await Task.Delay(500);
            }
            Debug.Log("[GAMEOVERSCREEN] loaded up the creature..");

            var targetcreature = SettingsManager.settingsManager.playerController.ownedCreature;
            while (!targetcreature.IsHealthDepleted()) {
                await Task.Delay(250);
            }
            Debug.Log("[GAMEOVERSCREEN] creature doesn't have health anymore..");

            
            GameManager.gameManager.SetGameState(false);
            var gameOverScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("gameOverScreen");
            var spawnedObj = UIManager.uiManager.SpawnCanvas(gameOverScreenPrefab);

            while(spawnedObj.gameObject.activeSelf) {
                await Task.Delay(500);
            }

            GameManager.gameManager.SetGameState(true);
            SceneManager.LoadScene("MainMenu");
        }

        public void Start(XElement possess, object[] parameters){return;}


        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}
    }
}