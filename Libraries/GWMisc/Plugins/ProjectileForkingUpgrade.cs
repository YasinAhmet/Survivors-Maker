using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using ProjectilesUtility;
using UnityEngine;

namespace GWMisc
{
    public class ProjectileForkingUpgrade : IObjBehaviour
    {
        private GameObj_Creature owned;
        private float _forkChance = 0;
        private float _forkRange = 0;
        private float _forkChancePerUpgrade = 0;
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            _forkChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("ForkChance")).parameterValue, CultureInfo.InvariantCulture);
            _forkRange = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("ForkRange")).parameterValue, CultureInfo.InvariantCulture);
            
            owned = (GameObj_Creature)parameters[0];
            bool hasUpgradeAlready = owned.DoesHaveBehaviour("ProjectileForkingUpgrade", out IObjBehaviour behaviour);

            if (hasUpgradeAlready)
            {
                ProjectileForkingUpgrade alreadyExistantUpgrade = (ProjectileForkingUpgrade)behaviour;   
                alreadyExistantUpgrade._forkChancePerUpgrade += float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("ForkChancePerUpgrade")).parameterValue, CultureInfo.InvariantCulture);
                owned.GetPossessed().ReplaceStat("ForkChance", _forkChance);
            }
            else
            {
                owned.onHit += OwnedOnOnHit;
                owned.AddBehaviour(this);
                owned.GetPossessed().ReplaceStat("ForkChance", owned.GetPossessed().GetStatValueByName("ForkChance") + _forkChance);
            }
        }

        private void OwnedOnOnHit(HitResult hitResult)
        {
            var projectile = (GameObj_Projectile)hitResult.hitSource;
            if(projectile.GetPossessed().GetStatValueByName("ChainAmount") <= 0) return;
            
            float randomChance = Random.Range(0, 1.0f);
            if (randomChance > _forkChance+_forkChancePerUpgrade) return;

            GameObj hitTarget = hitResult.hitTarget;
            UnityEngine.Vector2 position = hitTarget.ownedTransform.position;
            
            ProjectileBehaviourLibrary.ChainProjectileAllByChance(projectile, _forkRange, _forkChance);
        }

        
        public void Start(XElement possess, object[] parameters){return;}


        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void Suspend(object[] parameters)
        {
            owned.onHit -= OwnedOnOnHit;
            return;
        }

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "ProjectileForkingUpgrade";}
    }
}