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
using Random = System.Random;

namespace GWBase {

public class SpawnManager : Manager
{
    public Camera playerCamera;
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
    public Map currentMap;

    [SerializeField] private string EnemyTeamName = "Hostile";

    public override IEnumerator Kickstart()
    {
        spawnManager = this;
        InitializeSpawnables(AssetManager.assetLibrary.thingDefsDictionary);
        SpawnPlayerGroup();
        
        var defFile = XElement.Load(AssetManager.assetLibrary.fullPathToGameDatabase + "Maps.xml");
        currentMap = YKUtility.FromXElement<Map>(defFile.Element("Map"));
        currentMap.Load();
        StartCoroutine(EnemySpawnTick());
        playerCamera = Camera.main;
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
            GameObj_Creature creature = (GameObj_Creature)PoolManager.poolManager.GetObjectPool("Creatures").ObtainSlotForType(characterDefToAdd, Vector2.zero, 0, playerGroup.spawnableInfo.faction);
            groupInstance.AttachCreature(creature);

            if(member.isLeaderSTR != null && member.isLeaderSTR.Equals("Yes")) {
                GameObject spawnedInputControllerObject = Instantiate(SettingsManager.settingsManager.inputSpeaker, ((Component)creature).transform);
                PlayerController.playerController.ownedCreature = creature;
                leaderObj = creature;
                creature.pickupManager.autoGrab = true;
            }
            
            if(member.extraBehaviours != null && member.extraBehaviours.Count > 0) creature.PossessBehaviours(member.extraBehaviours.ToArray(), false);
        }

        StartCoroutine(PlayerController.playerController.TryFetchCreature());
    }
    
    public CreatureGroup SpawnGroup(string groupName, Vector2 location)
    {
        GroupDef group = spawnableGroupsDictionary.FirstOrDefault(x => x.Key.Equals(groupName)).Value;
        CreatureGroup groupInstance = new();
        groupInstance.Possess(group);

        groupInstance.groupFaction = group.spawnableInfo.faction;
        GameObj_Creature leaderObj = null;

        foreach (var member in group.members)
        {
            ThingDef characterDefToAdd = spawnableThingsDictionary.FirstOrDefault(x => x.Key.Equals(member.defName)).Value;
            GameObj_Creature creature = (GameObj_Creature)PoolManager.poolManager.GetObjectPool("Creatures").ObtainSlotForType(characterDefToAdd, location, 0, group.spawnableInfo.faction);
            groupInstance.AttachCreature(creature);

            if(member.isLeaderSTR != null && member.isLeaderSTR.Equals("Yes")) {
                leaderObj = creature;
                //creature.pickupManager.autoGrab = true;
            }
            
            if(member.extraBehaviours != null && member.extraBehaviours.Count > 0) creature.PossessBehaviours(member.extraBehaviours.ToArray(), false);
        }

        return groupInstance;
    }
    
    public IEnumerator EnemySpawnTick()
    {
        yield return new WaitForSeconds(SPW_FirstDelay);

        while (SPW_Activated && SettingsManager.playerSettings.shouldSpawn == "Yes")
        {
            GameObj slot = PoolManager.poolManager.GetObjectPool("Creatures").ObtainSlotForType(GetRandomEnemy(), (Vector2)GetRandomSpawnPosition(), 0, EnemyTeamName);
            UIManager.uiManager.AttachObjectToWorldCanvas(slot.gameObject);
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
        var entity = currentMap.GetRandomEntity(currentMap.cachedSpawnablesPack);
        foreach (var spawnable in spawnableEnemiesDictionary)
        {
            if (spawnable.Key == entity.defName) return spawnable.Value;
        }

        return null;
    }

    public Vector3 GetRandomSpawnPosition()
    {
        var camPos = playerCamera.transform.position;
        if (UnityEngine.Random.Range(0, 2) == 1)
        {
            if(YKUtility.Random)return playerCamera.ScreenToWorldPoint(GetRandomHorizontalSpawnPosition() + camPos);
            else return playerCamera.ScreenToWorldPoint(-GetRandomHorizontalSpawnPosition() + camPos);
        }
        else
        {
            if(YKUtility.Random)return playerCamera.ScreenToWorldPoint(GetRandomVerticalSpawnPosition() + camPos);
            else return playerCamera.ScreenToWorldPoint(-GetRandomVerticalSpawnPosition() + camPos);
        }
        
        /*Vector3 randomPoint = new(UnityEngine.Random.Range(RNP_Min, RNP_Max), UnityEngine.Random.Range(RNP_Min, RNP_Max));
        if (UnityEngine.Random.value > 0.5f) randomPoint = randomPoint * -1;
        randomPoint.z = RNP_PointDistance;
        Vector3 worldPoint = Camera.main.ViewportToWorldPoint(randomPoint);
        return worldPoint;*/
    }

    public Vector3 GetRandomVerticalSpawnPosition()
    {
        float verticalBoundPos = Screen.width*1.5f;
        float randomHorizontalPos = UnityEngine.Random.Range(0, Screen.height);

        if (YKUtility.Random) randomHorizontalPos = -randomHorizontalPos;
        if (YKUtility.Random) verticalBoundPos = -verticalBoundPos;

        return new Vector3(verticalBoundPos, randomHorizontalPos);
    }
    
    public Vector3 GetRandomHorizontalSpawnPosition()
    {
        float verticalBoundPos = UnityEngine.Random.Range(0, Screen.width);
        float randomHorizontalPos = Screen.height*1.5f;

        if (YKUtility.Random) randomHorizontalPos = -randomHorizontalPos;
        if (YKUtility.Random) verticalBoundPos = -verticalBoundPos;

        return new Vector3(verticalBoundPos, randomHorizontalPos);
    }

    private void FixedUpdate()
    {
        if(currentMap != null)currentMap.RareTick(Time.fixedDeltaTime);
    }
}

}