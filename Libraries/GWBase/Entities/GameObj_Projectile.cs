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
    public GameObj_Shooter shooter;
    public HitEvent hitEvent;
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
        gameObject.layer = LayerMask.NameToLayer(faction);
    }

    private void OnTriggerEnter2D(Collider2D col)
    { 
        if (col.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if (damageable.GetTeam() != faction)
            {
                onHit?.Invoke(col);
            }
        }
    }

    public void ProcessHit(HitResult hit) {
        foundResult = hit;
        hitEvent?.Invoke(this, hit);
    }
}

[System.Serializable]
public class TriggerContactEvent : UnityEvent<Collider2D>{}

[System.Serializable]
public class HitEvent : UnityEvent<GameObj_Projectile, HitResult>{}

public struct HitResult {
   public float damage;
   public bool killed;
   public GameObject hitTarget;
   public Vector3 hitPosition;

}
}