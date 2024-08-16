using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PoolManager : Manager
{
    public static PoolManager poolManager;
    [SerializeField] public ObjectPool creaturesPool;
    [SerializeField] public ObjectPool projectilesPool;
    [SerializeField] public LightObjectPool effectsPool;
    [SerializeField] public WorldUIObjectPool floatingTextPool;
    

    
    public override IEnumerator Kickstart()
    {
        poolManager = this;


        creaturesPool = new ObjectPool {
            pooledObjects = new GameObj[50],
            attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("enemy")
        };
        creaturesPool.FillList();

        projectilesPool = new ObjectPool {
            pooledObjects = new GameObj[50],
            attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("projectile")
        };
        projectilesPool.FillList();


        effectsPool = new LightObjectPool {
            pooledObjects = new GameObject[50],
            attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("hitEffect")
        };
        effectsPool.FillList();

        floatingTextPool = new WorldUIObjectPool {
            pooledObjects = new GameObject[50],
            attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("floatingText")
        };
        floatingTextPool.FillList();

        yield return this;
    }
}

[Serializable]
public class ObjectPool {
    [SerializeField] public GameObj[] pooledObjects;
    [SerializeField] public GameObject attachedPrefab;
    [SerializeField] private int maxAmount = 50;
    [SerializeField] private int maxAmountIncrease = 50;


    public GameObj HardSet(GameObject newObj, ThingDef creatureDef, Vector2 location, int targetIndex, string faction) {
        UnityEngine.Object.Destroy(pooledObjects[targetIndex].gameObject);
        var TargetSlot = UnityEngine.Object.Instantiate(newObj).GetComponent<GameObj>();
        TargetSlot.Spawned();
        TargetSlot.Possess<GameObj>(creatureDef, faction);
        TargetSlot.gameObject.SetActive(true);
        TargetSlot.transform.position = location;
        pooledObjects[targetIndex] = TargetSlot;
        return TargetSlot;
    }

    public GameObj ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction) {
        var slot = FindOrCreateSlot();
        slot.Possess<GameObj>(creature, faction);
        slot.gameObject.SetActive(true);
        slot.transform.position = location;
        slot.transform.eulerAngles = new Vector3(0,0,rotation);
        return slot;
    }

    public GameObj FindOrCreateSlot() {
        var foundSlot = FindFreeSlot();

        if(foundSlot == null) {
            IncreaseSizeOfPool(maxAmountIncrease);
            foundSlot = FindFreeSlot();
        }

        return foundSlot;
    }


    public GameObj FindFreeSlot() {
        return pooledObjects.FirstOrDefault(x => x != null && !x.gameObject.activeSelf);
    }

    public void IncreaseSizeOfPool(int by) {
        maxAmount += by;
        GameObj[] temporary = new GameObj[maxAmount];
        pooledObjects.CopyTo(temporary, 0);
        pooledObjects = temporary;

        FillList();
    }

    public void FillList() {
        for (int i = 0; i < maxAmount; i++)
        {
            if(pooledObjects[i] == null) {
                pooledObjects[i] = CreateSlot();
            }
        }
    }

    public GameObj CreateSlot() {
        var gmbj = UnityEngine.Object.Instantiate(attachedPrefab);
        gmbj.SetActive(false);
        var gmobject = gmbj.GetComponent<GameObj>(); 
        gmobject.Spawned();
        return gmobject;
    }
}

[Serializable]
public class WorldUIObjectPool: LightObjectPool {
    public override GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction)
    {
        var gameobj = base.ObtainSlotForType(creature, location, rotation, faction);
        UIManager.uiManager.AttachObjectToWorldCanvas(gameobj);
        return gameobj;
    }
}

[Serializable]
public class LightObjectPool {
    [SerializeField] public GameObject[] pooledObjects;
    [SerializeField] public GameObject attachedPrefab;
    [SerializeField] protected int maxAmount = 50;
    [SerializeField] protected int maxAmountIncrease = 50;

    public IEnumerator AutoDestroy(float seconds, GameObject gmObject) {
        yield return new WaitForSeconds(seconds);
        gmObject.SetActive(false);
    }

    public virtual GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction, float forSeconds) {
        var slot = FindOrCreateSlot();
        slot.gameObject.SetActive(true);
        slot.transform.position = location;
        slot.transform.eulerAngles = new Vector3(0,0,rotation);

        PoolManager.poolManager.StartCoroutine(AutoDestroy(forSeconds, slot));
        return slot;
    }

    public virtual GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction) {
        var slot = FindOrCreateSlot();
        slot.gameObject.SetActive(true);
        slot.transform.position = location;
        slot.transform.eulerAngles = new Vector3(0,0,rotation);
        return slot;
    }

    public GameObject FindOrCreateSlot() {
        var foundSlot = FindFreeSlot();

        if(foundSlot == null) {
            IncreaseSizeOfPool(maxAmountIncrease);
            foundSlot = FindFreeSlot();
        }

        return foundSlot;
    }


    public GameObject FindFreeSlot() {
        return pooledObjects.FirstOrDefault(x => x != null && !x.gameObject.activeSelf);
    }

    public void IncreaseSizeOfPool(int by) {
        maxAmount += by;
        GameObject[] temporary = new GameObject[maxAmount];
        pooledObjects.CopyTo(temporary, 0);
        pooledObjects = temporary;

        FillList();
    }

    public void FillList() {
        for (int i = 0; i < maxAmount; i++)
        {
            if(pooledObjects[i] == null) {
                pooledObjects[i] = CreateSlot();
            }
        }
    }

    public GameObject CreateSlot() {
        var gmbj = UnityEngine.Object.Instantiate(attachedPrefab);
        gmbj.SetActive(false);
        return gmbj; 
    }

    public interface ITextMeshProContact {
        public void SetText(string text);
    }
}