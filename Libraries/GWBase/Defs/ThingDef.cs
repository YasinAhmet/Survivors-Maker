using System.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
namespace GWBase
{


    [Serializable]
    [XmlRoot("ThingDef")]
    public class ThingDef
    {
        [XmlElement("SoundConfig")]
        public SoundConfig soundConfig;

        [XmlArray("behaviours")]
        [XmlArrayItem("behaviour")]
        public BehaviourInfo[] behaviours;

        [XmlAttribute("Name")]
        public string Name;

        [XmlArray("stats")]
        [XmlArrayItem("stat")]
        public Stat[] stats;

        [XmlElement("textureName")]
        public string TexturePath;

        [XmlElement("textureSize")]
        public float TextureSize = 1f;

        [XmlElement("spawnable")]
        public SpawnableInfo spawnable;

        [XmlArray("animations")]
        [XmlArrayItem("animation")]
        public AnimationInfo[] animations;

        [XmlArray("equipments")]
        [XmlArrayItem("equipment")]
        public EquipmentInfo[] equipmentNames;

        public Stat FindStatByName(string name)
        {
            return stats.FirstOrDefault(x => x.Name.Equals(name));
        }

        public float GetStatValueByName(string name)
        {
            return float.Parse(stats.FirstOrDefault(x => x.Name.Equals(name)).Value, CultureInfo.InvariantCulture);
        }

        public void AddNewStat(string statName, float statValue)
        {
            var list = stats.ToList();
            list.Add(new Stat()
            {
                Name = statName,
                Value = statValue.ToString()
            });
            stats = list.ToArray();
        }

        public void AddToStat(string statName, float newBonus)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    stats[i].Value = (float.Parse(stats[i].Value) + newBonus).ToString();
                    return;
                }
            }
        }
        public void RemoveFromStat(string statName, float newMinus)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    stats[i].Value = (float.Parse(stats[i].Value) - newMinus).ToString();
                    return;
                }
            }
        }

        public void ReplaceStat(string statName, float newValue)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    stats[i].Value = newValue.ToString();
                    return;
                }
            }

            AddNewStat(statName, newValue);
        }
    }

    [Serializable]
    [XmlRoot("equipment")]
    public struct EquipmentInfo {
        [XmlAttribute("Name")]
        public string name;
        
        [XmlElement("offset")]
        public OffsetInfo offset;
    }

    [Serializable]
    [XmlRoot("offset")]
    public struct OffsetInfo {
        [XmlElement("x")]
        public float x;
        [XmlElement("y")]
        public float y;
        [XmlElement("z")]
        public float z;
        [XmlElement("flipOffset")]
        public string flipOffset;
    }

    [Serializable]
    [XmlRoot("animation")]
    public struct AnimationInfo {
        [XmlAttribute("Name")]
        public string typeName;

        [XmlAttribute("SheetName")]
        public string sheetName;
    }

    [Serializable]
    [XmlRoot("parameter")]
    public struct CustomParameter
    {
        [XmlAttribute("Name")]
        public string parameterName;
        [XmlElement("value")]
        public string parameterValue;
    }

    [Serializable]
    [XmlRoot("spawnable")]
    public struct SpawnableInfo
    {
        [XmlAttribute("Type")]
        public string type;
        [XmlElement("faction")]
        public string faction;
        [XmlElement("level")]
        public int level;
    }

    [Serializable]
    [XmlRoot("SoundConfig")]
    public struct SoundConfig
    {
        [XmlArray("DamageTaken")]
        [XmlArrayItem("sound")]
        public string[] onDamageTakenSounds;


        [XmlArray("Death")]
        [XmlArrayItem("sound")]
        public string[] onDeathSounds;
    }

    [Serializable]
    [XmlRoot("parameterRequest")]
    public class ParameterRequest
    {


        [XmlAttribute("Type")]
        public string requestedType = "typeof";
    }
}