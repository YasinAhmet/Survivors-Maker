using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
namespace GWBase {

[Serializable]
public class ChasePlayer : IObjBehaviour
{

    public GameObj_Creature objectToFollow;
    public GameObj_Creature ownedObject;
    public float reachDistance = 2f;


    public virtual void InitializeVariables(GameObj_Creature creature){
        reachDistance = YKUtility.ConvertStat(creature, "ReachDistance");
    }

    public virtual void RareTick(object[] parameters, float deltaTime)
    {
        TickLogic();
    }

    public virtual void TickLogic() {
        switch(ownedObject.currentState) {
            case GameObj_Creature.CreatureState.Idle: case GameObj_Creature.CreatureState.Moving:
                var ownedPos = ownedObject.ownedTransform.position;
                var followPos = objectToFollow.ownedTransform.position;
                
                ownedObject.directionLookingAt =
                    YKUtility.GetDirection(ownedPos, followPos);
                TargetInRange(ownedPos, followPos, out Vector3 direction, out float distance);

                if(distance < reachDistance) ownedObject.RequestAction("MeleeAttack");
                else TryMove(direction);
                break;
        }
    }

    public virtual void TryMove(Vector3 direction) {
        ownedObject.currentState = GameObj_Creature.CreatureState.Moving;
        ownedObject.UpdateCharacterMovement(direction);
    }

    public bool TargetInRange(Vector3 ownedPos, Vector3 followPos, out Vector3 direction, out float distance)
    {
        direction = (followPos - ownedPos).normalized;

        // Calculate the squared distance instead of the actual distance
        float squaredDistance = (followPos - ownedPos).sqrMagnitude;
        float squaredReachDistance = reachDistance * reachDistance;

        // Return true if the squared distance is less than the squared reach distance
        bool inRange = squaredDistance < squaredReachDistance;

        // You can still output the actual distance if needed
        distance = Mathf.Sqrt(squaredDistance);
        return inRange;
    }

        public void Tick(object[] parameters, float deltaTime){}
        public void Suspend(object[] parameters){}
        public string GetName(){ return "ChasePlayer"; }
        public ParameterRequest[] GetParameters(){return null;}

    public virtual void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        ownedObject = (GameObj_Creature)parameters[0];
        objectToFollow = ((PlayerController)parameters[1]).ownedCreature;
        InitializeVariables(ownedObject);
    }
}

}