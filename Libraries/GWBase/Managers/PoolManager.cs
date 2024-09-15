using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GWBase
{

    public class PoolManager : Manager
    {
        [Serializable]
        public class OnPoolManagerEvent : UnityEvent<PoolManager> { }
        public static PoolManager poolManager;
        public static OnPoolManagerEvent OnPoolManagerInitiated;
        public int defaultPoolSize = 50;
        [SerializeField] protected float rareUpdateTickTime = 1.5f;
        public Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();
        public Dictionary<string, LightObjectPool> lightObjectPools = new Dictionary<string, LightObjectPool>();
        public Dictionary<string, WorldUIObjectPool> uiObjectPools = new Dictionary<string, WorldUIObjectPool>();
        public float rareUpdateTimeCounter = 0;
        public List<ObjectPool> cachedObjectPools = new List<ObjectPool>();

        public ObjectPool GetObjectPool(string poolName)
        {
            return objectPools.FirstOrDefault(x => x.Key == poolName).Value;
        }
        
        public LightObjectPool GetLightObjectPool(string poolName)
        {
            return lightObjectPools.FirstOrDefault(x => x.Key == poolName).Value;
        }
        
        public WorldUIObjectPool GetUIObjectPool(string poolName)
        {
            return uiObjectPools.FirstOrDefault(x => x.Key == poolName).Value;
        }
        
        public void TickPooledObjects(GameObj[] gameObjs)
        {
            foreach (var gameObj in gameObjs)
            {
                if (!gameObj.gameObject.activeSelf) continue;

                foreach (var behaviour in gameObj.installedBehaviours)
                {
                    behaviour?.RareTick(null, Time.fixedDeltaTime);
                }

                gameObj.RareTick(rareUpdateTickTime);

                foreach (var behaviour in gameObj.installedBehaviours)
                {
                    behaviour?.RareTick(null, rareUpdateTickTime);
                }
                
            }
        }


        public int rareFixedUpdateTick = 10;
        public int rareFixedUpdateCounter = 0;
        public void RareFixedUpdate()
        {
            rareUpdateTimeCounter += Time.fixedDeltaTime;
            bool goingToRareUpdate = rareUpdateTimeCounter > rareUpdateTickTime;
            foreach (var objectPool in cachedObjectPools)
            {
                if(goingToRareUpdate)TickPooledObjects(objectPool.pooledObjects);
                foreach (var gameObj in objectPool.pooledObjects)
                {
                    if (!gameObj.isActive) continue;
                    gameObj.MoveObject(gameObj.lastMovementVector, Time.fixedDeltaTime*rareFixedUpdateTick);
                }
            }
        }

        public void FixedUpdate()
        {
            rareFixedUpdateCounter++;
            if (rareFixedUpdateCounter > rareFixedUpdateTick)
            {
                rareFixedUpdateCounter = 0;
                RareFixedUpdate();
            }
        }

        public override IEnumerator Kickstart()
        {
            poolManager = this;

            var creaturesPool = new ObjectPool
            {
                pooledObjects = new GameObj[defaultPoolSize],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("enemy")
            };
            creaturesPool.FillList();
            objectPools.Add("Creatures", creaturesPool);

            var projectilesPool = new ObjectPool
            {
                pooledObjects = new GameObj[defaultPoolSize],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("projectile")
            };
            projectilesPool.FillList();
            objectPools.Add("Projectiles", projectilesPool);


            var effectsPool = new LightObjectPool
            {
                pooledObjects = new GameObject[defaultPoolSize],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("hitEffect")
            };
            effectsPool.FillList();
            lightObjectPools.Add("Effects", effectsPool);
            

            var floatingTextPool = new WorldUIObjectPool
            {
                pooledObjects = new GameObject[defaultPoolSize],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("floatingText")
            };
            floatingTextPool.FillList();
            uiObjectPools.Add("UI",floatingTextPool); 
            cachedObjectPools = objectPools.Values.ToList();

            OnPoolManagerInitiated?.Invoke(this);
            yield return this;
        }
    }

}