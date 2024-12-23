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
using Debug = UnityEngine.Debug;

namespace GWBase
{


    [Serializable]
    [XmlRoot("ThingDef")]
    public class ThingDef
    {
        public delegate void StatChanged(string statName, float newValue, float oldValue);

        public event StatChanged onStatChange;
        
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
        
        [XmlElement("mass")]
        public float mass = 1f;

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
            foreach (var stat in stats)
            {
                if (stat.Name == name)
                {
                    return stat;
                }
            }

            return new Stat();
        }

        public ThingDef()
        {
            
        }

        public ThingDef(ThingDef toPossess)
        {
            soundConfig = toPossess.soundConfig;
            behaviours = toPossess.behaviours;
            Name = toPossess.Name;

            List<Stat> stats = new List<Stat>();
            foreach (var stat in toPossess.stats)
            {
                Stat newStat = new()
                {
                    Name = stat.Name,
                    Value = stat.Value
                };
                stats.Add(newStat);
            }

            this.stats = stats.ToArray();
            TexturePath = toPossess.TexturePath;
            TextureSize = toPossess.TextureSize;
            mass = toPossess.mass;
            spawnable = toPossess.spawnable;
            animations = toPossess.animations;
            equipmentNames = toPossess.equipmentNames;

        }

        public float GetStatValueByName(string name)
        {
            float value = 0;
            try
            {
                foreach (var stat in stats)
                {
                    if (stat.Name == name)
                    {
                        value = float.Parse(stat.Value,CultureInfo.InvariantCulture);
                    }
                }
            }
            catch
            {
                return 0;
            }

            return value;
        }

        public void AddNewStat(string statName, float statValue)
        {
            var list = stats.ToList();
            list.Add(new Stat()
            {
                Name = statName,
                Value = statValue.ToString(CultureInfo.InvariantCulture)
            });
            stats = list.ToArray();
        }
        
        public void AddNewStat(string statName, string statValue)
        {
            var list = stats.ToList();
            list.Add(new Stat()
            {
                Name = statName,
                Value = statValue
            });
            stats = list.ToArray();
        }

        public void AddToStat(string statName, float newBonus)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    stats[i].Value = (float.Parse(stats[i].Value) + newBonus).ToString(CultureInfo.InvariantCulture);
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
                    stats[i].Value = (float.Parse(stats[i].Value) - newMinus).ToString(CultureInfo.InvariantCulture);
                    return;
                }
            }
        }

        public void ReplaceStat(string statName, float newValue)
        {
            var maxValue = GetStatValueByName("Max" + statName);
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    float oldValue = float.Parse(stats[i].Value, CultureInfo.InvariantCulture);

                    if (maxValue != 0 && newValue > maxValue)
                    {
                        stats[i].Value = maxValue.ToString(CultureInfo.InvariantCulture);
                        onStatChange?.Invoke(statName, maxValue, oldValue);
                        return;
                    }
                        
                    stats[i].Value = newValue.ToString(CultureInfo.InvariantCulture);
                    onStatChange?.Invoke(statName, newValue, oldValue);
                    return;
                }
            }

            AddNewStat(statName, newValue);
        }
        
        public void ReplaceStat(string statName, string newValue)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    stats[i].Value = newValue;
                    return;
                }
            }

            AddNewStat(statName, newValue);
        }
        
        public void ReplaceStat(string statName, float newValue, bool withoutSignal)
        {
            for (int i = 0; i < stats.Count(); i++)
            {
                if (stats[i].Name == statName)
                {
                    float oldValue = float.Parse(stats[i].Value, CultureInfo.InvariantCulture);
                    stats[i].Value = newValue.ToString(CultureInfo.InvariantCulture);
                    if(withoutSignal == false)onStatChange?.Invoke(statName, newValue, oldValue);
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
        [XmlAttribute("Value")]
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