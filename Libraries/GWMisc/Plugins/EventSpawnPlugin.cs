using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GWMisc
{
    public class EventSpawnPlugin : IObjBehaviour
    {
        public float timePasssed;
        public Map map;
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            map = SpawnManager.spawnManager.currentMap;
        }

        public void Start(XElement possess, object[] parameters)
        {
        }

        public void Tick(object[] parameters, float deltaTime)
        {
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
             
            if (map == null) map = SpawnManager.spawnManager.currentMap;
            if (map == null) return;
            timePasssed += deltaTime;

            if (timePasssed > map.eventSpawnInterval)
            {
                timePasssed = 0;
                TryThrowEvent(map);
            }
        }
        
        public void TryThrowEvent(Map map)
        {
            if (Random.Range(0, 1) < map.eventSpawnChance + (map.cachedLevel * map.eventSpawnChanceIncrease))
            {
                var pickedEvent = map.GetRandomEntity(map.cachedEventsPack);
                
                ObjBehaviourRef foundBehaviour = AssetManager.assetLibrary.GetBehaviour(pickedEvent.defName);
                var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
                Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
                IObjBehaviour instance = (IObjBehaviour)System.Activator.CreateInstance(targetType);
                
                instance.Start(null, null, pickedEvent.customParameters.ToArray());
                if(foundBehaviour.isOneTime != "Yes")map.installedBehaviours.Add(instance);
            }
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