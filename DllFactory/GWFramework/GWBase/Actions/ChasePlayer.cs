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
                TryMove();
                break;
        }
    }

    public IEnumerator TryAttack() {
        cooldownCounter = attackCooldown;
        ownedObject.UpdateCharacterMovement(Vector2.zero);
        ownedObject.currentState = GameObj_Creature.CreatureState.OnAction;

        yield return new WaitForSeconds(attackSpeed);

        if(TargetInRange(out Vector2 direction, out float distance)) {
            objectToFollow.TryDamage(damage, out bool endedUpKilling);
            cooldownCounter = attackCooldown;
        } 

        ownedObject.currentState = GameObj_Creature.CreatureState.Idle;
        yield return this;
    }

    public void TryMove() {
        bool isInRange = TargetInRange(out Vector2 direction, out float distance);

        if(isInRange)  {
            direction = Vector2.zero;

            if(cooldownCounter <= 0) {;
                ownedObject.StartCoroutine(TryAttack());
            }
            return;
        }

        ownedObject.currentState = GameObj_Creature.CreatureState.Moving;
        ownedObject.UpdateCharacterMovement(direction);
    }

    public bool TargetInRange(out Vector2 direction, out float distance) {
        direction = YKUtility.GetDirection(ownedObject.transform.position, objectToFollow.transform.position);
        distance = Vector2.Distance(ownedObject.transform.position, objectToFollow.transform.position);
        return distance < reachDistance;
    }

        public void Tick(object[] parameters, float deltaTime){}
        public void Suspend(object[] parameters){}
        public string GetName(){ return null; }
        public ParameterRequest[] GetParameters(){return null;}

    public void Start(XElement possess, object[] parameters, ThingDef.CustomParameter[] customParameters)
    {
        Start(possess, parameters);
    }
}
