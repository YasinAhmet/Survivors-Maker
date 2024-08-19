using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static ThingDef;

[Serializable]
[XmlRoot("UpgradeDef")]
public class UpgradeDef
{
    [XmlAttribute("Name")]
    public string upgradeName;
    
    [XmlElement("description")]
    public string description;
    
    [XmlArray("behaviours")]
    [XmlArrayItem("behaviour")]
    public List<BehaviourInfo> behaviours;

}