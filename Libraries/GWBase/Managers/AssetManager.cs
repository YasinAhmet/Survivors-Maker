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
using static GWBase.AnimationSheet;
namespace GWBase {

public class AssetManager : Manager
{
    public static AssetManager assetLibrary;
    [SerializeField] public Dictionary<string, ThingDef> initializedThingDefsDictionary = new();
    [SerializeField] public Dictionary<string, XElement> thingDefsDictionary = new();
    [SerializeField] public Dictionary<string, UpgradeDef> upgradeDefsDictionary = new();
    [SerializeField] public Dictionary<string, ObjBehaviourRef> behaviourDictionary = new();
    [SerializeField] public Dictionary<string, AudioClip> audioDictionary = new();
    [SerializeField] public Dictionary<string, ThingDef> actionsDictionary = new();
    [SerializeField] public Dictionary<string, Assembly> assemblyDictionary = new();
    [SerializeField] public Dictionary<string, Sprite> texturesDictionary = new();
    [SerializeField] public Dictionary<string, AnimationSheet> animationSheetsDictionary = new();
    [SerializeField] public Dictionary<string, Texture2D> rawAnimationSheetsDictionary = new();
    private string[] validTextureExtensions = { ".jpg", ".jpeg", ".png" };
    private string[] validSoundExtensions = { ".mp3" };
    public List<string> NameOfLoadedAssemblies = new List<string>();
    public List<string> NameOfLoadedBehaviours = new List<string>();
    public List<string> NameOfLoadedTextures = new List<string>();
    public List<string> NameOfLoadedThings = new List<string>();
    public List<AnimationSheet> SheetOfLoadedAnims = new List<AnimationSheet>();
    public List<Texture2D> SheetOfLoadedRawAnims = new List<Texture2D>();
    public string dllFolder = "Assembly";
    public string texturesFolder = "Resources/Textures";
    public string animationsFolder = "Resources/Animations";
    public string soundFolder = "Sound";
    public string pathToGameDatabase = "/Mods/.GameDatabase/";
    public string pathToAssemblies {
        get { return projectPath + "/Mods/" + dllFolder; }
    }
    public string fullPathToGameDatabase {
        get { return projectPath + pathToGameDatabase; }
    }
    public string projectPath;
    public string uriStart = "file://";

        public override IEnumerator Kickstart()
    {
        assetLibrary = this;
        projectPath = Application.dataPath;
        yield return StartCoroutine(LoadCustomSounds(fullPathToGameDatabase+soundFolder));
        LoadCustomAssemblies(pathToAssemblies);
        LoadCustomTextures(fullPathToGameDatabase+texturesFolder);
        LoadRawAnimationSheets(fullPathToGameDatabase+animationsFolder);
        LoadDefs();
    }

    void LoadCustomAssemblies(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var filePath in files)
        {
            if (!Path.GetExtension(filePath).Equals(".dll")) continue;
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
        var path = fullPathToGameDatabase+"Creatures";
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            LoadDef(path, Path.GetFileName(file));
        }

        path = fullPathToGameDatabase+"Behaviours";
        files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            LoadBehaviours(path, Path.GetFileName(file));
        }

        path = fullPathToGameDatabase+"Actions";
        files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            LoadActions(path, Path.GetFileName(file));
        }

        path = fullPathToGameDatabase+"Upgrades";
        files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            LoadUpgrades(path, Path.GetFileName(file));
        }


        path = fullPathToGameDatabase+"Animations";
        files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            LoadAnimationDef(path, Path.GetFileName(file));
        }
    }

    void LoadUpgrades(string path, string fileName) {
        var defFile = XElement.Load(path + "/" + fileName);
        var upgradeDefXMLs = defFile.Elements("UpgradeDef").Where(x => x.Attribute("abstract") == null);

        foreach (var def in upgradeDefXMLs)
        {
            UpgradeDef upgradeDef = YKUtility.FromXElement<UpgradeDef>(def);
            upgradeDefsDictionary.Add(upgradeDef.upgradeName, upgradeDef);
            NameOfLoadedThings.Add(upgradeDef.upgradeName);
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

    void LoadAnimationDef(string path, string fileName)
    {
        var defFile = XElement.Load(path + "/" + fileName);
        var animDefs = defFile.Elements("AnimationDef").Where(x => x.Attribute("abstract") == null);

        foreach (var animDef in animDefs)
        {
            AnimationSheet animationSheet = new AnimationSheet();
            animationSheet.info = YKUtility.FromXElement<AnimationSheetInfo>(animDef);
            Texture2D correspondingAnimationSheet = rawAnimationSheetsDictionary.FirstOrDefault(x => x.Key == animationSheet.info.sheetName).Value;
            animationSheet.Initiate(correspondingAnimationSheet, animationSheet.info.frameCount);
            animationSheetsDictionary.Add(animationSheet.info.sheetName, animationSheet);
            SheetOfLoadedAnims.Add(animationSheet);
            Debug.Log("[LOAD] Animation Def Added From [" + path + "]: " + fileName);
        }
    }

    void LoadRawAnimationSheets(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var filePath in files)
        {
            if (!validTextureExtensions.Any(x => x.Equals(Path.GetExtension(filePath)))) continue;

            var texture2D = LoadPNG(filePath);
            rawAnimationSheetsDictionary.Add(Path.GetFileNameWithoutExtension(filePath), texture2D);
            SheetOfLoadedRawAnims.Add(texture2D);
            Debug.Log($"[LOAD] Animation Raw Added From [{filePath }] in the name of { Path.GetFileNameWithoutExtension(filePath) }");
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

}