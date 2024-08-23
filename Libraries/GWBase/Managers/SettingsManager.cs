using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
namespace GWBase
{

    public class SettingsManager : Manager
    {

        public GameObject inputSpeaker;
        public static PlayerSettings playerSettings;
        public PlayerController playerController;
        public static SettingsManager settingsManager;
        public List<Plugin> loadedUpPlugins = new List<Plugin>();
        public List<IObjBehaviour> loadedUpBehaviours = new List<IObjBehaviour>();
        public bool loaded;
        public override IEnumerator Kickstart()
        {
            settingsManager = this;
            Debug.Log($"[SETTINGS] Settings manager initializing..");
            playerController = new();
            LoadSettings();
            yield return StartCoroutine(playerController.Start());

            loaded = true;
            yield return this;
        }

        public void LoadSettings()
        {
            var defFile = XElement.Load(AssetManager.assetLibrary.fullPathToGameDatabase + "UserSettings.xml");
            playerSettings = YKUtility.FromXElement<PlayerSettings>(defFile.Element("PlayerSettings"));
            
            foreach (var plugin in defFile.Element("GamePlugins").Elements("plugin"))
            {
                Plugin serializedPlugin = YKUtility.FromXElement<Plugin>(plugin);
                SerializePlugin(serializedPlugin);
            }
        }

        public void SerializePlugin(Plugin plugin) {
            foreach (var behaviourDef in plugin.behaviours)
            {
                SerializeBehaviour(behaviourDef);
            }
            
            loadedUpPlugins.Add(plugin);
        }

        public void SerializeBehaviour(BehaviourInfo behaviourDef) {
            var keyPair = AssetManager.assetLibrary.behaviourDictionary.FirstOrDefault(x => x.Key == behaviourDef.behaviourName);
            ObjBehaviourRef foundBehaviour = keyPair.Value;
            var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
            Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
            IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);
            newBehaviour.Start(null, null, behaviourDef.customParameters.ToArray());
            loadedUpBehaviours.Add(newBehaviour);
        }

        [XmlRoot("PlayerSettings")]
        public struct PlayerSettings
        {

            [XmlElement("playerGroup")]
            public string playerGroup;

            [XmlElement("playerCharacter")]
            public string playerCharacter;
        }

        [XmlRoot("plugin")]
        public struct Plugin {
            [XmlAttribute("Name")]
            public string name;
            [XmlElement("description")]
            public string description;
            [XmlArray("behaviours")]
            [XmlArrayItem("behaviour")]
            public List<BehaviourInfo> behaviours;
        }
    }

}