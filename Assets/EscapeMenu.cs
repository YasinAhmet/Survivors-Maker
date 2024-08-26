using System.Collections;
using System.Collections.Generic;
using GWBase;
using UnityEngine;

public class EscapeMenu : MonoBehaviour
{
    public void SetState(bool active) {
        GameManager.gameManager.SetGameState(!active);
        this.gameObject.SetActive(active);
    }
}
