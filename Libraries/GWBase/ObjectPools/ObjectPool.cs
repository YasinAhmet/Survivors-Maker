using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;


namespace GWBase
{

    [Serializable]
    public class ObjectPool
    {
        public delegate void newObjectsDelegate(List<GameObj> newObjects);

        public event newObjectsDelegate newObjectsInitiated;
        [SerializeField] public GameObj[] pooledObjects;
        [SerializeField] public GameObject attachedPrefab;
        [SerializeField] private int maxAmount = 50;
        [SerializeField] private int maxAmountIncrease = 50;


        public GameObj HardSet(GameObject newObj, ThingDef creatureDef, Vector2 location, int targetIndex, string faction)
        {
            UnityEngine.Object.Destroy(pooledObjects[targetIndex].gameObject);
            var TargetSlot = UnityEngine.Object.Instantiate(newObj).GetComponent<GameObj>();
            TargetSlot.Spawned();
            TargetSlot.Possess<GameObj>(creatureDef, faction);
            TargetSlot.gameObject.SetActive(true);
            TargetSlot.transform.position = location;
            pooledObjects[targetIndex] = TargetSlot;
            return TargetSlot;
        }

        public GameObj ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction)
        {
            var slot = FindOrCreateSlot();
            slot.Possess<GameObj>(creature, faction);
            slot.gameObject.SetActive(true);
            slot.transform.position = location;
            slot.transform.eulerAngles = new Vector3(0, 0, rotation);
            return slot;
        }

        public GameObj FindOrCreateSlot()
        {
            var foundSlot = FindFreeSlot();

            if (foundSlot == null)
            {
                IncreaseSizeOfPool(maxAmountIncrease);
                foundSlot = FindFreeSlot();
            }

            return foundSlot;
        }


        public GameObj FindFreeSlot()
        {
            return pooledObjects.FirstOrDefault(x => x != null && !x.gameObject.activeSelf);
        }

        public void IncreaseSizeOfPool(int by)
        {
            maxAmount += by;
            GameObj[] temporary = new GameObj[maxAmount];
            pooledObjects.CopyTo(temporary, 0);
            pooledObjects = temporary;

            FillList();
        }

        public void FillList()
        {
            List<GameObj> newObjects = new List<GameObj>();
            for (int i = 0; i < maxAmount; i++)
            {
                if (pooledObjects[i] == null)
                {
                    var newGameObject = CreateSlot();
                    pooledObjects[i] = newGameObject;
                    newObjects.Add(newGameObject);
                }
            }

            newObjectsInitiated?.Invoke(newObjects);
        }

        public GameObj CreateSlot()
        {
            var gmbj = UnityEngine.Object.Instantiate(attachedPrefab);
            gmbj.SetActive(false);
            var gmobject = gmbj.GetComponent<GameObj>();
            gmobject.Spawned();
            return gmobject;
        }
    }
}