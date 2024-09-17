using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
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
        public List<ObjectPool> cachedObjectPools = new List<ObjectPool>();
        public bool enabled = true;
        public bool multiThreadTick = true;
        JobHandle poolTickHandle;

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

        private void OnDisable()
        {
            enabled = false;
        }

        public void PoolTick(float deltaTime)
        {
            
            foreach (var objectPool in PoolManager.poolManager.cachedObjectPools)
            {
                foreach (var gameObj in objectPool.pooledObjects)
                {
                    if (!gameObj.isActive) continue;
                    gameObj.MoveObject(gameObj.lastMovementVector,deltaTime);
                    foreach (var behaviour in gameObj.installedBehaviours)
                    {
                        behaviour?.RareTick(null, rareUpdateTickTime);
                    }

                    gameObj.RareTick(rareUpdateTickTime);
                }
            }
        }

        private float timePassedSinceLastTick = 1;
        private void FixedUpdate()
        {
            timePassedSinceLastTick += Time.fixedDeltaTime;
            if (timePassedSinceLastTick > rareUpdateTickTime)
            {
                PoolTick(timePassedSinceLastTick);
                timePassedSinceLastTick = 0;
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
            enabled = true;
            rareUpdateTickTime = SettingsManager.playerSettings.rareTickTime;
            yield return this;
        }
        
    }

}