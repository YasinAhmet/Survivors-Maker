using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class ShockEffect : IObjBehaviour
    {
        private GameObj_Creature _ownedCreature = null;
        private ThingDef _cachedPossessed;
        private float timePassed = 0;
        private GameObj_Creature causer;
        private float strength;

        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            _ownedCreature = (GameObj_Creature)parameters[0];
            causer = _ownedCreature.lastDamagedBy;
            strength = causer.GetPossessed().GetStatValueByName("ShockPower");
            
            YKUtility.SpawnFloatingText(_ownedCreature.ownedTransform.position, "Zap!! " + (float)Math.Round(Zap(),2), Color.blue);
        }
        public void Start(XElement possess, object[] parameters){return;}

        public float Zap()
        {
            if (_ownedCreature.IsHealthDepleted()) return 0;
            float randomDamage = UnityEngine.Random.Range(strength,strength*2f);
            float resistance = _ownedCreature.GetPossessed().GetStatValueByName("Shock"+"Resistance");
            float processedDamage = randomDamage - Math.Min(randomDamage * resistance, 1);
            _ownedCreature.TryDamage(processedDamage, out HealthInfo healthInfo, causer);
            return processedDamage;
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            if (timePassed > 2) return;
            
            timePassed += deltaTime;
            Zap();
        }
        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "Shock";}
    }
}