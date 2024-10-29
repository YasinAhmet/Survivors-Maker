using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class FpsLimitPlugin : IObjBehaviour
    
    {
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            QualitySettings.vSyncCount = 0;
            float fps = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("TargetFPS")).parameterValue, CultureInfo.InvariantCulture);
            Application.targetFrameRate = (int)fps;
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
            return "FpsLimitPlugin";
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}