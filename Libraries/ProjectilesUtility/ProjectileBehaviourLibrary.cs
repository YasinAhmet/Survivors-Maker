using GWBase;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectilesUtility;

public class ProjectileBehaviourLibrary
{
    public static void ChainProjectileAll(GameObj_Projectile projectile, float range)
    {
        var projectilePos = projectile.ownedTransform.position;
        var mask = AssetManager._masksByLayer[projectile.gameObject.layer];
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(projectilePos, range, mask);
        foreach (var collider in colliders)
        {
            foreach (var hit in projectile.objectsHit)
            {
                if(collider.gameObject == hit) goto skip;
            }
        
            var targetTransform = collider.transform;
            float rotation = YKUtility.GetRotationToTargetPoint(projectile.ownedTransform.position, targetTransform.position);
            var spawnedProjectile = projectile.shooter.LaunchNewProjectileCustom(projectile.GetPossessed(), projectilePos, rotation);
            spawnedProjectile.objectsHit = projectile.objectsHit;
            skip:
            continue;
        }
        
        projectile.GetPossessed().ReplaceStat("ChainAmount", projectile.GetPossessed().GetStatValueByName("ChainAmount")-1);
    }
    
    public static void ChainProjectileAllByChance(GameObj_Projectile projectile, float range, float chance)
    {
        var projectilePos = projectile.ownedTransform.position;
        var mask = AssetManager._masksByLayer[projectile.gameObject.layer];
        
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(projectilePos, range, mask);
        foreach (var collider in colliders)
        {
            foreach (var hit in projectile.objectsHit)
            {
                if(collider.gameObject == hit) goto skip;
            }
            
            var randomChance = Random.Range(0, 1.0f);
            if (randomChance > chance) continue;
            
            var targetTransform = collider.transform;
            float rotation = YKUtility.GetRotationToTargetPoint(projectile.ownedTransform.position, targetTransform.position);
            var spawnedProjectile = projectile.shooter.LaunchNewProjectileCustom(projectile.GetPossessed(), projectilePos, rotation);
            spawnedProjectile.objectsHit = projectile.objectsHit;
            
            skip:
            continue;
        }
        
        projectile.GetPossessed().ReplaceStat("ChainAmount", projectile.GetPossessed().GetStatValueByName("ChainAmount")-1);
    }
    
    public static void ChainProjectile(GameObj_Projectile projectile, float range)
    {
        var mask = AssetManager._masksByLayer[projectile.gameObject.layer];
        var projectilePos = projectile.ownedTransform.position;
        GameObject_Area.ClosestObject closestTarget = YKUtility.GetClosestObject(projectilePos,
            range, mask);
        var targetTransform = closestTarget.closest.transform;
        
        
        foreach (var hit in projectile.objectsHit)
        {
            if(!closestTarget.closest || closestTarget.closest.gameObject == hit) goto skip;
        }

        float rotation = YKUtility.GetRotationToTargetPoint(projectile.ownedTransform.position, targetTransform.position);
        projectile.GetPossessed().ReplaceStat("ChainAmount", projectile.GetPossessed().GetStatValueByName("ChainAmount")-1);
        var spawnedProjectile = projectile.shooter.LaunchNewProjectileCustom(projectile.GetPossessed(), projectilePos, rotation);
        spawnedProjectile.objectsHit = projectile.objectsHit;
        skip:
        return;
    }

    public static void TryChainProjectile(GameObj_Projectile projectile, float range)
    {
        if (projectile.GetPossessed().GetStatValueByName("ChainAmount") > 0)
        {
            ChainProjectile(projectile, range);
        }
    }
}