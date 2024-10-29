using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GWBase
{
     [Serializable]
    public class CreatureGroup
    {
        public GameObj_Creature groupLeader = null;
        public GroupDef groupDef = null;
        public List<GameObj_Creature> attachedCreatures = new List<GameObj_Creature>();
        public string groupFaction = "";
        public List<GroupPositionData> emptyPositions = new();
        public GroupPositionData lastUsedPosition;

        public void FreePositionIfDied(HealthInfo info)
        {
            if(!info.gotKilled) {return;}

            var obj = (GameObj_Creature)info.infoOf;
            emptyPositions.Add(obj.possessedPosition);
        }
        
        public void Possess(GroupDef groupDef) {
            this.groupDef = groupDef;
            groupFaction = groupDef.spawnableInfo.faction;
            emptyPositions = groupDef.positions.ToList();
        }

        public void AttachCreature(GameObj_Creature creature) {
            attachedCreatures.Add(creature);

            if(groupLeader != null) {
                int randomIndex = UnityEngine.Random.Range(0, emptyPositions.Count);
                lastUsedPosition = emptyPositions[randomIndex];
                emptyPositions.Remove(lastUsedPosition);
                ((Component)creature).transform.SetParent(((Component)groupLeader).transform);
                creature.SetRigidbodyMode("Kinematic");
                creature.GroupAttached = new GroupMemberReference() {
                    group = this,
                    position = new Vector3(lastUsedPosition.x, lastUsedPosition.y, 0)
                };
                creature.possessedPosition = lastUsedPosition;
                creature.onHealthChange += FreePositionIfDied;
            } else {
                groupLeader = creature;
                creature.GroupAttached = new GroupMemberReference() {
                group = this,
                position = new Vector3()
            };
            }
        }
        
    }

    [Serializable]
    public struct GroupMemberReference {
        public CreatureGroup group;
        public Vector3 position;
    }

}