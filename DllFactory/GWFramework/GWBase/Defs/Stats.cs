using System;
using System.Xml.Serialization;


[Serializable]
[XmlRoot("stat")]
public struct Stat {

    [XmlAttribute("Name")]
    public string Name;
    
    [XmlText]
    public string Value;
}