using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GWBase {

public class GameManager : MonoBehaviour
{
    public static SessionInformation sessionInformation = new SessionInformation();
    public Manager[] linkedManagers = {};
    public GameObject[] objectsToBoot = {};
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Kickstarting Game Manager");
        StartCoroutine(LoadManagers());
    }

    public IEnumerator LoadManagers() {
        foreach (var manager in linkedManagers)
        {
            Debug.Log($"Kickstarting {manager.name}");
            yield return StartCoroutine(manager.Kickstart());
        }

        foreach (var objectBootable in objectsToBoot)
        {
            yield return StartCoroutine(objectBootable.GetComponent<IBootable>().Boot());
        }
    }

    public struct SessionInformation {
        public int killCount;
        public int totalXP;
        public int totalDamageGiven;
        public int totalDamageTaken;
        public int totalHitsGiven;
        public int totalHitsTaken;
        
    }
}

}