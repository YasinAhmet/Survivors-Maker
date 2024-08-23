using System;
using System.Xml.Serialization;
namespace GWBase {


[Serializable]
[XmlRoot("stat")]
public struct Stat {

    [XmlAttribute("Name")]
    public string Name;
    
    [XmlText]
    public string Value;
}
}