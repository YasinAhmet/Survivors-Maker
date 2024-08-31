using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace GWBase
{

    public class PoolManager : Manager
    {
        public static PoolManager poolManager;
        [SerializeField] public ObjectPool creaturesPool;
        [SerializeField] public ObjectPool projectilesPool;
        [SerializeField] public LightObjectPool effectsPool;
        [SerializeField] public WorldUIObjectPool floatingTextPool;
        [SerializeField] protected float rareUpdateTickTime = 1.5f;
        public float rareUpdateTimeCounter = 0;

        public void FixedUpdate()
        {
            rareUpdateTimeCounter += Time.fixedDeltaTime;

            foreach (var creature in creaturesPool.pooledObjects)
            {
                if (!creature.gameObject.activeSelf) continue;
                creature.MoveObject(creature.lastMovementVector, Time.fixedDeltaTime);

                foreach (var behaviour in creature.installedBehaviours)
                {
                    behaviour?.Tick(null, Time.fixedDeltaTime);
                }

                if (rareUpdateTimeCounter > rareUpdateTickTime)
                {
                    creature.RareTick(rareUpdateTickTime);

                    foreach (var behaviour in creature.installedBehaviours)
                    {
                        behaviour?.RareTick(null, rareUpdateTickTime);
                    }
                }

            }

            foreach (var projectile in projectilesPool.pooledObjects)
            {
                if (!projectile.gameObject.activeSelf) continue;

                foreach (var behaviour in projectile.installedBehaviours)
                {
                    behaviour?.Tick(null, Time.fixedDeltaTime);
                }

                if (rareUpdateTimeCounter > rareUpdateTickTime)
                {
                    projectile.RareTick(rareUpdateTickTime);
                    foreach (var behaviour in projectile.installedBehaviours)
                    {
                        behaviour?.RareTick(null, rareUpdateTickTime);
                    }

                    rareUpdateTimeCounter = 0;
                }

            }

        }

        public override IEnumerator Kickstart()
        {
            poolManager = this;


            creaturesPool = new ObjectPool
            {
                pooledObjects = new GameObj[50],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("enemy")
            };
            creaturesPool.FillList();

            projectilesPool = new ObjectPool
            {
                pooledObjects = new GameObj[50],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("projectile")
            };
            projectilesPool.FillList();


            effectsPool = new LightObjectPool
            {
                pooledObjects = new GameObject[50],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("hitEffect")
            };
            effectsPool.FillList();

            floatingTextPool = new WorldUIObjectPool
            {
                pooledObjects = new GameObject[50],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("floatingText")
            };
            floatingTextPool.FillList();

            yield return this;
        }
    }

}