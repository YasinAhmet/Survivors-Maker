using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
namespace GWBase {

[Serializable]
public class RandomStatIncrease : IncreaseStat
{
    string[] randomStatPool = {"Damage", "MovementSpeed", "Health", "MaxHealth"};
    public override async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        CustomParameter parameter = new CustomParameter() {
            parameterName = GetRandomStat()
        };

        CustomParameter[] customRandomParameters = {parameter};

        base.Start(possess, parameters, customRandomParameters);
        
    }

    public string GetRandomStat() {
        return randomStatPool[UnityEngine.Random.Range(0, randomStatPool.Length)];
    }
}

}