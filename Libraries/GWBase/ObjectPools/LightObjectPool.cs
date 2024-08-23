using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;

namespace GWBase
{
    [Serializable]
    public class LightObjectPool
    {
        [SerializeField] public GameObject[] pooledObjects;
        [SerializeField] public GameObject attachedPrefab;
        [SerializeField] protected int maxAmount = 50;
        [SerializeField] protected int maxAmountIncrease = 50;

        public IEnumerator AutoDestroy(float seconds, GameObject gmObject)
        {
            yield return new WaitForSeconds(seconds);
            gmObject.SetActive(false);
        }

        public virtual GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction, float forSeconds)
        {
            var slot = FindOrCreateSlot();
            slot.gameObject.SetActive(true);
            slot.transform.position = location;
            slot.transform.eulerAngles = new Vector3(0, 0, rotation);

            PoolManager.poolManager.StartCoroutine(AutoDestroy(forSeconds, slot));
            return slot;
        }

        public virtual GameObject ObtainSlotForType(ThingDef creature, Vector2 location, float rotation, string faction)
        {
            var slot = FindOrCreateSlot();
            slot.gameObject.SetActive(true);
            slot.transform.position = location;
            slot.transform.eulerAngles = new Vector3(0, 0, rotation);
            return slot;
        }

        public GameObject FindOrCreateSlot()
        {
            var foundSlot = FindFreeSlot();

            if (foundSlot == null)
            {
                IncreaseSizeOfPool(maxAmountIncrease);
                foundSlot = FindFreeSlot();
            }

            return foundSlot;
        }


        public GameObject FindFreeSlot()
        {
            return pooledObjects.FirstOrDefault(x => x != null && !x.gameObject.activeSelf);
        }

        public void IncreaseSizeOfPool(int by)
        {
            maxAmount += by;
            GameObject[] temporary = new GameObject[maxAmount];
            pooledObjects.CopyTo(temporary, 0);
            pooledObjects = temporary;

            FillList();
        }

        public void FillList()
        {
            for (int i = 0; i < maxAmount; i++)
            {
                if (pooledObjects[i] == null)
                {
                    pooledObjects[i] = CreateSlot();
                }
            }
        }

        public GameObject CreateSlot()
        {
            var gmbj = UnityEngine.Object.Instantiate(attachedPrefab);
            gmbj.SetActive(false);
            return gmbj;
        }
    }
}
