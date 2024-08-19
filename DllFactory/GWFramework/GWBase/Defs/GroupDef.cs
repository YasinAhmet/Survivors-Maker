using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static ThingDef;

[Serializable]
[XmlRoot("ThingDef")]
public class GroupDef
{
    [XmlAttribute("Name")]
    public string groupName;
    
    [XmlElement("description")]
    public string description;
    
    [XmlElement("spawnable")]
    public SpawnableInfo spawnableInfo;

    [XmlArray("members")]
    [XmlArrayItem("member")]
    public List<GroupMember> members;

    [XmlRoot("member")]
    public struct GroupMember {
        [XmlAttribute("Leader")]
        public string isLeaderSTR;

        [XmlAttribute("Name")]
        public string defName;

        [XmlArray("behaviours")]
        [XmlArrayItem("behaviour")]
        public List<string> extraBehaviours;
    }
}