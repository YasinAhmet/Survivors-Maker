using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using static LightObjectPool;

[Serializable]
public class FireAttack : IObjBehaviour
{

    public GameObj_Projectile ownedProjectile;
    public float damage = 0;
    public float damageVariety = 1;
    public float movementSpeed = 0;
    private float lifetime = 0;
    private float hitlifetime = 0;

    private float lifetimeCounter = 0;

    public string GetName(){return null;}
    public ParameterRequest[] GetParameters(){return null;}
    public void RareTick(object[] parameters, float deltaTime){
    }
    public void Suspend(object[] parameters){}

    public void Start(XElement possess, object[] parameters)
    {
        lifetimeCounter = 0;
        ownedProjectile = (GameObj_Projectile)parameters[0];
        movementSpeed = float.Parse(ownedProjectile.GetPossessed().FindStatByName("MovementSpeed").Value, CultureInfo.InvariantCulture);
        damage = float.Parse(ownedProjectile.GetPossessed().FindStatByName("Damage").Value, CultureInfo.InvariantCulture);
        damageVariety = float.Parse(ownedProjectile.GetPossessed().FindStatByName("DamageVariety").Value, CultureInfo.InvariantCulture);
        lifetime = float.Parse(ownedProjectile.GetPossessed().FindStatByName("Lifetime").Value, CultureInfo.InvariantCulture);
        hitlifetime = float.Parse(ownedProjectile.GetPossessed().FindStatByName("HitLifetime").Value, CultureInfo.InvariantCulture);
        ownedProjectile.movementSpeed = movementSpeed;
        ownedProjectile.onHit.AddListener(HitHostileObject);
    }

    public void Tick(object[] parameters, float deltaTime){
        ownedProjectile.MoveObject(ownedProjectile.transform.right, lifetime);
        lifetimeCounter += deltaTime;

        if(lifetimeCounter > lifetime) {
            ownedProjectile.gameObject.SetActive(false);
        }
    }

    public void HitHostileObject(Collider2D collider) {
        if(!ownedProjectile.gameObject.activeSelf) return;
        ownedProjectile.gameObject.SetActive(false);

         var closestPoint = collider.ClosestPoint(ownedProjectile.GetComponent<Collider2D>().bounds.center);
        PoolManager.poolManager.effectsPool.ObtainSlotForType(null, closestPoint, ownedProjectile.transform.eulerAngles.z, ownedProjectile.faction, hitlifetime);
        

        if(collider.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            float randomDamage = (float)Math.Floor(UnityEngine.Random.Range(damage, damage*damageVariety));
            bool didHit = damageable.TryDamage(randomDamage, out bool didKill);

            if(didHit) {
                HitResult newHit = new(){
                    hitTarget = collider.gameObject,
                    damage = randomDamage,
                    killed = didKill,
                    hitPosition = closestPoint
                };
                ownedProjectile.ProcessHit(newHit);
                SpawnFloatingText(closestPoint, randomDamage);
            }
        }
    }

    public void SpawnFloatingText(Vector3 position, float damage) {
        var obj = PoolManager.poolManager.floatingTextPool.ObtainSlotForType(null, position, 0, ownedProjectile.faction);
        obj.GetComponent<IBootable>().BootSync();
        obj.GetComponent<ITextMeshProContact>().SetText($"{damage}");
    }
}
