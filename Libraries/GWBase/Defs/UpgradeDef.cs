using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
namespace GWBase {

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

public interface IUpgradeTaker{
    public void PossessUpgrade(UpgradeDef def);
    public UpgradeDef GetPossessed();
    public IEnumerator WaitForPickComplete();
}
}