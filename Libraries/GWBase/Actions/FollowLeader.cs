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
public class FollowLeader : IObjBehaviour
{

    public GameObj_Creature objectToFollow;
    public GameObj_Creature ownedObject;
    
    public float reachDistance = 2f;
    public float cooldownCounter;

    public void InitializeVariables(GameObj_Creature creature){
        reachDistance = ConvertStat(creature, "ReachDistance");
    }
    
    public float ConvertStat(GameObj_Creature creature, string statname) {
        string statValue = creature.GetPossessed().FindStatByName(statname).Value;
        float.TryParse(statValue, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float statValueInFloat);
        return statValueInFloat;
    }

    public void RareTick(object[] parameters, float deltaTime)
    {
        cooldownCounter -= deltaTime;
        TickLogic();
    }

    public void TickLogic() {
        switch(ownedObject.currentState) {
            case GameObj_Creature.CreatureState.Idle: case GameObj_Creature.CreatureState.Moving:
                TryMove();
                break;
        }
    }

    public void TryMove() {
        bool isInRange = TargetInRange(out Vector2 direction, out float distance);

        if(isInRange)  {
            direction = Vector2.zero;
            return;
        }

        ownedObject.currentState = GameObj_Creature.CreatureState.Moving;
        ownedObject.UpdateCharacterMovement(direction);
    }

    public bool TargetInRange(out Vector2 direction, out float distance) {
        direction = YKUtility.GetDirection(((Component)ownedObject).transform.position, ((Component)objectToFollow).transform.position);
        distance = Vector2.Distance(((Component)ownedObject).transform.position, ((Component)objectToFollow).transform.position);
        return distance < reachDistance;
    }

        public void Tick(object[] parameters, float deltaTime){}
        public void Suspend(object[] parameters){}
        public string GetName(){ return null; }
        public ParameterRequest[] GetParameters(){return null;}

    public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    { 
        ownedObject = (GameObj_Creature)parameters[0];
        InitializeVariables(ownedObject);
    }
}

}