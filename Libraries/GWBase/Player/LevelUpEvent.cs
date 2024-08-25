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
            GameObject levelUpScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("levelUpScreen");
            spawned = UIManager.uiManager.SpawnComponentAtUI(levelUpScreenPrefab);
            MonoBehaviour reference = spawned.GetComponent<MonoBehaviour>();
            upgradeTakers = spawned.transform.GetComponentsInChildren<IUpgradeTaker>().ToList();

            foreach (var upgradeTaker in upgradeTakers)
            {
                var upgradeToPossess = YKUtility.GetRandomElement<UpgradeDef>(AssetManager.assetLibrary.upgradeDefsDictionary);
                upgradeTaker.PossessUpgrade(upgradeToPossess);
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