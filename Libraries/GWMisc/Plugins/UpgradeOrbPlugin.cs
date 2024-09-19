using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class UpgradeOrbPlugin : OrbPlugin
    {
        public float dropChance;
        public float dropChancePerLevel;
        public override async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            dropChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("DropChance")).parameterValue, CultureInfo.InvariantCulture);
            dropChancePerLevel = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("DropChancePerLevel")).parameterValue, CultureInfo.InvariantCulture);
            await base.Start(possess, parameters, customParameters);
        }

        public override void DropOrb(GameObj_Creature target)
        {
            float chance = Random.Range(0f, 1);
            float levelChance = PlayerController.playerController.currentLevel.level * dropChancePerLevel;
            if (chance > (dropChance + levelChance))
            {
                return;
            }
            
            base.DropOrb(target);
            string randomUpgradeKey = YKUtility.GetRandomIndex(assetManager.upgradeDefsDictionary);
            var randomUpgradeDef =
                assetManager.upgradeDefsDictionary.FirstOrDefault(x => x.Key == randomUpgradeKey).Value;

            var renderer = cachedSpawned.GetComponent<SpriteRenderer>();
            renderer.sprite =
                assetManager.texturesDictionary.FirstOrDefault(x => x.Key == randomUpgradeDef.renderInfo.imageDefName).Value;
            cachedSpawned.transform.localScale *= randomUpgradeDef.renderInfo.renderSize;
            Debug.Log($"Upgrade render def name: {randomUpgradeDef.renderInfo.imageDefName}, and def itself {randomUpgradeKey}, def found {randomUpgradeDef.upgradeName}");
            
            CustomParameter xPParameter = new CustomParameter()
            {
                parameterName = "UpgradeIndex",
                parameterValue = randomUpgradeKey
            };
            cachedGrabbable.AttachParameter(xPParameter);
        }

        public override void OnGrab(GameObj by, GameObject targ, IGrabbable which)
        {
            string key = which.GetParameter("UpgradeIndex").parameterValue;
            var randomUpgradeDef =
                assetManager.upgradeDefsDictionary.FirstOrDefault(x => x.Key == key).Value;
            by.PossessUpgrades(new []{randomUpgradeDef});
            GameObject.Destroy(targ);
        }
    }
}