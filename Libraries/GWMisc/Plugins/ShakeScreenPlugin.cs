using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class ShakeScreenPlugin : IObjBehaviour
    {
        public float shakePower;
        public float zPos;
        public float duration;
        public float waitTimeMultiplier;
        public float randomness;
        public Camera targettedCamera;
        public GameObj_Creature attachedObj;

        public string GetName()
        {
            return "ShakeScreen";
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            return;
        }

        public async Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            shakePower = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Power")).parameterValue, CultureInfo.InvariantCulture);
            duration = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Duration")).parameterValue, CultureInfo.InvariantCulture);
            randomness = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Randomness")).parameterValue, CultureInfo.InvariantCulture);
            waitTimeMultiplier = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("WaitTimeMultiplier")).parameterValue, CultureInfo.InvariantCulture);
            targettedCamera = Camera.main;
            await SetupShaking();
        }

        public void Start(XElement possess, object[] parameters)
        {
            return;
        }

        public async Task SetupShaking()
        {
            while (!SettingsManager.settingsManager.loaded || !SettingsManager.settingsManager.playerController.ownedCreature)
            {
                await Task.Delay(500);
            }

            attachedObj = SettingsManager.settingsManager.playerController.ownedCreature;
            attachedObj.onHealthChange.AddListener(TryShake);
        }

        public async void TryShake(HealthInfo healthInfo)
        {
            if (healthInfo.damageTaken > 0)
            {
                await Shake(shakePower, duration);
            }
        }

        public async Task Shake(float power, float duration)
        {
            zPos = targettedCamera.transform.position.z;
            float timePassed = 0;

            while (duration > timePassed)
            {
                timePassed += Time.deltaTime;
                float strength = UnityEngine.Random.Range(Math.Max(shakePower - randomness, 0), shakePower + randomness);
                UnityEngine.Vector3 newPos = attachedObj.transform.position + UnityEngine.Random.insideUnitSphere * strength;
                targettedCamera.transform.position = new UnityEngine.Vector3(newPos.x, newPos.y, zPos);
                await Task.Delay((int)(Time.deltaTime * waitTimeMultiplier));
            }

            targettedCamera.transform.position = new UnityEngine.Vector3(attachedObj.transform.position.x, attachedObj.transform.position.y, zPos);
        }

        public void Suspend(object[] parameters)
        {
            return;
        }

        public void Tick(object[] parameters, float deltaTime)
        {
            return;
        }

        ParameterRequest[] IObjBehaviour.GetParameters()
        {
            return null;
        }
    }
}