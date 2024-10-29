using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;

namespace GWMisc
{
    public class IncreasePiercingAmount : IObjBehaviour
    {
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            GameObj_Creature owned = (GameObj_Creature)parameters[0];
            foreach (var weapon in owned.possessedWeapons)
            {
                weapon.currentProjectileDef.ReplaceStat("Piercing", weapon.currentProjectileDef.GetStatValueByName("Piercing") + 1);
            }
        }

        public void Start(XElement possess, object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public void Tick(object[] parameters, float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public void Suspend(object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public string GetName()
        {
            return "IncreasePiercingAmount";
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}