using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace GWBase
{
    public class PluginDef
    {
        [XmlAttribute("Name")]
        public string name;

        [XmlElement("description")]
        public string description;

        [XmlArray("behaviours")]
        [XmlArrayItem("behaviour")]
        public List<BehaviourInfo> behaviours;
    }
}