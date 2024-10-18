using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace GWBase
{

    public class GameObj_Shooter : GameObj
    {
        public delegate void OnProjectileHit(HitResult hitResult);
        public event OnProjectileHit onProjectileHit;
        [SerializeField] private GameObject_Area rangeArea = null;
        [SerializeField] private ThingDef currentProjectileDef;
        [SerializeField] private float attackPerSecond = 1.1f;
        private float AttackCooldown
        {
            get
            {
                return 1 / attackPerSecond;
            }
        }
        [SerializeField] private float range = 1f;
        [SerializeField] private float cooldownCounter = 0;
        [SerializeField] private float rotationUpdateTime = 0.2f;
        [SerializeField] private float rotationUpdateCounter = 0;
        [SerializeField] private float AngleOffset = 0;
        [SerializeField] private float quaternionToFlip = 0.7f;
        public GameObject target;
        public Stat[] stats = { };

        public override void Possess<GameObj_Shooter>(ThingDef entity, string faction)
        {
            base.Possess<GameObj_Shooter>(entity, faction);          
            gameObject.layer = LayerMask.NameToLayer(faction+"Projectile");
            SetupRangeArea(entity);

            attackPerSecond = float.Parse(entity.FindStatByName("AttackPerSecond").Value, CultureInfo.InvariantCulture);
            var projectileName = entity.FindStatByName("Action");
            var projectileDef = AssetManager.assetLibrary.actionsDictionary.FirstOrDefault(x => x.Key.Equals(projectileName.Value)).Value;
            AttachNewProjectileDef(projectileDef);
        }

        public void SetupRangeArea(ThingDef entity)
        {
            if (rangeArea == null) return;

            range = float.Parse(entity.FindStatByName("Range").Value);
            rangeArea.faction = faction;          
            rangeArea.gameObject.layer = LayerMask.NameToLayer(faction+"Projectile");
            rangeArea.transform.localScale = new Vector3(range, range, 1);
            rangeArea.gameObject.SetActive(true);
        }

        public void AttachNewProjectileDef(ThingDef projectileDef)
        {
            currentProjectileDef = projectileDef;
        }

        public bool TryLaunchNewProjectile(ThingDef type)
        {
            if (target != null)
            {
                LaunchNewProjectile(type);
                return true;
            }
            return false;
        }

        public void LookAtTarget(Transform target)
        {
            var newRotation =  Quaternion.Euler(new Vector3(0, 0, YKUtility.GetRotationToTargetPoint(ownedTransform.position, target.position)));
            ownedTransform.rotation = newRotation;

            if (ownedTransform.rotation.z > quaternionToFlip)
            {
                ownedSpriteRenderer.flipY = true;
            }
            else
            {
                ownedSpriteRenderer.flipY = false;
            }
        }

        public GameObj_Projectile LaunchNewProjectile(ThingDef type)
        {
            GameObj_Projectile slot = (GameObj_Projectile)PoolManager.poolManager.GetObjectPool("Projectiles").ObtainSlotForType(type, transform.position, transform.eulerAngles.z, faction+"Projectile");
            slot.transform.position = transform.position;
            slot.shooter = this;
            slot.stats = stats;
            slot.hitEvent.AddListener(OwnedProjectileHit);
            return slot;
        }
        
        public GameObj_Projectile LaunchNewProjectileCustom(ThingDef type, Vector2 position, float rotation)
        {
            GameObj_Projectile slot = (GameObj_Projectile)PoolManager.poolManager.GetObjectPool("Projectiles").ObtainSlotForType(type, position, rotation, faction+"Projectile");
            slot.shooter = this;
            slot.stats = stats;
            slot.hitEvent.AddListener(OwnedProjectileHit);
            return slot;
        }

        public void OwnedProjectileHit(HitResult result) {
            onProjectileHit?.Invoke(result);
            var projectile = (GameObj_Projectile)result.hitSource;
            projectile.hitEvent.RemoveListener(OwnedProjectileHit);
        }


        public override void FixedUpdate()
        {
            cooldownCounter += Time.fixedDeltaTime;
            rotationUpdateCounter += Time.fixedDeltaTime;

            if (rotationUpdateCounter > rotationUpdateTime)
            {
                rotationUpdateCounter = 0;
                target = rangeArea.GetClosestObject().closest?.gameObject;
                if (target != null) LookAtTarget(target.transform);
            }

            if (cooldownCounter >= AttackCooldown)
            {
                cooldownCounter = 0;
                TryLaunchNewProjectile(currentProjectileDef);
            }

            foreach (var behaviour in installedBehaviours)
            {
                behaviour?.Tick(null, Time.fixedDeltaTime);
            }
        }

    }

}