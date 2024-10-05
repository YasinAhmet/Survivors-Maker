using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class ProjectileForkingUpgrade : IObjBehaviour
    {
        private float _forkChance = 0;
        private float _forkRange = 0;
        public Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            _forkChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("ForkChance")).parameterValue, CultureInfo.InvariantCulture);
            _forkRange = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("ForkRange")).parameterValue, CultureInfo.InvariantCulture);
            GameObj_Creature owned = (GameObj_Creature)parameters[0];
            owned.onHit += OwnedOnOnHit;
            return Task.CompletedTask;
        }

        private void OwnedOnOnHit(HitResult hitResult, GameObj_Projectile projectile)
        {
            float randomChance = Random.Range(0, 1.0f);
            if (randomChance > _forkChance) return;

            GameObject hitTarget = hitResult.hitTarget;
            UnityEngine.Vector2 position = hitTarget.transform.position;
            Collider2D[] targetsToFork = Physics2D.OverlapCircleAll(position, _forkRange);

            foreach (var target in targetsToFork)
            {
                randomChance = Random.Range(0, 1.0f);
                if (randomChance > _forkChance) continue;

                float rotation = YKUtility.GetRotationToTargetPoint(hitTarget.transform, target.transform.position);
                projectile.shooter.LaunchNewProjectileCustom(projectile.GetPossessed(), position, rotation);
            }
        }

        
        public void Start(XElement possess, object[] parameters){return;}


        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "AddCompanion";}
    }
}