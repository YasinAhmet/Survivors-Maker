using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace GWBase {

public class GameObj_Projectile : GameObj
{
    public TriggerContactEvent onHit;
    public HitResult foundResult;
    public Stat[] stats;

    private BehaviourHandler<GameObj_Projectile> behaviourHandler = null;

    public void Start(){
        behaviourHandler = new BehaviourHandler<GameObj_Projectile>(){
            ownedThing = this
        };

    }

    public override void Possess<T>(ThingDef entity, string faction)
    {
        onHit.RemoveAllListeners();
        base.Possess<T>(entity, faction);
    }

    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer != LayerMask.NameToLayer(faction)) {
            onHit?.Invoke(other);
        }
    }

    public void ProcessHit(HitResult hit) {
        foundResult = hit;
    }

    public bool ResultFound() {return foundResult.hitTarget == null || !gameObject.activeSelf;}

    public IEnumerator WaitProjectileResult() {
        yield return new WaitUntil(ResultFound);
    }
}

[System.Serializable]
public class TriggerContactEvent : UnityEvent<Collider2D>{}

public struct HitResult {
   public float damage;
   public bool killed;
   public GameObject hitTarget;
   public Vector3 hitPosition;

}
}