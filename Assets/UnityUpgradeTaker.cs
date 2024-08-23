using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GWBase;


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
        headerField.text = upgradeDef.upgradeName;
    }

    public IEnumerator WaitForPickComplete()
    {
        while (!selected) {
            yield return new WaitForSeconds(0.25f);
        }
    }
}
