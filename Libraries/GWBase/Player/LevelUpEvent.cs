using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace GWBase
{

    public class LevelUpEvent : IPopup
    {
        public bool eventOver;
        public IUpgradeTaker pickedUpgrade;
        GameObject spawned;
        public List<IUpgradeTaker> upgradeTakers;
        public Task StartPopup()
        {
            GameManager.gameManager.SetGameState(false);
            GameObject levelUpScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("levelUpScreen");
            spawned = UIManager.uiManager.SpawnComponentAtUI(levelUpScreenPrefab);
            upgradeTakers = spawned.transform.GetComponentsInChildren<IUpgradeTaker>().ToList();

            List<UpgradeDef> upgradesCanBeTaken = (from val in AssetManager.assetLibrary.upgradeDefsDictionary where val.Value.onLevelUpgrade == "Yes" select val.Value).ToList();
            
            foreach (var upgradeTaker in upgradeTakers)
            {
                var upgradeToPossess = YKUtility.GetRandomElement<UpgradeDef>(upgradesCanBeTaken);
                upgradeTaker.PossessUpgrade(upgradeToPossess);
                upgradesCanBeTaken.Remove(upgradeToPossess);
            }

            return Task.CompletedTask;
        }

        public async Task WaitForDone()
        {
            while (!eventOver)
            {
                foreach (var taker in upgradeTakers)
                {
                    if(taker.IsSelected()) {
                        GameManager.gameManager.SetGameState(true);
                        eventOver = true;
                        pickedUpgrade = taker;
                        PlayerController.playerController.ownedGroup.groupLeader.PossessUpgrades(new UpgradeDef[] { taker.GetPossessed() });
                        MonoBehaviour.Destroy(spawned);
                        break;
                    }
                }
                await Task.Delay(250);
            }
        }
    }
}