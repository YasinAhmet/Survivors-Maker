using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class IncreaseStat : IObjBehaviour
{

    public GameObj_Creature ownedObject;

    private float rareTickCounter = 0;
    public float cooldownCounter;

    public void Start(XElement possess, object[] parameters){}

    public void RareTick(object[] parameters, float deltaTime){}


        public void Tick(object[] parameters, float deltaTime){}
        public void Suspend(object[] parameters){}
        public string GetName(){ return null; }
        public ParameterRequest[] GetParameters(){return null;}

    public void Start(XElement possess, object[] parameters, ThingDef.CustomParameter[] customParameters)
    {
        ownedObject = (GameObj_Creature)parameters[0];
        Debug.Log($"[STAT INCREASE] Stat Increase setup..");
        string targetStatName = customParameters.FirstOrDefault(x => x.parameterName.Equals("StatName")).parameterValue;
        float targetStatIncrease = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("BonusRate")).parameterValue, CultureInfo.InvariantCulture);
        ThingDef possessed = ownedObject.GetPossessed();
        float newValue = float.Parse(possessed.FindStatByName(targetStatName).Value, CultureInfo.InvariantCulture) * targetStatIncrease;
        possessed.ReplaceStat(targetStatName, newValue);
        Debug.Log($"[STAT INCREASE] Stat Increase over.. {newValue} {targetStatIncrease}");
        
    }
}
