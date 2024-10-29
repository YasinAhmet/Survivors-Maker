using UnityEngine;

namespace GWBase
{
    public interface IGrabbable
    {
        public delegate void Grabbed(GameObj by, GameObject grabbableGameObj, IGrabbable which);
        public event Grabbed onGrabEvent;
        public void GrabRequest(GameObj by);
        public void DropRequest(bool force);
        public void AttachParameter(CustomParameter parameter);
        public CustomParameter GetParameter(string paramName);
    }
}