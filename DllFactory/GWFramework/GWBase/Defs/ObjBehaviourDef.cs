using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using static ThingDef;

[Serializable]
[XmlRoot("behaviour")]
public class ObjBehaviourRef {
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("IsOneTime")]
    public string isOneTime = "No";
    [XmlAttribute("From")]
    public string DllName;
    
    
    [XmlArray("parameters")]
    [XmlArrayItem("parameterRequest")]
    public ParameterRequest[] parameterRequests = {};
    

    public XElement linkedXmlSource;

}

[Serializable]
[XmlRoot("behaviour")]
    public struct BehaviourInfo {
        [XmlAttribute("Name")]
        public string behaviourName;
        [XmlArray("customParameters")]
        [XmlArrayItem("parameter")]
        public List<CustomParameter> customParameters;

    }