using System.Collections;
using UnityEngine;
namespace GWBase {

public class LevelUpEvent
{
    public void EventStart() {
        GameObject levelUpScreenPrefab = PrefabManager.prefabManager.GetPrefabOf("levelUpScreen");
        GameObject spawned = UIManager.uiManager.SpawnComponentAtUI(levelUpScreenPrefab);
        MonoBehaviour reference = spawned.GetComponent<MonoBehaviour>();

        foreach (var upgradeTaker in spawned.transform.GetComponentsInChildren<IUpgradeTaker>())
        {
            var upgradeToPossess = YKUtility.GetRandomElement<UpgradeDef>(AssetManager.assetLibrary.upgradeDefsDictionary);
            upgradeTaker.PossessUpgrade(upgradeToPossess);
            reference.StartCoroutine(WaitForOptionPick(reference, upgradeTaker));
        }
    }

    public IEnumerator WaitForOptionPick(MonoBehaviour reference, IUpgradeTaker upgradeTaker) {
        yield return reference.StartCoroutine(upgradeTaker.WaitForPickComplete());
        PlayerController.playerController.ownedGroup.groupLeader.PossessUpgrades(new UpgradeDef[]{upgradeTaker.GetPossessed()});
        GameObject.Destroy(reference.gameObject);
    }
}

public interface GameEvent {
    public void EventStart();
    public void EventEnd();
}
}