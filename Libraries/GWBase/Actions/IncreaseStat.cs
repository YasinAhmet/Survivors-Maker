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
public class IncreaseStat : IObjBehaviour
{

    public GameObj_Creature ownedObject;
    public float cooldownCounter;

    public void Start(XElement possess, object[] parameters){}

    public void RareTick(object[] parameters, float deltaTime){}


        public void Tick(object[] parameters, float deltaTime){}
        public void Suspend(object[] parameters){}
        public string GetName(){ return null; }
        public ParameterRequest[] GetParameters(){return null;}

    public virtual void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        ownedObject = (GameObj_Creature)parameters[0];
        string targetStatName = customParameters.FirstOrDefault(x => x.parameterName.Equals("StatName")).parameterValue;
        float targetStatIncrease = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("BonusRate")).parameterValue, CultureInfo.InvariantCulture);
        targetStatIncrease = TypeSpecialAction(targetStatName, targetStatIncrease);
        
        ThingDef possessed = ownedObject.GetPossessed();
        float newValue = float.Parse(possessed.FindStatByName(targetStatName).Value, CultureInfo.InvariantCulture) + targetStatIncrease;
        
        possessed.ReplaceStat(targetStatName, newValue);
    }

    public float TypeSpecialAction(string type, float value)
    {
        if (type == "MaxSpeed")
        {
            value *= ownedObject.GetPossessed().mass;
            ownedObject.cachedMovementSpeed += value;
        }

        return value;
    }
}

}