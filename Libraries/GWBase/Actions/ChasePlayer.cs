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
using Unity.Collections;
using Unity.Jobs;
namespace GWBase {

[Serializable]
public class ChasePlayer : IObjBehaviour
{

    public GameObj_Creature objectToFollow;
    public GameObj_Creature ownedObject;

    private float rareTickCounter = 0;

    public float reachDistance = 2f;
    public float attackSpeed = 1f;
    public float damage = 1f;
    public float cooldownCounter;
    public float attackCooldown;

    public void Start(XElement possess, object[] parameters)
    {
        ownedObject = (GameObj_Creature)parameters[0];
        objectToFollow = ((PlayerController)parameters[1]).ownedCreature;
        InitializeVariables(ownedObject);
        

        Debug.Log($"[CHASE] Chase Behaviour setup.. {ownedObject} {objectToFollow} validity: {ownedObject != null} {objectToFollow != null}");
    }

    public void InitializeVariables(GameObj_Creature creature){
        reachDistance = ConvertStat(creature, "ReachDistance");
        attackSpeed = ConvertStat(creature, "AttackSpeed");
        damage = ConvertStat(creature, "Damage");
        attackCooldown = ConvertStat(creature, "AttackCooldown");
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
                TargetInRange(ownedObject.ownedTransform.position, objectToFollow.ownedTransform.position, out Vector3 direction, out float distance);

                if(distance < reachDistance && cooldownCounter <= 0) TryAttack();
                else TryMove(direction);
                break;
        }
    }
 
    public void TryAttack() {
        cooldownCounter = attackCooldown;
        ownedObject.UpdateCharacterMovement(Vector2.zero);
        ownedObject.currentState = GameObj_Creature.CreatureState.OnAction;

        objectToFollow.TryDamage(damage, out bool endedUpKilling);
        cooldownCounter = attackCooldown;

        ownedObject.currentState = GameObj_Creature.CreatureState.Idle;
    }

    public void TryMove(Vector3 direction) {
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
        public string GetName(){ return null; }
        public ParameterRequest[] GetParameters(){return null;}

    public async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        Start(possess, parameters);
    }
}

}