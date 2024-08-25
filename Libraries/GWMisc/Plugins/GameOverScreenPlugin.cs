using System;
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
        public GameObj_Creature attachedObj;

        public string GetName()
        {
            return "GameOverScreen";
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public async void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            targettedCamera = Camera.main;
            await SetupGameOverScreen();
        }

        public void Start(XElement possess, object[] parameters)
        {
            return;
        }

        public async Task SetupGameOverScreen()
        {
            while (!SettingsManager.settingsManager.loaded || !SettingsManager.settingsManager.playerController.ownedCreature)
            {
                await Task.Delay(500);
            }

            attachedObj = SettingsManager.settingsManager.playerController.ownedCreature;
            attachedObj.onHealthChange.AddListener(CheckGameOver);
        }

        public async void CheckGameOver(HealthInfo healthInfo) {
            if(healthInfo.currentHealth > 0) return;
            var gameOverScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("gameOverScreen");
            Time.timeScale = 0.001f;
            var spawnedObj = UIManager.uiManager.SpawnComponentAtUI(gameOverScreenPrefab);
            await spawnedObj.GetComponent<IPopup>().WaitForDone();
            await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }

        public void Suspend(object[] parameters)
        {
            return;
        }

        public void Tick(object[] parameters, float deltaTime)
        {
            return;
        }

        ParameterRequest[] IObjBehaviour.GetParameters()
        {
            return null;
        }
    }
}