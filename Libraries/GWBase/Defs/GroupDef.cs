using System.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GWBase
{

    [Serializable]
    [XmlRoot("ThingDef")]
    public class GroupDef
    {
        [XmlArray("positions")]
        [XmlArrayItem("position")]
        public GroupPositionData[] positions;
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
        public struct GroupMember
        {
            [XmlAttribute("Leader")]
            public string isLeaderSTR;

            [XmlAttribute("Name")]
            public string defName;

            [XmlArray("behaviours")]
            [XmlArrayItem("behaviour")]
            public List<BehaviourInfo> extraBehaviours;
        }
    }

    [XmlRoot("position")]
    public struct GroupPositionData {
        [XmlElement("x")]
        public float x;
        [XmlElement("y")]
        public float y;
    }
}