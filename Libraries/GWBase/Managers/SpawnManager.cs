using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Unity.Collections;
using UnityEngine;
namespace GWBase {

public class SpawnManager : Manager
{
    public static SpawnManager spawnManager;
    [SerializeField] public Dictionary<string, ThingDef> spawnableThingsDictionary = new();
    [SerializeField] public Dictionary<string, ThingDef> spawnableEnemiesDictionary = new();
    [SerializeField] public Dictionary<string, GroupDef> spawnableGroupsDictionary = new();

    public float RNP_PointDistance = 5f;
    public float RNP_Max = 1.25f;
    public float RNP_Min = 1f;
    public bool SPW_Activated = true;
    public float SPW_DelayBetweenSpawns = 0.7f;
    public float SPW_FirstDelay = 2f;

    [SerializeField] private string EnemyTeamName = "Hostile";

    public override IEnumerator Kickstart()
    {
        spawnManager = this;
        InitializeSpawnables(AssetManager.assetLibrary.thingDefsDictionary);
        SpawnPlayerGroup();
        StartCoroutine(EnemySpawnTick());
        yield return this;
    }

    public void SpawnPlayerGroup()
    {
        string playerGroupName = SettingsManager.playerSettings.playerGroup;
        GroupDef playerGroup = spawnableGroupsDictionary.FirstOrDefault(x => x.Key.Equals(playerGroupName)).Value;
        CreatureGroup groupInstance = new();
        groupInstance.Possess(playerGroup);

        PlayerController.playerController.ownedGroup = groupInstance;
        groupInstance.groupFaction = playerGroup.spawnableInfo.faction;
        GameObj_Creature leaderObj = null;

        foreach (var member in playerGroup.members)
        {
            ThingDef characterDefToAdd = spawnableThingsDictionary.FirstOrDefault(x => x.Key.Equals(member.defName)).Value;
            GameObj_Creature creature = (GameObj_Creature)PoolManager.poolManager.creaturesPool.ObtainSlotForType(characterDefToAdd, Vector2.zero, 0, playerGroup.spawnableInfo.faction);
            groupInstance.AttachCreature(creature);

            if(member.isLeaderSTR != null && member.isLeaderSTR.Equals("Yes")) {
                GameObject spawnedInputControllerObject = Instantiate(SettingsManager.settingsManager.inputSpeaker, creature.transform);
                PlayerController.playerController.ownedCreature = creature;
                leaderObj = creature;
            }
            
            if(member.extraBehaviours != null && member.extraBehaviours.Count > 0) creature.PossessBehaviours(member.extraBehaviours.ToArray(), false);
        }

        StartCoroutine(PlayerController.playerController.TryFetchCreature());
    }

    /*public void BreakdownNameToThingDef(string defName, out ThingDef thingDef)
    {
        thingDef = spawnableThingsDictionary.FirstOrDefault(x => x.Key.Equals(defName)).Value;
        Debug.Log("BREAKDOWN: " + defName);
    }

    /*public void SpawnPlayer(string byChar)
    {
        BreakdownNameToThingDef(byChar, out ThingDef playerCharDef);
        var player = (GameObj_Creature)PoolManager.poolManager.creaturesPool.HardSet(PrefabManager.prefabManager.GetPrefabOf("player"), playerCharDef, Vector2.zero, 0, PlayerTeamName);
        PlayerController.playerController.ownedCreature = player;
        StartCoroutine(PlayerController.playerController.TryFetchCreature());
    }*/

    public IEnumerator EnemySpawnTick()
    {
        yield return new WaitForSeconds(SPW_FirstDelay);

        while (SPW_Activated)
        {
            PoolManager.poolManager.creaturesPool.ObtainSlotForType(GetRandomEnemy(), (Vector2)GetRandomSpawnPosition(), 0, EnemyTeamName);
            yield return new WaitForSeconds(SPW_DelayBetweenSpawns);
        }
    }

    public void InitializeSpawnables(Dictionary<string, XElement> definitionsList)
    {
        foreach (var definition in definitionsList)
        {
            var definitionType = definition.Value.Element("spawnable")?.Element("faction");
            if (definitionType == null) return;
            switch (definition.Value.Element("spawnable")?.Attribute("Type").Value)
            {
                case "group":
                    InitializeGroupDef(definition);
                    break;
                case "creature":
                    InitializeCreatureDef(definition);
                    break;
            }
        }
    }

    public void InitializeCreatureDef(KeyValuePair<string, XElement> definition)
    {
        var initializedDef = YKUtility.FromXElement<ThingDef>(definition.Value);

        if (initializedDef.spawnable.faction.Equals(EnemyTeamName))
        {
            spawnableEnemiesDictionary.Add(definition.Key, initializedDef);
        }
        spawnableThingsDictionary.Add(definition.Key, initializedDef);
    }

    public void InitializeGroupDef(KeyValuePair<string, XElement> definition)
    {
        var initializedDef = YKUtility.FromXElement<GroupDef>(definition.Value);
        spawnableGroupsDictionary.Add(definition.Key, initializedDef);
    }

    public ThingDef GetRandomEnemy()
    {
        return spawnableEnemiesDictionary[YKUtility.GetRandomIndex<ThingDef>(spawnableEnemiesDictionary)];
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = new(UnityEngine.Random.Range(RNP_Min, RNP_Max), UnityEngine.Random.Range(RNP_Min, RNP_Max));
        if (UnityEngine.Random.value > 0.5f) randomPoint = randomPoint * -1;
        randomPoint.z = RNP_PointDistance;
        Vector3 worldPoint = Camera.main.ViewportToWorldPoint(randomPoint);
        return worldPoint;
    }

}

}