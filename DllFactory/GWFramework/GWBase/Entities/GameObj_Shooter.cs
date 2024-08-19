using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GameObj_Shooter : GameObj
{
    public delegate void OnProjectileHit(HitResult hitResult);
    public event OnProjectileHit onProjectileHit;
    [SerializeField] private GameObject_Area rangeArea;
    [SerializeField] private ThingDef currentProjectileDef;
    [SerializeField] private float attackPerSecond = 1.1f;
    private float AttackCooldown{
        get {
            return 1/attackPerSecond;
        }
    }
    [SerializeField] private float range = 1f;
    [SerializeField] private float cooldownCounter = 0;
    [SerializeField] private float AngleOffset = 0;

    private BehaviourHandler<GameObj_Creature> behaviourHandler = null;

    public override void Possess<GameObj_Shooter>(ThingDef entity, string faction)
    {
        base.Possess<GameObj_Shooter>(entity, faction);
        SetupRangeArea(entity);

        attackPerSecond = float.Parse(entity.FindStatByName("AttackPerSecond").Value, CultureInfo.InvariantCulture);
        var projectileName = entity.FindStatByName("Action");
        var projectileDef = AssetManager.assetLibrary.actionsDictionary.FirstOrDefault(x => x.Key.Equals(projectileName.Value)).Value;
        AttachNewProjectileDef(projectileDef);
    }

    public void SetupRangeArea(ThingDef entity) {
        if(rangeArea == null) return;

        range = float.Parse(entity.FindStatByName("Range").Value);
        rangeArea.faction = faction;
        rangeArea.gameObject.layer = gameObject.layer;
        rangeArea.transform.localScale = new Vector3(range, range, 1);
        rangeArea.gameObject.SetActive(true);
    }

    public void AttachNewProjectileDef(ThingDef projectileDef)
    {
        currentProjectileDef = projectileDef;
    }

    public bool TryLaunchNewProjectile(ThingDef type) {
        var closestObject = rangeArea.GetClosestObject();
        if(closestObject.closest != null) {
            var newRotation = YKUtility.GetRotationToTargetPoint(transform, closestObject.closest.transform.position);
            transform.eulerAngles = new Vector3(0,0,newRotation + AngleOffset);
            StartCoroutine(LaunchNewProjectile(type));
            return true;
        }
        return false;
    }

    public IEnumerator LaunchNewProjectile(ThingDef type) {
        GameObj_Projectile slot = (GameObj_Projectile)PoolManager.poolManager.projectilesPool.ObtainSlotForType(type, transform.position, transform.eulerAngles.z, faction);
        yield return StartCoroutine(slot.WaitProjectileResult());
        HitResult result = slot.foundResult;
        if(result.hitTarget != null) onProjectileHit?.Invoke(result); 
    }


    public override void Update()
    {
        base.Update();
        cooldownCounter += Time.deltaTime;
        if(cooldownCounter >= AttackCooldown) {
            cooldownCounter = 0;
            TryLaunchNewProjectile(currentProjectileDef);
        }
    }

}

