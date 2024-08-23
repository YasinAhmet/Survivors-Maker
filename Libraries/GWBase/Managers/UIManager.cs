using System.Collections;
using UnityEngine;
namespace GWBase {

public class UIManager : Manager
{
    public static UIManager uiManager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private GameObject levelBar;

    public override IEnumerator Kickstart()
    {
        uiManager = this;
        SpawnCanvas();
        SpawnLevelBar();
        SpawnWorldCanvas();
        yield return this;
    }

    public GameObject SpawnComponentAtUI(GameObject component) {
        GameObject spawned = Instantiate(component, canvas.transform);
        return spawned;
    }

    public void SpawnCanvas() {
        var canvasPrefab = PrefabManager.prefabManager.GetPrefabOf("canvas");
        canvas = Instantiate(canvasPrefab).GetComponent<Canvas>();
    }

    public void SpawnLevelBar() {
        var barPrefab = PrefabManager.prefabManager.GetPrefabOf("levelBar");
        levelBar = Instantiate(barPrefab, canvas.transform);
        var bootable = levelBar.GetComponent<IBootable>();
        StartCoroutine(bootable.Boot());
    }

    public void SpawnWorldCanvas() {
        var canvas = PrefabManager.prefabManager.GetPrefabOf("worldCanvas");
        worldCanvas = Instantiate(canvas).GetComponent<Canvas>();
        worldCanvas.worldCamera = Camera.main;
    }

    public GameObject SpawnObjectAtWorldCanvas(GameObject objPrefab) {
        return Instantiate(objPrefab, worldCanvas.transform);
    }

    public void AttachObjectToWorldCanvas(GameObject gm) {
        gm.transform.SetParent(worldCanvas.transform);
    }
}
}