using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Runtime;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class AssetManager : Manager
{
    public static AssetManager assetLibrary;
    [SerializeField] public Dictionary<string, ThingDef> initializedThingDefsDictionary = new();
    [SerializeField] public Dictionary<string, XElement> thingDefsDictionary = new();
    [SerializeField] public Dictionary<string, ObjBehaviourRef> behaviourDictionary = new();
    [SerializeField] public Dictionary<string, AudioClip> audioDictionary = new();
    [SerializeField] public Dictionary<string, ThingDef> actionsDictionary = new();
    [SerializeField] public Dictionary<string, Assembly> assemblyDictionary = new();
    [SerializeField] public Dictionary<string, Sprite> texturesDictionary = new();
    private string[] validTextureExtensions = { ".jpg", ".jpeg", ".png" };
    private string[] validSoundExtensions = { ".mp3" };
    public List<string> NameOfLoadedAssemblies = new List<string>();
    public List<string> NameOfLoadedBehaviours = new List<string>();
    public List<string> NameOfLoadedTextures = new List<string>();
    public List<string> NameOfLoadedThings = new List<string>();
    public string pathToDlls = ".Mods/Assembly";
    public string pathToTextures = "Resources/Textures";
    public string pathToSound = "Resources/Effects/Sound";
    public string extensionForDlls = ".dll";
    public string projectPath;
    public string uriStart = "file://";
    public override IEnumerator Kickstart()
    {
        assetLibrary = this;
        projectPath = Application.dataPath;
        yield return StartCoroutine(LoadCustomSounds(projectPath+"/"+pathToSound));
        LoadCustomAssemblies(projectPath+"/"+pathToDlls);
        LoadCustomTextures(projectPath+"/"+pathToTextures);
        LoadDefs();
    }
    void LoadCustomAssemblies(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var filePath in files)
        {
            if (!Path.GetExtension(filePath).Equals(extensionForDlls)) continue;
            Assembly assembly = Assembly.LoadFrom(filePath);
            assemblyDictionary.Add(assembly.GetName().Name, assembly);
            Debug.Log("[LOAD] Assembly Added: " + assembly.GetName().Name);
            NameOfLoadedAssemblies.Add(assembly.GetName().Name);
        }
    }

    IEnumerator LoadCustomSounds(string path)
    {
        Debug.Log("[LOADING] Sounds : " + path);
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            if (!validSoundExtensions.Any(x => x.Equals(Path.GetExtension(file)))) continue;
            yield return StartCoroutine(AddAudioClip(path, Path.GetFileName(file)));
        }

        yield return this;
    }

    IEnumerator AddAudioClip(string path, string fileName)
    {
        string uri = $"{uriStart}{path}/{fileName}";
        Debug.Log("[LOADING] Sound : " + uri);
        using (var req = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG)) {
            yield return req.SendWebRequest();
            var loadedClip = DownloadHandlerAudioClip.GetContent(req);

            audioDictionary.Add(fileName, loadedClip);
            NameOfLoadedTextures.Add(fileName);
        }
    }

    void LoadDefs()
    {
        var files = Directory.GetFiles(projectPath+"/.Mods/GameDatabase/Creatures");
        foreach (var file in files)
        {
            LoadDef(projectPath+"/.Mods/GameDatabase/Creatures", Path.GetFileName(file));
        }

        files = Directory.GetFiles(projectPath+"/.Mods/GameDatabase/Behaviours");
        foreach (var file in files)
        {
            LoadBehaviours(projectPath+"/.Mods/GameDatabase/Behaviours", Path.GetFileName(file));
        }

        files = Directory.GetFiles(projectPath+"/.Mods/GameDatabase/Actions");
        foreach (var file in files)
        {
            LoadActions(projectPath+"/.Mods/GameDatabase/Actions", Path.GetFileName(file));
        }
    }

    void LoadActions(string path, string fileName)
    {
        foreach (var def in GetDefList(path, fileName))
        {
            actionsDictionary.Add(def.Name, def);
            NameOfLoadedThings.Add(def.Name);
        }
    }


    void LoadBehaviours(string path, string fileName)
    {
        var defFile = XElement.Load(path + "/" + fileName);
        var behaviours = defFile.Elements("behaviour").Where(x => x.Attribute("abstract") == null);

        foreach (var behaviour in behaviours)
        {
            var newBehaviour = YKUtility.FromXElement<ObjBehaviourRef>(behaviour);
            newBehaviour.linkedXmlSource = behaviour;
            behaviourDictionary.Add(newBehaviour.Name, newBehaviour);
            NameOfLoadedBehaviours.Add(newBehaviour.Name);
            Debug.Log($"[LOAD] Behaviour Added From [{fileName}]: {newBehaviour.Name}. The behaviour's target dll is {newBehaviour.DllName}");
        }
    }

    void LoadDef(string path, string fileName)
    {
        var defFile = XElement.Load(path + "/" + fileName);
        var thingDefs = defFile.Elements("ThingDef").Where(x => x.Attribute("abstract") == null);

        foreach (var thingDef in thingDefs)
        {
            ThingDef converted = YKUtility.FromXElement<ThingDef>(thingDef);
            thingDefsDictionary.Add(converted.Name, thingDef);
            initializedThingDefsDictionary.Add(converted.Name, converted);

            NameOfLoadedThings.Add(converted.Name);
            Debug.Log("[LOAD] Thing Def Added From [" + fileName + "]: " + converted.Name);
        }
    }

    public List<ThingDef> GetDefList(string path, string fileName)
    {
        var defFile = XElement.Load(path + "/" + fileName);
        var thingDefs = defFile.Elements("ThingDef").Where(x => x.Attribute("abstract") == null);

        List<ThingDef> loadedUpThings = new();
        foreach (var thingDef in thingDefs)
        {
            loadedUpThings.Add(YKUtility.FromXElement<ThingDef>(thingDef));
        }

        return loadedUpThings;
    }

    void LoadCustomTextures(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var filePath in files)
        {
            //Debug.Log(Path.GetExtension(filePath) + " " + validTextureExtensions.Any(x => x.Equals(Path.GetExtension(filePath))));
            if (!validTextureExtensions.Any(x => x.Equals(Path.GetExtension(filePath)))) continue;

            var texture2D = LoadPNG(filePath);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texturesDictionary.Add(Path.GetFileNameWithoutExtension(filePath), sprite);
            NameOfLoadedTextures.Add(Path.GetFileNameWithoutExtension(filePath));
            Debug.Log("[LOAD] Texture Added From [" + filePath + "]: " + Path.GetFileNameWithoutExtension(filePath));
        }
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, fileData);
        }
        return tex;
    }
    public Assembly GetAssembly(string name)
    {
        return assemblyDictionary.FirstOrDefault(x => x.Key.Equals(name)).Value;
    }
}
