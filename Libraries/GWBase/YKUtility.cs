using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using GWBase;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public static class YKUtility
{
    public static bool Random
    {
        get
        {
            return RandomBool();
        }
    }

    public static Vector3 GetRandomPosition(float range)
    {
        return new Vector3(UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range), 1);
    }
    
    public static bool RandomBool()
    {
        return UnityEngine.Random.Range(0, 2) == 1;
    }
    public static int GetRandomIndex<T>(List<T> list)
    {
        if (list.Count == 0) return 0;

        System.Random random = new System.Random();
        return random.Next(0, list.Count);
    }
    
    public static GameObject_Area.ClosestObject GetClosestObject(Vector3 location, float range, LayerMask layerMask) {
        GameObject_Area.ClosestObject closestObject = new GameObject_Area.ClosestObject() {
            closest = null,
            distance = 10000
        };

        Collider2D[] colliders = Physics2D.OverlapCircleAll(location, range, layerMask);
        foreach (var collider in colliders)
        {
            float distance = (location - collider.transform.position).sqrMagnitude;
            if(distance < closestObject.distance) {
                closestObject.closest = collider;
                closestObject.distance = distance;
            }
        }

        return closestObject;
    }

    public static IObjBehaviour CreateBehaviourInstance(BehaviourInfo behaviour)
    {
        ObjBehaviourRef foundBehaviour = AssetManager.assetLibrary.GetBehaviour(behaviour.behaviourName);
        var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
        Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
        IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);
        return newBehaviour;
    }
    
    public static IObjBehaviour CreateBehaviourInstance(BehaviourInfo behaviour, GameObj gameObj)
    {
        ObjBehaviourRef foundBehaviour = AssetManager.assetLibrary.GetBehaviour(behaviour.behaviourName);
        var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
        Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
        IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);
        var behaviourHandler = new BehaviourHandler<GameObj>()
        {
            ownedThing = gameObj
        };
                

        newBehaviour.Start(foundBehaviour.linkedXmlSource, behaviourHandler.GetObjectsByRequests(foundBehaviour.parameterRequests), behaviour.customParameters?.ToArray());

        return newBehaviour;
    }

    public static void SpawnFloatingText(Vector3 position, string text) {
        var obj = PoolManager.poolManager.GetUIObjectPool("UI").ObtainSlotForType(null, position+GetRandomPosition(0.5f), 0, "Player");
        obj.GetComponent<IBootable>().BootSync();
        obj.GetComponent<ITextMeshProContact>().SetText($"{text}");
    }
    
    public static void SpawnFloatingText(Vector3 position, string text, Color color) {
        var obj = PoolManager.poolManager.GetUIObjectPool("UI").ObtainSlotForType(null, position+GetRandomPosition(0.5f), 0, "Player");
        obj.GetComponent<IBootable>().BootSync();
        ITextMeshProContact contact = obj.GetComponent<ITextMeshProContact>();
        contact.SetText($"{text}");
        contact.SetColor(color);
        
    }
    
    public static float ConvertStat(GameObj creature, string statname) {
        string statValue = creature.GetPossessed().FindStatByName(statname).Value;
        float.TryParse(statValue, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float statValueInFloat);
        return statValueInFloat;
    }

    public static string GetRandomIndex<T>(Dictionary<string, T> dict)
    {
        if (dict.Count == 0) return "null";

        System.Random random = new System.Random();
        return dict.ElementAt(random.Next(0, dict.Count)).Key;
    }

    public static int GetRandomIndex<T>(T[] list)
    {
        if (list.Length == 0) return 0;

        System.Random random = new System.Random();
        return random.Next(0, list.Length);
    }

    public static XElement ToXElement<T>(this object obj)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (TextWriter streamWriter = new StreamWriter(memoryStream))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(streamWriter, obj);
                return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }
        }
    }

    public static T FromXElement<T>(this XElement xElement)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T)xmlSerializer.Deserialize(xElement.CreateReader());
    }

    public static UnityEngine.Vector2 GetDirection(UnityEngine.Vector2 pointA, UnityEngine.Vector2 pointB)
    {
        return (UnityEngine.Vector2)(pointB - pointA).normalized;
    }

    public static float GetRotationToTargetPoint(Vector3 position, Vector3 targetPoint)
    {
        UnityEngine.Vector2 diff = position - targetPoint;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        return rot_z + 180;
    }


    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
#pragma warning disable SYSLIB0011
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;
#pragma warning restore SYSLIB0011

            return (T)formatter.Deserialize(ms);
        }
    }

    public static T GetRandomElement<T>(List<T> list) {
        int listSize = list.Count;
        return list[UnityEngine.Random.Range(0, listSize)];
    }

    public static T GetRandomElement<T>(Dictionary<string, T> list) {
        int listSize = list.Count;
        return list.ElementAt(UnityEngine.Random.Range(0, listSize)).Value;
    }


}