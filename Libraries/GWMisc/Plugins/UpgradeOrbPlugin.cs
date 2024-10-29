using System.Collections.Generic;
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
        private float _dropChance;
        private float _dropChancePerLevel;
        private List<UpgradeDef> _upgradesCanBeTaken;

        public override void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            _dropChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("DropChance")).parameterValue, CultureInfo.InvariantCulture);
            _dropChancePerLevel = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("DropChancePerLevel")).parameterValue, CultureInfo.InvariantCulture);
            _upgradesCanBeTaken = (from val in AssetManager.assetLibrary.upgradeDefsDictionary where val.Value.onDropUpgrade == "Yes" select val.Value).ToList();
            base.Start(possess, parameters, customParameters);
        }

        public override void DropOrb(GameObj_Creature target)
        {
            float chance = Random.Range(0f, 1);
            float levelChance = PlayerController.playerController.currentLevel.level * _dropChancePerLevel;
            if (chance > (_dropChance + levelChance))
            {
                return;
            }
            
            base.DropOrb(target);
            string randomUpgradeKey = YKUtility.GetRandomElement(_upgradesCanBeTaken).upgradeName;
            var randomUpgradeDef = _upgradesCanBeTaken.FirstOrDefault(x => x.upgradeName == randomUpgradeKey);

            var renderer = cachedSpawned.GetComponent<SpriteRenderer>();
            renderer.sprite =
                assetManager.texturesDictionary.FirstOrDefault(x => x.Key == randomUpgradeDef.renderInfo.imageDefName).Value;
            cachedSpawned.transform.localScale *= randomUpgradeDef.renderInfo.renderSize;
            cachedSpawned.name = randomUpgradeDef.upgradeName;
            
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
            var randomUpgradeDef = _upgradesCanBeTaken.FirstOrDefault(x => x.upgradeName == key);
            by.PossessUpgrades(new []{randomUpgradeDef});
            GameObject.Destroy(targ);
        }
    }
}