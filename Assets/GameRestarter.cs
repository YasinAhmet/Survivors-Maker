using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GWBase;


public class GameRestarter : MonoBehaviour
{
    public EscapeMenu attachedEscapeMenu;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerController.playerController.ownedCreature.Heal(25, true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            attachedEscapeMenu.SetState(!attachedEscapeMenu.gameObject.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PlayerController.playerController.ownedCreature.TryDamage(25, out bool didDie);
        }
    }
}
