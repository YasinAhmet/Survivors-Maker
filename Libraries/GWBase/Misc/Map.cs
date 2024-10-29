using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GWBase
{
    [Serializable]
    [XmlRoot("Map")]
    public class Map
    {
        [XmlElement("tierLvlDistr")] public float tierLvlDistribution;

        [XmlArray("customParameters")] [XmlArrayItem("parameter")]
        public CustomParameter[] parameters;

        [XmlElement("content")]
        public Content[] contents;
        
        [XmlArray("behaviours")]
        [XmlArrayItem("behaviour")]
        public BehaviourInfo[] behaviours;

        [XmlElement("background")] public string backgroundID;
        [XmlElement("backgroundSize")] public float backgroundSize;
        
        public Content cachedSpawnablesPack;
        public Content cachedEventsPack;
        public Content cachedPropsPack;
        
        [XmlIgnoreAttribute]
        public List<IObjBehaviour> installedBehaviours = new List<IObjBehaviour>();
        
        public int cachedLevel = 0;

        [XmlElement("spawnSpeed")] public float spawnSpeed;
        [XmlElement("propSpawnInterval")] public float propSpawnInterval;
        [XmlElement("eventSpawnInterval")] public float eventSpawnInterval;
        [XmlElement("eventSpawnChance")] public float eventSpawnChance;
        [XmlElement("eventSpawnChanceIncrease")] public float eventSpawnChanceIncrease;
        
        public Content GetPackByType(string type)
        {
            foreach (var pack in contents)
            {
                if (pack.type == type) return pack;
            }

            return new Content();
        }
        
        public void Load()
        {
            PlayerController.playerController.onXP += TickLevel;
            PossessBehaviours(behaviours, true);
            TickLevel(PlayerController.playerController.currentLevel);
        }
        
        public virtual void PossessBehaviours(BehaviourInfo[] behaviourInfo, bool clean)
        {
            if (installedBehaviours != null && clean) installedBehaviours.Clear();
            if (behaviourInfo == null || behaviourInfo == Array.Empty<BehaviourInfo>()) return;

            foreach (var behaviour in behaviourInfo)
            {
                ObjBehaviourRef foundBehaviour = AssetManager.assetLibrary.GetBehaviour(behaviour.behaviourName);
                var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
                Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
                IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);
                
                newBehaviour.Start(foundBehaviour.linkedXmlSource, null, behaviour.customParameters?.ToArray());
                if (foundBehaviour.isOneTime == "No") installedBehaviours.Add(newBehaviour);
            }
        }

        public void RareTick(float delta)
        {
            if (installedBehaviours == null || installedBehaviours.Count == 0) return;
            
            foreach (var behaviour in installedBehaviours)
            {
                behaviour?.RareTick(null, delta);
            }
        }
        

        public void TickLevel(PlayerController.LevelInfo levelInfo)
        {
            if (cachedLevel != levelInfo.level)
            {
                cachedLevel = levelInfo.level;

                for (int i = 0; i < contents.Length; i++)
                {
                    var content = contents[i];
                    contents[i] = CacheWeights(cachedLevel, content, content.entities);
                }
            }

            cachedSpawnablesPack = GetPackByType("Hostiles");
            cachedEventsPack = GetPackByType("Events");
            cachedPropsPack = GetPackByType("Props");
        }

        public Content CacheWeights(int lvl, Content pack, EntityRef[] source)
        {
            if (source == null || source.Length == 0) return new Content();
            pack.calculatedList?.Clear();
            pack.weight = 0;

            foreach (var entity in source)
            {
                EntityRef entityRef = entity;
                float lvlDistance = 1+(Math.Abs(lvl - entityRef.tier)*tierLvlDistribution);
                float entitysWeight = entityRef.rarity / (lvlDistance);
                entityRef.cachedWeight = entitysWeight;
                pack.weight += entitysWeight;
                pack.calculatedList.Add(entityRef);
            }
            
            pack.calculatedList.OrderBy(x => x.cachedWeight);
            return pack;
        }

        public EntityRef GetRandomEntity(Content pack)
        {
            float randomWeight = Random.Range(0, pack.weight);
            foreach (var entity in pack.calculatedList)
            {
                if (entity.cachedWeight >= randomWeight) return entity;
                randomWeight -= entity.cachedWeight;
            }
            
            return new EntityRef();
        }
    }

    [Serializable]
    [XmlRoot("entity")]
    public struct EntityRef
    {
        [XmlAttribute("DefName")] public string defName;
        [XmlAttribute("Tier")] public int tier;
        [XmlElement("rarity")] public float rarity;
        public float cachedWeight;
        
        [XmlArray("customParameters")]
        [XmlArrayItem("parameter")]
        public List<CustomParameter> customParameters;
    }

    [Serializable]
    public struct Content
    {
        [XmlElement("entity")]
        public EntityRef[] entities;
        
        public List<EntityRef> calculatedList;
        public float weight;

        [XmlAttribute("Type")] public string type;
    }

    
}