using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
public class SettingsManager : Manager
{

    public GameObject inputSpeaker;
    public static PlayerSettings playerSettings;
    public  PlayerController playerController;
    public static SettingsManager settingsManager;
    public override IEnumerator Kickstart()
    {        
        settingsManager = this;
        Debug.Log($"[SETTINGS] Settings manager initializing..");
        playerController = new();
        LoadSettings();
        yield return StartCoroutine(playerController.Start());
        yield return this;
    }

    public void LoadSettings() {
        var defFile = XElement.Load(AssetManager.assetLibrary.projectPath+"/.Mods/GameDatabase" + "/" + "UserSettings.xml");
        playerSettings = YKUtility.FromXElement<PlayerSettings>(defFile.Element("PlayerSettings"));
    }

    [XmlRoot("PlayerSettings")]
    public struct PlayerSettings {

    [XmlElement("playerGroup")]
    public string playerGroup;

    [XmlElement("playerCharacter")]
    public string playerCharacter;
    }
}
