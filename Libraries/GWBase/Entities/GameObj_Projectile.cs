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
    public List<GameObject> objectsHit = new List<GameObject>();
    public bool canHitSameTarget = false;

    private BehaviourHandler<GameObj_Projectile> behaviourHandler = null;

    public void Start(){
        behaviourHandler = new BehaviourHandler<GameObj_Projectile>(){
            ownedThing = this
        };

    }

    public override void MoveObject(Vector2 axis, float delta, bool passMax)
    {
        var direction = new Vector3(axis.x * (cachedMovementSpeed * delta), axis.y * (cachedMovementSpeed * delta));
        
        
        RaycastHit2D result = Physics2D.Raycast(ownedTransform.position, direction, direction.sqrMagnitude, mask);
        if (result && result.collider)
        {
            OnHit(result.collider);
        }
        
        ownedTransform.position += direction;

        
    }

    public int mask = 0;
    public override void Possess<T>(ThingDef entity, string faction)
    {
        onHit.RemoveAllListeners();
        base.Possess<T>(entity, faction);
        gameObject.layer = LayerMask.NameToLayer(faction);
        mask = AssetManager._masksByLayer[selfObject.layer];
        objectsHit = new List<GameObject>();
    }

    private void OnHit(Collider2D col)
    { 
        if (!col.TryGetComponent<IDamageable>(out IDamageable damageable) || damageable.GetTeam() == faction) return;
        
        if (!canHitSameTarget)
        {
            var ownedGM = col.gameObject;
            foreach (var creature in objectsHit)
            {
                if (ownedGM == creature)
                {
                    return;
                }
            }
            objectsHit.Add(ownedGM);
        }
        
        onHit?.Invoke(col);
    }


    public void ProcessHit(HitResult hit) {
        foundResult = hit;
        hitEvent?.Invoke(hit);
    }
}

[System.Serializable]
public class TriggerContactEvent : UnityEvent<Collider2D>{}

[System.Serializable]
public class HitEvent : UnityEvent<HitResult>{}

public struct HitResult {
   public float damage;
   public bool killed;
   public GameObj hitTarget;
   public Vector3 hitPosition;
   public GameObj hitSource;

}
}