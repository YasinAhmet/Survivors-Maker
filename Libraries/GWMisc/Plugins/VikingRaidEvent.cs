using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class VikingRaidEvent : IObjBehaviour
    {
        public Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            //Debug.Log("[EVENT] Viking Raid!!");
            var groupSpawned = SpawnManager.spawnManager.SpawnGroup("VikingGroup", SpawnManager.spawnManager.GetRandomSpawnPosition());
            foreach (var creature in groupSpawned.attachedCreatures)
            {
                if(groupSpawned.groupLeader == creature) continue;
                creature.RemoveBehaviour("ChasePlayer");
            }
            
            return Task.CompletedTask;
        }

        public void Start(XElement possess, object[] parameters)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public ParameterRequest[] GetParameters()
        {
            throw new System.NotImplementedException();
        }
    }
}