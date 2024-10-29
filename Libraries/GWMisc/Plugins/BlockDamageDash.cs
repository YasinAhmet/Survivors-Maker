using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class BlockDamageDash : IObjBehaviour
    {
        public GameObj_Creature ownedObject;
        public float blockChance;
        public float blockDmgRate;
        
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            blockChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("BlockChance")).parameterValue, CultureInfo.InvariantCulture);
            blockDmgRate = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("BlockPower")).parameterValue, CultureInfo.InvariantCulture);
            ownedObject = (GameObj_Creature)parameters[0];
            ownedObject.pre_onHealthChange += TryBlock;
        }

        public void TryBlock(HealthInfo healthInfo)
        {
            if (healthInfo.damageTaken > 0)
            {
                float random = Random.Range(0f, 1f);
                if (random > blockChance) return;

                float returnAmount = healthInfo.damageTaken * blockDmgRate;
                ownedObject.lastHealthInfo.damageTaken -= returnAmount;
                ownedObject.GetPossessed().AddToStat("Health", returnAmount);
                ownedObject.lastHealthInfo.currentHealth += returnAmount;
                
                ownedObject.StartColorChange(Color.yellow);
                ownedObject.RequestAction("Dash");
            }
        }

        
        public void Start(XElement possess, object[] parameters){return;}


        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void Suspend(object[] parameters)
        {
            ownedObject.pre_onHealthChange -= TryBlock;
            return;
        }

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "BlockDamageDash";}
        
    }
}