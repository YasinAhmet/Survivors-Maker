using System;
using System.Xml.Linq;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("behaviour")]
public class ObjBehaviourRef {
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("From")]
    public string DllName;
    
    
    [XmlArray("parameters")]
    [XmlArrayItem("parameterRequest")]
    public ParameterRequest[] parameterRequests = {};

    public XElement linkedXmlSource;

}