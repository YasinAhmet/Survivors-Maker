using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GWBase;
using UnityEngine;

public class Hack : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            GameObj_Creature owned = PlayerController.playerController.ownedCreature;
            CreatureGroup group = owned.groupAttached.group;

            var characterDefToAdd =
                SpawnManager.spawnManager.spawnableThingsDictionary.FirstOrDefault(x => x.Key == "SpaceMarine").Value;
            GameObj_Creature creature = (GameObj_Creature)PoolManager.poolManager.GetObjectPool("Creatures").ObtainSlotForType(characterDefToAdd, UnityEngine.Vector2.zero, 0, group.groupFaction);
            group.AttachCreature(creature);
            creature.transform.localPosition = new UnityEngine.Vector3(group.lastUsedPosition.x,group.lastUsedPosition.y, 0);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            float xpAmount = 1000;
            PlayerController.playerController.GainXP(xpAmount);
        }
    }
}
