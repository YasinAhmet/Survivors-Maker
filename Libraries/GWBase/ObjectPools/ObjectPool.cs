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
        public Transform parentTransform;
        [SerializeField] public GameObj[] pooledObjects;
        [SerializeField] public List<GameObj> freeObjects = new List<GameObj>();
        [SerializeField] public GameObject attachedPrefab;
        [SerializeField] private int maxAmount = 50;
        [SerializeField] private int maxAmountIncrease = 50;


        public GameObj HardSet(GameObject newObj, ThingDef creatureDef, Vector2 location, int targetIndex, string faction)
        {
            UnityEngine.Object.Destroy(pooledObjects[targetIndex].gameObject);
            var TargetSlot = UnityEngine.Object.Instantiate(newObj).GetComponent<GameObj>();
            TargetSlot.transform.position = location;
            TargetSlot.Spawned();
            TargetSlot.Possess<GameObj>(creatureDef, faction);
            TargetSlot.CallActivationChange(true);
            pooledObjects[targetIndex] = TargetSlot;
            return TargetSlot;
        }

        public GameObj ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction)
        {
            var slot = FindOrCreateSlot();
            //Debug.Log($"Obtaining Object Pool slot for type {creature.Name}.. Location:  {location}", slot.gameObject);
            slot.transform.position = location;
            slot.transform.eulerAngles = new Vector3(0, 0, rotation);
            slot.Possess<GameObj>(creature, faction);
            slot.CallActivationChange(true);
            return slot;
        }

        public GameObj FindOrCreateSlot()
        {
            if (freeObjects.Count == 0)
            {
                IncreaseSizeOfPool(maxAmountIncrease);
            }
            
            var foundSlot = FindFreeSlot();

            return foundSlot;
        }


        public GameObj FindFreeSlot()
        {
            var found = freeObjects.ElementAt(0);
            freeObjects.RemoveAt(0);
            return found;
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
                if (!pooledObjects[i])
                {
                    var newGameObject = CreateSlot();
                    pooledObjects[i] = newGameObject;
                    newGameObject.activityChange += NewGameObjectOnactivityChange;
                    newGameObject.transform.SetParent(parentTransform);
                    freeObjects.Add(newGameObject);
                    newObjects.Add(newGameObject);
                }
            }

            newObjectsInitiated?.Invoke(newObjects);
        }

        private void NewGameObjectOnactivityChange(GameObj obj, bool active)
        {
            if(!active) freeObjects.Add(obj);
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