using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GWBase;

namespace GWMisc
{
    public class TensionByTimePlugin : IObjBehaviour
    {
        public float tensionByLevel;
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            tensionByLevel = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("PerLevelIncrease")).parameterValue, CultureInfo.InvariantCulture);
            PlayerController.playerController.onLevelUp += IncreaseDifficulty;
        }
        public void IncreaseDifficulty(PlayerController.LevelInfo levelInfo)
        {
            var targetMap = SpawnManager.spawnManager.currentMap;
            targetMap.spawnSpeed -= tensionByLevel;
        }

        public void Tick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void Suspend(object[] parameters)
        {
            return;
        }

        public string GetName()
        {
            return "TensionByTimePlugin";
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}