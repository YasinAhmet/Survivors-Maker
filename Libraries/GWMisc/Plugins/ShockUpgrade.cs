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
    public class ShockUpgrade : IObjBehaviour
    {
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            GameObj_Creature owned = (GameObj_Creature)parameters[0];
            var possessed = owned.GetPossessed();
            var currentPower = possessed.GetStatValueByName("ShockPower");

            if (currentPower < 0)
            {
                possessed.ReplaceStat("ShockPower", 0.25f);
            }
            else
            {
                possessed.ReplaceStat("ShockPower", currentPower+0.15f);
            }
            
            foreach (var weapon in owned.possessedWeapons)
            {
                weapon.currentProjectileDef.ReplaceStat("DamageType", "Shock");
                
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

        public string GetName(){return "ShockUpgrade";}
    }
}