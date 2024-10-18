using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class XPOrbPlugin : OrbPlugin
    {
        public float orbSize;
        public override async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            await base.Start(possess, parameters, customParameters);
            orbSize = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("OrbSize")).parameterValue, CultureInfo.InvariantCulture);

        }

        public override void DropOrb(GameObj_Creature target)
        {
            base.DropOrb(target);
            CustomParameter xPParameter = new CustomParameter()
            {
                parameterName = "XPValue",
                parameterValue = target.GetPossessed().GetStatValueByName("XPValue").ToString(CultureInfo.InvariantCulture)
            };
            cachedGrabbable.AttachParameter(xPParameter);
            
            var renderer = cachedSpawned.GetComponent<SpriteRenderer>();
            renderer.sprite =
                assetManager.texturesDictionary.FirstOrDefault(x => x.Key == "xpOrb").Value;
            cachedSpawned.transform.localScale *= orbSize;
        }

        public void DropXpOnLocation(Vector3 location, float xpAmount)
        {
            
            base.DropOrbInLocation(location);
            CustomParameter xPParameter = new CustomParameter()
            {
                parameterName = "XPValue",
                parameterValue = xpAmount.ToString(CultureInfo.InvariantCulture)
            };
            cachedGrabbable.AttachParameter(xPParameter);
            
            var renderer = cachedSpawned.GetComponent<SpriteRenderer>();
            renderer.sprite =
                assetManager.texturesDictionary.FirstOrDefault(x => x.Key == "xpOrb").Value;
            cachedSpawned.transform.localScale *= orbSize;
        }



        public override void OnGrab(GameObj by, GameObject targ, IGrabbable which)
        {
            float xpAmount = float.Parse(which.GetParameter("XPValue").parameterValue, CultureInfo.InvariantCulture);
            by.GainXP(xpAmount);
            GameObject.Destroy(targ);
        }
        
        public override string GetName()
        {
            return "XPOrbPlugin";
        }
    }
}