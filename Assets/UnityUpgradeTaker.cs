using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GWBase;
using System.Threading.Tasks;


public class UnityUpgradeTaker : MonoBehaviour, IUpgradeTaker
{
    public TextMeshProUGUI descriptionField;
    public TextMeshProUGUI headerField;
    public UpgradeDef possessedUpgrade;
    public bool selected;

    public void Selected() {
        selected = true;
    }

    public UpgradeDef GetPossessed()
    {
        return possessedUpgrade;
    }

    public void PossessUpgrade(UpgradeDef upgradeDef) {
        possessedUpgrade = upgradeDef;
        descriptionField.text = upgradeDef.description;
        headerField.text = upgradeDef.displayName;
    }

    public async Task WaitForPickComplete()
    {
        while (!selected) {
            await Task.Delay(250);
        }
    }

    public bool IsSelected()
    {
        return selected;
    }
}
