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
        [XmlArray("entities")] [XmlArrayItem("entity")]
        public EntityRef[] entities;

        public List<EntityRef> entitiesListedByWeight = new List<EntityRef>();
        public int cachedLevel = 0;
        public float cachedTotalWeight = 0;

        public void Load()
        {
            PlayerController.playerController.onXP += TickLevel;
            TickLevel(PlayerController.playerController.currentLevel);
        }

        public void TickLevel(PlayerController.LevelInfo levelInfo)
        {
            Debug.Log("LEVEL: " + levelInfo.level);
            if (cachedLevel != levelInfo.level)
            {
                cachedLevel = levelInfo.level;
                CacheWeights(cachedLevel);
            }
        }
        
        public void CacheWeights(int lvl)
        {
            entitiesListedByWeight.Clear();
            cachedTotalWeight = 0;

            foreach (var entity in entities)
            {
                EntityRef entityRef = entity;
                float lvlDistance = 1+(Math.Abs((Math.Abs(lvl) - Math.Abs(entityRef.tier))*tierLvlDistribution));
                float entitysWeight = entityRef.rarity / (lvlDistance);
                entityRef.cachedWeight = entitysWeight;
                if(entitysWeight>cachedTotalWeight)cachedTotalWeight = entitysWeight;
                entitiesListedByWeight.Add(entityRef);
            }
            
            entitiesListedByWeight.OrderBy(x => x.cachedWeight);
        }

        public EntityRef GetRandomEntity()
        {
            float randomWeight = Random.Range(0, cachedTotalWeight);
            foreach (var entity in entitiesListedByWeight)
            {
                if (entity.cachedWeight >= randomWeight) return entity;
                
            }
            
            Debug.Log("Haven't found an entity for " + randomWeight+"/"+cachedTotalWeight);
            return new EntityRef();
        }
    }

    [XmlRoot("entity")]
    public struct EntityRef
    {
        [XmlAttribute("DefName")] public string defName;
        [XmlAttribute("Tier")] public int tier;
        [XmlElement("rarity")] public float rarity;
        public float cachedWeight;
    }
}