using System.Collections;
using System.Collections.Generic;
using GWBase;
using UnityEngine;

public class EscapeMenu : MonoBehaviour
{
    public void SetState(bool active) {
        GameManager.gameManager.gameState = !active;
        this.gameObject.SetActive(active);
    }
}
