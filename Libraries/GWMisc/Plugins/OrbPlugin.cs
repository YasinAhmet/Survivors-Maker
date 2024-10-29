using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class OrbPlugin : IObjBehaviour
    {
        public float locationRandomization;
        public virtual void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            assetManager = AssetManager.assetLibrary;
            locationRandomization = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("LocationRandomization")).parameterValue, CultureInfo.InvariantCulture);

            PlayerManager.newSessionStarted += StartOrbs;
        }

        public void StartOrbs(PlayerManager playerManager)
        {
            GameManager.gameManager.Execute(SubscribeGotKilled());
        }

        public IEnumerator SubscribeGotKilled()
        {
            
            while (PoolManager.poolManager == null)
            {
                yield return new WaitForSecondsRealtime(0.25f); 
            }
            PoolManager.poolManager.creatureGotKilled += DropOrb;
        }

        public async void Start(XElement possess, object[] parameters)
        {
            throw new System.NotImplementedException();
        }
        
        protected AssetManager assetManager;
        protected BaseGrabbable cachedGrabbable;
        protected GameObject cachedSpawned;

        public virtual void DropOrb(GameObj_Creature target)
        {
            if (!assetManager) assetManager = AssetManager.assetLibrary;

            UIManager uiManager = UIManager.uiManager;
            GameObject orbPrefab = PrefabManager.prefabManager.GetPrefabOf("orb");
            cachedSpawned = uiManager.SpawnObjectAtWorldCanvas(orbPrefab);
            cachedSpawned.transform.position = target.transform.position +
                                               new Vector3(Random.Range(-locationRandomization, locationRandomization),
                                                   Random.Range(-locationRandomization, locationRandomization));
            cachedGrabbable = cachedSpawned.AddComponent<BaseGrabbable>();
            cachedGrabbable.onGrabEvent += OnGrab;

        }

        public virtual void DropOrbInLocation(Vector3 location)
        {
            if(!assetManager) assetManager = AssetManager.assetLibrary;
            
            UIManager uiManager = UIManager.uiManager;
            GameObject orbPrefab = PrefabManager.prefabManager.GetPrefabOf("orb");
            cachedSpawned = uiManager.SpawnObjectAtWorldCanvas(orbPrefab);
            cachedSpawned.transform.position = location + new Vector3(Random.Range(-locationRandomization,locationRandomization), Random.Range(-locationRandomization,locationRandomization));
            cachedGrabbable = cachedSpawned.AddComponent<BaseGrabbable>();
            cachedGrabbable.onGrabEvent += OnGrab;
        }

        public virtual void OnGrab(GameObj by, GameObject targ, IGrabbable which)
        {
        }

        public void Tick(object[] parameters, float deltaTime)
        {
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
        }

        public void Suspend(object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public virtual string GetName()
        {
            return "OrbPlugin";
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}