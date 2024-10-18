using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class Prop : MonoBehaviour, IGrabbable
    {
        public float xpToGiveOnBreak = 10;
        public void GrabRequest(GameObj by)
        {
            gameObject.SetActive(false);

            foreach (var behaviour in SettingsManager.settingsManager.loadedUpBehaviours)
            {
                if (behaviour.GetName() == "XPOrbPlugin")
                {
                    XPOrbPlugin xpOrbPlugin = (XPOrbPlugin)behaviour;
                    xpOrbPlugin.DropXpOnLocation(transform.position, xpToGiveOnBreak);
                }
            }
        }

        public void DropRequest(bool force)
        {
            return;
        }

        public void AttachParameter(CustomParameter parameter)
        {
            return;
        }

        public CustomParameter GetParameter(string paramName)
        {
            return new CustomParameter();
        }

        public event IGrabbable.Grabbed onGrabEvent;
    }
}