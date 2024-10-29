using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class AddCompanionUpgrade : IObjBehaviour
    {
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            GameObj_Creature owned = (GameObj_Creature)parameters[0];
            CreatureGroup group = owned.groupAttached.group;

            var CompanionID = customParameters.FirstOrDefault(x => x.parameterName.Equals("CompanionID")).parameterValue;
            var characterDefToAdd =
                AssetManager.assetLibrary.initializedThingDefsDictionary.FirstOrDefault(x => x.Key == CompanionID).Value;
            GameObj_Creature creature = (GameObj_Creature)PoolManager.poolManager.GetObjectPool("Creatures").ObtainSlotForType(characterDefToAdd, UnityEngine.Vector2.zero, 0, group.groupFaction);
            group.AttachCreature(creature);
            creature.transform.localPosition = new UnityEngine.Vector3(group.lastUsedPosition.x,group.lastUsedPosition.y, 0);
        }

        
        public void Start(XElement possess, object[] parameters){return;}


        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "AddCompanionUpgrade";}
    }
}