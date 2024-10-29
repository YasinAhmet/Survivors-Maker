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
    string[] randomStatPool = {"Damage", "MaxSpeed", "Health", "MaxHealth"};
    public override void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        float targetStatIncrease = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("BonusRate")).parameterValue, CultureInfo.InvariantCulture);
        CustomParameter parameter = new CustomParameter() {
            parameterName = GetRandomStat(),
            parameterValue = targetStatIncrease.ToString(CultureInfo.InvariantCulture)
        };

        CustomParameter[] customRandomParameters = {parameter};

        base.Start(possess, parameters, customRandomParameters);
    }

    public string GetRandomStat() {
        return randomStatPool[UnityEngine.Random.Range(0, randomStatPool.Length)];
    }
}

}