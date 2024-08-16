using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;


[Serializable]
[XmlRoot("ThingDef")]
public class ThingDef
{
    [XmlElement("SoundConfig")]
    public SoundConfig soundConfig;

    [XmlArray("behaviours")]
    [XmlArrayItem("behaviour")]
    public string[] behaviours;

    [XmlAttribute("Name")]
    public string Name;

    [XmlArray("stats")]
    [XmlArrayItem("stat")]
    public Stat[] stats;

    [XmlElement("textureName")]
    public string TexturePath;

    [XmlElement("textureSize")]
    public float TextureSize = 1f;


    [XmlArray("equipments")]
    [XmlArrayItem("equipment")]
    public string[] equipmentNames;

    public Stat FindStatByName(string name)
    {
        UnityEngine.Debug.Log($"Trying to find {name} stat...");
        return stats.FirstOrDefault(x => x.Name.Equals(name));
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
}



[XmlRoot("parameterRequest")]
public class ParameterRequest
{


    [XmlAttribute("Type")]
    public string requestedType = "typeof";
}