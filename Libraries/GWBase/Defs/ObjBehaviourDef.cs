using System.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
namespace GWBase
{

    [Serializable]
    [XmlRoot("behaviour")]
    public class ObjBehaviourRef
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlAttribute("IsOneTime")]
        public string isOneTime = "No";
        [XmlAttribute("From")]
        public string DllName;
        [XmlAttribute("Namespace")]
        public string Namespace;


        [XmlArray("parameters")]
        [XmlArrayItem("parameterRequest")]
        public ParameterRequest[] parameterRequests = { };
        public XElement linkedXmlSource;

    }

    [Serializable]
    [XmlRoot("behaviour")]
    public struct BehaviourInfo
    {
        [XmlAttribute("Name")]
        public string behaviourName;
        [XmlArray("customParameters")]
        [XmlArrayItem("parameter")]
        public List<CustomParameter> customParameters;
    }
}