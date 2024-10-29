using System.Collections;
using System.Collections.Generic;
using GWBase;
using TMPro;
using UnityEngine;

public class LoadingScreenUpdater : MonoBehaviour
{
    public TextMeshProUGUI textField;

    public void UpdateText(string newText)
    {
        textField.text = newText;
    }
}
