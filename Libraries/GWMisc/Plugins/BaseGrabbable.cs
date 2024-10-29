using System.Collections.Generic;
using System.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class BaseGrabbable : MonoBehaviour, IGrabbable
    {
        private List<CustomParameter> parameters = new ();
        public void GrabRequest(GameObj by)
        {
            onGrabEvent.Invoke(by, gameObject, this);
        }

        public void DropRequest(bool force)
        {
            throw new System.NotImplementedException();
        }

        public void AttachParameter(CustomParameter parameter)
        {
            parameters.Add(parameter);
        }

        public CustomParameter GetParameter(string paramName)
        {
            return parameters.FirstOrDefault<CustomParameter>(x => x.parameterName == paramName);
        }

        public event IGrabbable.Grabbed onGrabEvent;
    }
}