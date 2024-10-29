using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class DamageTypeModule : IObjBehaviour
    {
        public static DamageTypeRef[] DamageTypes;

        public void HitReaction(HitResult hitResult)
        {
            var damageTypeStat = hitResult.hitSource.GetPossessed().FindStatByName("DamageType");

            foreach (var damageType in DamageTypes)
            {
                if(damageType.name != damageTypeStat.Value) continue;
                foreach (var hitBehaviour in damageType.onHit)
                {
                    var instantiatedBehaviour = YKUtility.CreateBehaviourInstance(hitBehaviour, hitResult.hitTarget);
                    hitResult.hitTarget.AddBehaviour(instantiatedBehaviour);
                }
            }
        }

        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            ReloadDamageTypes();
            var pool = PoolManager.poolManager.GetObjectPool("Projectiles");
            SubscribeToDamageCallbacks(pool);
            pool.newObjectsInitiated += SubscribeToDamageCallbacks;
        }
        public void Start(XElement possess, object[] parameters) { }
        public void Tick(object[] parameters, float deltaTime) { }
        public void RareTick(object[] parameters, float deltaTime) { }
        public void Suspend(object[] parameters) { }
        public string GetName() { return "DamageTypeModule";}
        public ParameterRequest[] GetParameters() { return null; }
        
        public void ReloadDamageTypes()
        {
            var pathToDamageTypes = AssetManager.assetLibrary.fullPathToGameDatabase + "Misc/BaseDamageTypes.xml";
            var defFile = XElement.Load(pathToDamageTypes);
            XElement[] damageTypes = defFile.Elements("DamageType").Where(x => x.Attribute("abstract") == null).ToArray();
            var damageTypeAmount = damageTypes.Count();
            DamageTypes = new DamageTypeRef[damageTypeAmount];

            for (int i = 0; i < damageTypeAmount; i++)
            {
                DamageTypes[i] = damageTypes[i].FromXElement<DamageTypeRef>();
            }
        }
        
        public void SubscribeToDamageCallbacks(ObjectPool pool)
        {
            foreach (var obj in pool.pooledObjects)
            {
                var projectile = (GameObj_Projectile)obj;
                projectile.hitEvent.AddListener(HitReaction);
            }
        }

        public void SubscribeToDamageCallbacks(List<GameObj> specificObjects)
        {           
            foreach (var obj in specificObjects)
            {
                var projectile = (GameObj_Projectile)obj;
                projectile.hitEvent.AddListener(HitReaction);
            }
        }
    }

    [Serializable]
    [XmlRoot("DamageType")]
    public struct DamageTypeRef
    {
        [XmlAttribute("Name")] public string name;
        [XmlArray("behaviours")] [XmlArrayItem("behaviour")]
        public BehaviourInfo[] onHit;
    }
}