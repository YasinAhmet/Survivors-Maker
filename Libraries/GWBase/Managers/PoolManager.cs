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