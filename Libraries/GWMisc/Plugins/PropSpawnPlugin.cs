using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GWMisc
{
    public class PropSpawnPlugin : IObjBehaviour
    {
        public float timePasssed;
        public AssetManager assetLibrary;
        public LightObjectPool pool;
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            var effectsPool = new LightObjectPool
            {
                pooledObjects = new GameObject[100],
                attachedPrefab = PrefabManager.prefabManager.GetPrefabOf("hitEffect")
            };
            effectsPool.FillList();
            PoolManager.poolManager.lightObjectPools.Add("Props", effectsPool);
            pool = PoolManager.poolManager.GetLightObjectPool("Props"); 
            assetLibrary = AssetManager.assetLibrary;
        }

        public void Start(XElement possess, object[] parameters)
        {
        }

        public void Tick(object[] parameters, float deltaTime)
        {
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            var map = SpawnManager.spawnManager.currentMap;
            if (map == null) return;
            timePasssed += deltaTime;

            if (timePasssed > map.eventSpawnInterval)
            {
                timePasssed = 0;
                TrySpawnProp(map);
            }
        }
        
        public void TrySpawnProp(Map map)
        {
            var pickedProp = map.GetRandomEntity(map.cachedPropsPack);
            var location = SpawnManager.spawnManager.GetRandomSpawnPosition();
            var foundObjSlot = pool.ObtainSlotForType(location, 0);
            if (!foundObjSlot || pickedProp.defName == string.Empty) return;
            var foundThingDef = assetLibrary.initializedThingDefsDictionary[pickedProp.defName];
            var sprite = assetLibrary.texturesDictionary[foundThingDef.TexturePath];
            var spriteRenderer = foundObjSlot.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            foundObjSlot.transform.localScale = new Vector3(foundThingDef.TextureSize, foundThingDef.TextureSize, 0);
            var prop = foundObjSlot.AddComponent<Prop>();
            foundObjSlot.AddComponent<CircleCollider2D>();
            prop.xpToGiveOnBreak = foundThingDef.GetStatValueByName("XPValue");

        }

        public void Suspend(object[] parameters)
        {
        }

        public string GetName()
        {
            throw new System.NotImplementedException();
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}