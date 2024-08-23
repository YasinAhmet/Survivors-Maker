using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace GWBase {

public class PrefabManager : Manager
{
    public static PrefabManager prefabManager;
    public Dictionary<string, GameObject> prefabDictionary = new();
    public List<PrefabDef> prefabDefs = new();
    public override IEnumerator Kickstart()
    {
        prefabManager = this;
        yield return StartCoroutine(LoadAllPrefabs());
    }

    public IEnumerator LoadAllPrefabs() {
        foreach (var def in prefabDefs)
        {
            prefabDictionary.Add(def.name, def.prefabObj);
        }
        yield return this;
    }

    public GameObject GetPrefabOf(string name) {
        return prefabDictionary.FirstOrDefault(x => x.Key == name).Value;
    }
}

[Serializable]
public class PrefabDef {
    public string name = "Prefab";
    public GameObject prefabObj;
} 
}