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
    public class DashListenerPlugin : IObjBehaviour
    {
        private GameObj_Creature _ownedCreature = null;
        private float _dashCooldown = 0;
        private float _dashCooldownCounter = 0;
        private float _force = 1;
        private float _movementRecovery = 1;
        private float _cachedSpeed = 1;
        private float _cachedDifference = 1;
        private float _cachedLoweredSpeed = 1;
        private bool _doesSlowDown = false;
        private ThingDef _cachedPossessed;
        private bool IsDashReady => _dashCooldownCounter >= _dashCooldown;

        public Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            _dashCooldown = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Cooldown")).parameterValue, CultureInfo.InvariantCulture);
            _force = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Force")).parameterValue, CultureInfo.InvariantCulture);
            _movementRecovery = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("Recovery")).parameterValue, CultureInfo.InvariantCulture);
            _doesSlowDown = customParameters.FirstOrDefault(x => x.parameterName.Equals("Slows")).parameterValue == "Yes";

            _ownedCreature = (GameObj_Creature)parameters[0];

            _ownedCreature.actionRequested += ProcessSignal;
            Debug.Log("[DASH] Setupping Dash for " + _ownedCreature.gameObject.name);
            return Task.CompletedTask;
        }

        private void ProcessSignal(string actionType)
        {
            Debug.Log("[DASH] Got Signal " + actionType);
            if (actionType == "Dash" && IsDashReady)
            {
                Dash();
            }
        }

        private void Dash()
        {
            _dashCooldownCounter = 0;
            _ownedCreature.MoveObject(_ownedCreature.directionLookingAt, _force, true);

            if (_doesSlowDown)
            {
                _cachedPossessed = _ownedCreature.GetPossessed();
                _cachedSpeed = _cachedPossessed.GetStatValueByName("MaxSpeed");
                _cachedLoweredSpeed = _cachedSpeed - (_movementRecovery * _cachedSpeed);
                _cachedDifference = _cachedSpeed - _cachedLoweredSpeed;

                _cachedPossessed.ReplaceStat("MaxSpeed", _cachedLoweredSpeed);
            }
            
            Debug.Log("[DASH] Dashing.. " + _ownedCreature.directionLookingAt*_force);
        }
        
        public void Start(XElement possess, object[] parameters){return;}


        public void RareTick(object[] parameters, float deltaTime)
        {
            _dashCooldownCounter += deltaTime;
            
            if (IsDashReady || _cachedPossessed == null || !_doesSlowDown) return;
            

            float remainingTime = _dashCooldownCounter / _dashCooldown;
            float progress = EaseInBack(remainingTime);
            float total = _cachedLoweredSpeed + (_cachedDifference * progress);
            _cachedPossessed.ReplaceStat("MaxSpeed", total);
            _ownedCreature.cachedMovementSpeed = total;
            //Debug.Log($"Dash Tick: {remainingTime}: {dashCooldownCounter}, {dashCooldown}. {cachedSpeed}, {cachedLoweredSpeed}, {cachedDifference}. {progress} Total:{total}");
        }
        
        private float EaseInBack(float x) {
            var c1 = 1.70158;
            var c3 = c1 + 1;

            return (float)(c3 * x * x * x - c1 * x * x);
        }

        public void Suspend(object[] parameters){return;}

        public void Tick(object[] parameters, float deltaTime){return;}

        ParameterRequest[] IObjBehaviour.GetParameters(){return null;}

        public string GetName(){return "DashListener";}
    }
}