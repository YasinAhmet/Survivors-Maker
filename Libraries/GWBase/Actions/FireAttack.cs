using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
namespace GWBase {

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
    public void RareTick(object[] parameters, float deltaTime)
    {
        lifetimeCounter += deltaTime;

        if(lifetimeCounter > lifetime) {
            ownedProjectile.CallActivationChange(false);
        }
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
        ownedProjectile.lastMovementVector = ownedProjectile.ownedTransform.right;
        return;
    }

    public void Tick(object[] parameters, float deltaTime){
        lifetimeCounter += deltaTime;

        if(lifetimeCounter > lifetime) {
            ownedProjectile.CallActivationChange(false);
        }
    }

    public void HitHostileObject(Collider2D collider) {
        if(!ownedProjectile.gameObject.activeSelf) return;
        if(!collider.TryGetComponent(out GameObj gameObj)) return;
        
        var piercingLevel = possessed.GetStatValueByName("Piercing");
        if (piercingLevel <= 0)
        {
            ownedProjectile.CallActivationChange(false);
        }
        else
        {
            possessed.ReplaceStat("Piercing", piercingLevel - 1);
        }
        

        var closestPoint = gameObj.ownedTransform.position;
        PoolManager.poolManager.GetLightObjectPool("Effects").ObtainSlotForType(closestPoint, ownedProjectile.transform.eulerAngles.z, hitlifetime);
        float totalDamage = float.Parse(ownedProjectile.stats.FirstOrDefault(x => x.Name.Equals("Damage")).Value, CultureInfo.InvariantCulture) + possessed.GetStatValueByName("Damage");

            float randomDamage = (float)Math.Floor(UnityEngine.Random.Range(totalDamage, totalDamage*possessed.GetStatValueByName("DamageVariety")));
            var damageType = possessed.FindStatByName("DamageType").Value;
            float resistance = gameObj.GetPossessed().GetStatValueByName(damageType+"Resistance");
            float processedDamage = randomDamage - Math.Min(randomDamage * resistance, 1);
            bool didHit = gameObj.TryDamage(processedDamage, out HealthInfo healthInfo);

            if(didHit) {
                HitResult newHit = new(){
                    hitTarget = gameObj,
                    damage = randomDamage,
                    killed = healthInfo.gotKilled,
                    hitPosition = closestPoint,
                    hitSource = ownedProjectile
                };
                ownedProjectile.ProcessHit(newHit);
                SpawnFloatingText(closestPoint, healthInfo.damageTaken);
            }
        
    }

    public void SpawnFloatingText(Vector3 position, float damage) {
        var obj = PoolManager.poolManager.GetUIObjectPool("UI").ObtainSlotForType(null, position, 0, ownedProjectile.faction);
        obj.GetComponent<IBootable>().BootSync();
        obj.GetComponent<ITextMeshProContact>().SetText($"{damage}");
    }

    public Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
    {
        Start(possess, parameters);
        return Task.CompletedTask;
    }
}

}