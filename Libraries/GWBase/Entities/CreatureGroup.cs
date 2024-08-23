using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GWBase
{
    public class CreatureGroup
    {
        public GameObj_Creature groupLeader = null;
        public GroupDef groupDef = null;
        public List<GameObj_Creature> attachedCreatures = new List<GameObj_Creature>();
        public string groupFaction = "";
        public List<GroupPositionData> emptyPositions = new();

        public void Possess(GroupDef groupDef) {
            this.groupDef = groupDef;
            groupFaction = groupDef.spawnableInfo.faction;
            emptyPositions = groupDef.positions.ToList();
        }

        public void AttachCreature(GameObj_Creature creature) {
            attachedCreatures.Add(creature);
            int randomIndex = Random.Range(0, emptyPositions.Count);
            var position = emptyPositions[randomIndex];
            creature.transform.position = creature.transform.position + new Vector3(position.x, position.y, 0);
            emptyPositions.Remove(position);

            if(groupLeader != null) {
                creature.transform.SetParent(groupLeader.transform);
            }
        }
        
    }

}