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
    private float lifetime = 0;
    private float hitlifetime = 0;

    private float lifetimeCounter = 0;
    private ThingDef possessed;

    public string GetName(){return null;}
    public ParameterRequest[] GetParameters(){return null;}
    public void RareTick(object[] parameters, float deltaTime){
    }
    public void Suspend(object[] parameters){}

    public void Start(XElement possess, object[] parameters)
    {
        lifetimeCounter = 0;
        ownedProjectile = (GameObj_Projectile)parameters[0];
        possessed = ownedProjectile.GetPossessed();
        lifetime = float.Parse(ownedProjectile.GetPossessed().FindStatByName("Lifetime").Value, CultureInfo.InvariantCulture);
        hitlifetime = float.Parse(ownedProjectile.GetPossessed().FindStatByName("HitLifetime").Value, CultureInfo.InvariantCulture);
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
        float totalDamage = float.Parse(ownedProjectile.stats.FirstOrDefault(x => x.Name.Equals("Damage")).Value, CultureInfo.InvariantCulture) + possessed.GetStatValueByName("Damage");

        if(collider.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            float randomDamage = (float)Math.Floor(UnityEngine.Random.Range(totalDamage, totalDamage*possessed.GetStatValueByName("DamageVariety")));
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

    public void Start(XElement possess, object[] parameters, ThingDef.CustomParameter[] customParameters)
    {
        Start(possess, parameters);
    }
}
