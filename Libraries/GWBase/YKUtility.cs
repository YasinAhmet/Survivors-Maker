using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

public static class YKUtility
{
    public static int GetRandomIndex<T>(List<T> list)
    {
        if (list.Count == 0) return 0;

        System.Random random = new System.Random();
        return random.Next(0, list.Count);
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

    public static float GetRotationToTargetPoint(Transform ownedTransform, UnityEngine.Vector2 targetPoint)
    {
        UnityEngine.Vector2 diff = (UnityEngine.Vector2)ownedTransform.position - targetPoint;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        return rot_z;
    }


    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

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